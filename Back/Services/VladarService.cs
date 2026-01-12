using Neo4jClient;
using MongoDB.Driver;

public class VladarService : IVladarService
{
    private readonly IGraphClient _client;
    private readonly IMongoCollection<VladarMongo> _vladarCollection;

    public VladarService(IGraphClient client, IMongoCollection<VladarMongo> vladarCollection)
    {
        _client = client;
        _vladarCollection = vladarCollection;
    }

    public async Task<List<LicnostFlatDto>> GetVladariFlat(Guid dinastijaId)
    {
        var vladari = (await _client.Cypher.Match("(v:Licnost:Vladar)-[:PRIPADA_DINASTIJI]->(d:Dinastija { ID: $id })")
                                          .WithParam("id", dinastijaId)
                                          .OptionalMatch("(v)-[r3:JE_RODITELJ]->(dete:Licnost)")
                                          .OptionalMatch("(v)<-[r4:JE_RODITELJ]-(rod:Licnost)")
                                          //.OptionalMatch("(v)-[r6:PRIPADA_DINASTIJI]->(d:Dinastija)")
                                          .With("v, collect(DISTINCT rod.ID) as roditeljiID, collect(DISTINCT dete.ID) as decaID")
                                          .Return((v, decaID, roditeljiID) => new
                                          {
                                              Vladar = v.As<VladarNeo>(),
                                              //Dinastija = d.As<DinastijaNeo>(),
                                              DecaID = decaID.As<List<Guid>>(),
                                              RoditeljiID = roditeljiID.As<List<Guid>>()
                                          })
                                          .ResultsAsync)
                                          .ToList();

            var ids = vladari.Select(v => v.Vladar.ID).ToList();
            var mongoList = await _vladarCollection.Find(m => ids.Contains(m.ID)).ToListAsync();

            return vladari.Select(vl =>
            {
                var mongo = mongoList.FirstOrDefault(m => m.ID == vl.Vladar.ID);
                return new LicnostFlatDto
                {
                    ID = vl.Vladar.ID,
                    Titula = vl.Vladar.Titula,
                    Ime = vl.Vladar.Ime,
                    Prezime = vl.Vladar.Prezime,
                    GodinaRodjenja = vl.Vladar.GodinaRodjenja,
                    GodinaRodjenjaPNE = vl.Vladar.GodinaRodjenjaPNE,
                    GodinaSmrti = vl.Vladar.GodinaSmrti,
                    GodinaSmrtiPNE = vl.Vladar.GodinaSmrtiPNE,
                    Pol = vl.Vladar.Pol,
                    MestoRodjenja = vl.Vladar.MestoRodjenja,
                    Tekst = mongo?.Tekst,
                    Slika = mongo?.Slika,
                    DecaID = vl.DecaID,
                    RoditeljiID = vl.RoditeljiID
                };
            }).ToList();
    }
}