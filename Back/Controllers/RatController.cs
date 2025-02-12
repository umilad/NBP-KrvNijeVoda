using Microsoft.AspNetCore.Mvc;
using Neo4jClient;
using System;
using System.Threading.Tasks;
using KrvNijeVoda.Back.Models;
using System.Reflection.Metadata;
using KrvNijeVoda.Back;
[Route("api")]
[ApiController]
public class RatController : ControllerBase
{
    private readonly IGraphClient _client;

    public RatController(Neo4jService neo4jService)
    {
        _client = neo4jService.GetClient();  
    }


    // [HttpPost("CreateRat")]
    // public async Task<IActionResult> CreateRat([FromBody] Dinastija dinastija)
    // {    
    //     await _client.Cypher.Create("(d:Dinastija $din)")
    //                             .WithParam("din", dinastija)
    //                             .ExecuteWithoutResultsAsync();
    //     return Ok();
    // }

    // [HttpGet("GetRat/{id}")]
    // public async Task<IActionResult> GetRat(Guid id)
    // {
    //     var din = await _client.Cypher.Match("(d:Dinastija)")
    //                                 .Where((Dinastija d) => d.ID == id)
    //                                 .Return(d => d.As<Dinastija>())
    //                                 .ResultsAsync;
    //     return Ok(din.LastOrDefault());
    // }

    // [HttpPut("UpdateRat/{id}")]
    // public async Task<IActionResult> UpdateRat([FromBody] Dinastija dinastija, Guid id)
    // {
    //     await _client.Cypher.Match("(d:Dinastija)")
    //                                 .Where((Dinastija d) => d.ID == id)
    //                                 .Set("d = $dinastija")
    //                                 .WithParam("dinastija", dinastija)
    //                                 .ExecuteWithoutResultsAsync();
    //     return Ok();
    // }

    // [HttpDelete("DeleteRat/{id}")]
    // public async Task<IActionResult> DeleteRat(Guid id)
    // {
    //     await _client.Cypher.Match("(d:Dinastija)")
    //                         .Where((Dinastija d) => d.ID == id)
    //                         .Delete("d")
    //                         .ExecuteWithoutResultsAsync();
    //     return Ok();
    // }

}