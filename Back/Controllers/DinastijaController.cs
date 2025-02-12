using Microsoft.AspNetCore.Mvc;
//using Neo4j.Driver;
using Neo4jClient;
using System;
using System.Threading.Tasks;
using KrvNijeVoda.Back.Models;
using System.Reflection.Metadata;
using KrvNijeVoda.Back;
[Route("api")]
[ApiController]
public class DinastijaController : ControllerBase
{
    private readonly IGraphClient _client;

    // Constructor: Injecting Neo4jService and getting the client
    public DinastijaController(Neo4jService neo4jService)
    {
        _client = neo4jService.GetClient();  // Get the Neo4jClient
    }


[HttpPost("CreateDinastija")]
public async Task<IActionResult> CreateDinastija([FromBody] Dinastija dinastija)
{
    // await _client.Cypher.Create("(d:Dinastija $din)")
    //                     .WithParam("din", dinastija)
    //                     .ExecuteAsync();
    
    await _client.Cypher.Create("(d:Dinastija $din)")
                             .WithParam("din", dinastija)
                             .ExecuteWithoutResultsAsync();
    return Ok();
}

[HttpGet("GetDinastija/{id}")]
public async Task<IActionResult> GetDinastija(Guid id)
{
    var din = await _client.Cypher.Match("(d:Dinastija)")
                                  .Where((Dinastija d) => d.ID == id)
                                  .Return(d => d.As<Dinastija>())
                                  .ResultsAsync;
    return Ok(din.LastOrDefault());
}

[HttpPut("UpdateDinastija/{id}")]
public async Task<IActionResult> UpdateDinastija([FromBody] Dinastija dinastija, Guid id)
{
    await _client.Cypher.Match("(d:Dinastija)")
                                  .Where((Dinastija d) => d.ID == id)
                                  .Set("d = $dinastija")
                                  .WithParam("dinastija", dinastija)
                                  .ExecuteWithoutResultsAsync();
    return Ok();
}

[HttpDelete("DeleteDinastija/{id}")]
public async Task<IActionResult> DeleteDinastija(Guid id)
{
    await _client.Cypher.Match("(d:Dinastija)")
                        .Where((Dinastija d) => d.ID == id)
                        .Delete("d")
                        .ExecuteWithoutResultsAsync();
    return Ok();
}

}