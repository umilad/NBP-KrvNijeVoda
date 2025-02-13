using Neo4jClient;
using KrvNijeVoda.Back.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http.HttpResults;
public class ZemljaService
{
    private readonly IGraphClient _client;

    public ZemljaService(Neo4jService neo4jService)
    {
        _client = neo4jService.GetClient();
    }

    public async Task<Zemlja> DodajZemlju(Zemlja zem)
    {    
        var nz = (await _client.Cypher.Match("(z:Lokacija:Zemlja)")
                                      .Where((Zemlja z) => z.Naziv == zem.Naziv) 
                                      .Return(z => z.As<Zemlja>())
                                      .ResultsAsync)
                                      .FirstOrDefault();
        if(nz == null)
        {
            zem.ID = Guid.NewGuid();
            await _client.Cypher.Create("(z:Lokacija:Zemlja $zemlja)")
                            .WithParam("zemlja", zem)
                            .Return(g => g.As<Godina>())
                            .ResultsAsync;
        }
        return nz!;
    }
}
