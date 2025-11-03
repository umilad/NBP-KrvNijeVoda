using Neo4jClient;
//using KrvNijeVoda.Back.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http.HttpResults;
public class ZemljaService
{
    private readonly IGraphClient _client;

    public ZemljaService(Neo4jService neo4jService)
    {
        _client = neo4jService.GetClient();
    }

    //VRV NISTA NE VALJA JER JE SAD SAMO ZEMLJA NEMA LOKACIJA

    // public async Task<Zemlja> DodajZemlju(Zemlja zem)
    // {    
    //     var nz = (await _client.Cypher.Match("(z:Zemlja)")
    //                                   .Where((Zemlja z) => z.Naziv == zem.Naziv) 
    //                                   .Return(z => z.As<Zemlja>())
    //                                   .ResultsAsync)
    //                                   .FirstOrDefault();
    //     if(nz == null)
    //     {
    //         zem.ID = Guid.NewGuid();
    //         nz = (await _client.Cypher.Create("(z:Zemlja $zemlja)")
    //                         .WithParam("zemlja", zem)
    //                         .Return(z => z.As<Zemlja>())
    //                         .ResultsAsync)
    //                         .FirstOrDefault();//dodaj mozda kojoj zemlji pripada da vraca 
    //     }
    //     return nz!;
    // }
    // public async Task<Zemlja> DodajZemljuParametri(string naziv, string? grb, string? trajanje)
    // {    

    //     var nz = (await _client.Cypher.Match("(z:Zemlja)")
    //                                   .Where((Zemlja z) => z.Naziv == naziv) 
    //                                   .Return(z => z.As<Zemlja>())
    //                                   .ResultsAsync)
    //                                   .FirstOrDefault();
    //     if(nz == null)
    //     {
    //         var zem = new Zemlja {
    //             ID = Guid.NewGuid(),
    //             Naziv = naziv,
    //             Grb = grb,
    //             Trajanje = trajanje
    //         };
    //         nz = (await _client.Cypher.Create("(z:Zemlja $zemlja)")
    //                                   .WithParam("zemlja", zem)
    //                                   .Return(z => z.As<Zemlja>())
    //                                   .ResultsAsync)
    //                                   .FirstOrDefault();
    //     }
    //     return nz!;
    // }
}
