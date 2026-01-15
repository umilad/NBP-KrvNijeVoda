using Neo4jClient;
using MongoDB.Driver;
public class LicnostService : ILicnostService
{
    private readonly IGraphClient _client;
    private readonly IMongoCollection<LicnostMongo> _licnostCollection;

    public LicnostService(IGraphClient client, IMongoCollection<LicnostMongo> licnostCollection)
    {
        _client = client;
        _licnostCollection = licnostCollection;
    }

    public async Task<List<LicnostFlatDto>> GetLicnostiFlat(Guid dinastijaId)
    {
        var licnosti = (await _client.Cypher.Match("(l:Licnost)-[:PRIPADA_DINASTIJI]->(d:Dinastija { ID: $id })")
                                          .Where("NOT l:Vladar")
                                          .WithParam("id", dinastijaId)
                                          .OptionalMatch("(l)-[r3:JE_RODITELJ]->(dete:Licnost)")
                                          .OptionalMatch("(l)<-[r4:JE_RODITELJ]-(rod:Licnost)")
                                          .With("l, collect(DISTINCT rod.ID) as roditeljiID, collect(DISTINCT dete.ID) as decaID")
                                          .Return((l, decaID, roditeljiID) => new
                                          {
                                              Licnost = l.As<LicnostNeo>(),
                                              DecaID = decaID.As<List<Guid>>(),
                                              RoditeljiID = roditeljiID.As<List<Guid>>()
                                          })
                                          .ResultsAsync)
                                          .ToList();

            var ids = licnosti.Select(l => l.Licnost.ID).ToList();
            var mongoList = await _licnostCollection.Find(m => ids.Contains(m.ID)).ToListAsync();

            return licnosti.Select(lic =>
            {
                var mongo = mongoList.FirstOrDefault(m => m.ID == lic.Licnost.ID);
                return new LicnostFlatDto
                {
                    ID = lic.Licnost.ID,
                    Titula = lic.Licnost.Titula,
                    Ime = lic.Licnost.Ime,
                    Prezime = lic.Licnost.Prezime,
                    GodinaRodjenja = lic.Licnost.GodinaRodjenja,
                    GodinaRodjenjaPNE = lic.Licnost.GodinaRodjenjaPNE,
                    GodinaSmrti =lic.Licnost.GodinaSmrti,
                    GodinaSmrtiPNE = lic.Licnost.GodinaSmrtiPNE,
                    Pol = lic.Licnost.Pol,
                    MestoRodjenja = lic.Licnost.MestoRodjenja,
                    Tekst = mongo?.Tekst,
                    Slika = mongo?.Slika,
                    DecaID = lic.DecaID,
                    RoditeljiID = lic.RoditeljiID
                };
            }).ToList();
    }
}