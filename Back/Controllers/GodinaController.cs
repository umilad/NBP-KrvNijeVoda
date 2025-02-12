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


    // [HttpPost("CreateGodina")]
    // public async Task<IActionResult> CreateGodina([FromBody] Dinastija dinastija)
    // {    
    //     await _client.Cypher.Create("(d:Dinastija $din)")
    //                             .WithParam("din", dinastija)
    //                             .ExecuteWithoutResultsAsync();
    //     return Ok();
    // }

    // [HttpGet("GetGodina/{id}")]
    // public async Task<IActionResult> GetGodina(Guid id)
    // {
    //     var din = await _client.Cypher.Match("(d:Dinastija)")
    //                                 .Where((Dinastija d) => d.ID == id)
    //                                 .Return(d => d.As<Dinastija>())
    //                                 .ResultsAsync;
    //     return Ok(din.LastOrDefault());
    // }

    // [HttpPut("UpdateGodina/{id}")]
    // public async Task<IActionResult> UpdateGodina([FromBody] Dinastija dinastija, Guid id)
    // {
    //     await _client.Cypher.Match("(d:Dinastija)")
    //                                 .Where((Dinastija d) => d.ID == id)
    //                                 .Set("d = $dinastija")
    //                                 .WithParam("dinastija", dinastija)
    //                                 .ExecuteWithoutResultsAsync();
    //     return Ok();
    // }

    // [HttpDelete("DeleteGodina/{id}")]
    // public async Task<IActionResult> DeleteGodina(Guid id)
    // {
    //     await _client.Cypher.Match("(d:Dinastija)")
    //                         .Where((Dinastija d) => d.ID == id)
    //                         .Delete("d")
    //                         .ExecuteWithoutResultsAsync();
    //     return Ok();
    // }

}