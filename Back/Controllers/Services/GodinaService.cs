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
}
