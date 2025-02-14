using Microsoft.AspNetCore.Mvc;
using Neo4jClient;
using System;
using System.Threading.Tasks;
using KrvNijeVoda.Back.Models;
using System.Reflection.Metadata;
using KrvNijeVoda.Back;
[Route("api")]
[ApiController]
public class GodinaController : ControllerBase
{
    private readonly IGraphClient _client;

    public GodinaController(Neo4jService neo4jService)
    {
        _client = neo4jService.GetClient();  
    }


    [HttpPost("CreateGodina")]
    public async Task<IActionResult> CreateGodina([FromBody] Godina godina)
    {    
        var god = (await _client.Cypher.Match("(g:Godina)")
                                      .Where((Godina g) => g.God == godina.God) // Match by the start year
                                      .Return(g => g.As<Godina>())
                                      .ResultsAsync)
                                      .FirstOrDefault();
        if(god != null)
        {
            return BadRequest($"Godina {god.God}. vec postoji u bazi!");
        }
        await _client.Cypher.Create("(g:Godina {ID: $id, God: $god})")
                                .WithParam("god", godina.God)
                                .WithParam("id", Guid.NewGuid())
                                .ExecuteWithoutResultsAsync();
        return Ok($"Godina {godina.God}. je uspesno dodata u bazu!");
    }

    


}