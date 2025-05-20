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

    //MORA DA SE DODA ZA P.N.E. ----dodav treba da se testira

    public async Task DodajGodinu(int go, bool pne)
    {    
        var ng = (await _client.Cypher.Match("(g:Godina)")
                                      .Where((Godina g) => g.God == go && g.IsPNE == pne) 
                                      .Return(g => g.As<Godina>())
                                      .ResultsAsync)
                                      .FirstOrDefault();
        if(ng == null)
        {
            await _client.Cypher.Create("(g:Godina {ID: $id, God: $god, IsPNE: $ispne})")
                            .WithParam("god", go)
                            .WithParam("ispne", pne)
                            .WithParam("id", Guid.NewGuid())
                            .Return(g => g.As<Godina>())
                            .ResultsAsync;
        }
    }
}
