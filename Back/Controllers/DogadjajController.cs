using Microsoft.AspNetCore.Mvc;
using Neo4jClient;
using System;
using System.Threading.Tasks;
using KrvNijeVoda.Back.Models;
using System.Reflection.Metadata;
using KrvNijeVoda.Back;
[Route("api")]
[ApiController]
public class DogadjajController : ControllerBase
{
    private readonly IGraphClient _client;

    public DogadjajController(Neo4jService neo4jService)
    {
        _client = neo4jService.GetClient();  
    }


    // [HttpPost("CreateDogadjaj")]
    // public async Task<IActionResult> CreateDogadjaj([FromBody] Dinastija dinastija)
    // {    
    //     await _client.Cypher.Create("(d:Dinastija $din)")
    //                             .WithParam("din", dinastija)
    //                             .ExecuteWithoutResultsAsync();
    //     return Ok();
    // }

    // [HttpGet("GetDogadjaj/{id}")]
    // public async Task<IActionResult> GetDogadjaj(Guid id)
    // {
    //     var din = await _client.Cypher.Match("(d:Dinastija)")
    //                                 .Where((Dinastija d) => d.ID == id)
    //                                 .Return(d => d.As<Dinastija>())
    //                                 .ResultsAsync;
    //     return Ok(din.LastOrDefault());
    // }

    // [HttpPut("UpdateDogadjaj/{id}")]
    // public async Task<IActionResult> UpdateDogadjaj([FromBody] Dinastija dinastija, Guid id)
    // {
    //     await _client.Cypher.Match("(d:Dinastija)")
    //                                 .Where((Dinastija d) => d.ID == id)
    //                                 .Set("d = $dinastija")
    //                                 .WithParam("dinastija", dinastija)
    //                                 .ExecuteWithoutResultsAsync();
    //     return Ok();
    // }

    // [HttpDelete("DeleteDogadjaj/{id}")]
    // public async Task<IActionResult> DeleteDogadjaj(Guid id)
    // {
    //     await _client.Cypher.Match("(d:Dinastija)")
    //                         .Where((Dinastija d) => d.ID == id)
    //                         .Delete("d")
    //                         .ExecuteWithoutResultsAsync();
    //     return Ok();
    // }

}