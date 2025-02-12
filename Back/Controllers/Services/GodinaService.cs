using Neo4jClient;
using KrvNijeVoda.Back.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http.HttpResults;
public class GodinaService
{
    private readonly IGraphClient _client;

    public GodinaService(Neo4jService neo4jService)
    {
        _client = neo4jService.GetClient();
    }

    public async Task DodajGodinu(int go)
    {    
        var ng = (await _client.Cypher.Match("(g:Godina)")
                                      .Where((Godina g) => g.God == go) 
                                      .Return(g => g.As<Godina>())
                                      .ResultsAsync)
                                      .FirstOrDefault();
        if(ng == null)
        {
            await _client.Cypher.Create("(g:Godina {ID: $id, God: $god})")
                            .WithParam("god", go)
                            .WithParam("id", Guid.NewGuid())
                            .Return(g => g.As<Godina>())
                            .ResultsAsync;
        }
    }

    // public async Task<Godina> DodajGodinu(int go)
    // {    
    //     var ng = (await _client.Cypher.Create("(g:Godina {ID: $id, God: $god})")
    //                             .WithParam("god", go)
    //                             .WithParam("id", Guid.NewGuid())
    //                             .Return(g => g.As<Godina>())
    //                             .ResultsAsync)
    //                             .FirstOrDefault();
    //     return ng!;
    // }

    // public async Task<Godina> CreateGodinaIfNotExists(int god)
    // {
    //     var existingGodina = (await _client.Cypher
    //         .Match("(g:Godina)")
    //         .Where((Godina g) => g.God == god)
    //         .Return(g => g.As<Godina>())
    //         .ResultsAsync)
    //         .FirstOrDefault();

    //     if (existingGodina != null)
    //     {
    //         return existingGodina; // Return if it already exists
    //     }

    //     var newGodina = new Godina { ID = Guid.NewGuid(), God = god };

    //     await _client.Cypher
    //         .Create("(g:Godina {ID: $id, God: $god})")
    //         .WithParam("id", newGodina.ID)
    //         .WithParam("god", newGodina.God)
    //         .ExecuteWithoutResultsAsync();

    //     return newGodina;
    // }
}
