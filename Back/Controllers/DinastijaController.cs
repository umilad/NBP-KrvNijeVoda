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


// [HttpPost("CreateDinastija")]
// public async Task<IActionResult> CreateDinastija([FromBody] Dinastija dinastija)
// {
//     // await _client.Cypher.Create("(d:Dinastija $din)")
//     //                     .WithParam("din", dinastija)
//     //                     .ExecuteAsync();
    
//     await _client.Cypher.Create("(d:Dinastija $din)")
//                              .WithParam("din", dinastija)
//                              .ExecuteWithoutResultsAsync();
//     return Ok();
// }

[HttpPost("CreateDinastija")]
public async Task<IActionResult> CreateDinastija([FromBody] Dinastija dinastija)
{
    // Check if the 'Godina' for the start year exists
    var pocetakGodina = await _client.Cypher
                                      .Match("(g:Godina)")
                                      .Where((Godina g) => g.God == dinastija.PocetakVladavine.God) // Match by the start year
                                      .Return(g => g.As<Godina>())
                                      .ResultsAsync;

    Godina pocetakGodinaToUse;

    if (pocetakGodina.Any())
    {
        // If the start year 'Godina' exists, use the existing one
        pocetakGodinaToUse = pocetakGodina.First();
    }
    else
    {
        // If the start year 'Godina' does not exist, create it
        pocetakGodinaToUse = new Godina { God = dinastija.PocetakVladavine.God,
                                          ID =  Guid.NewGuid()
                                        };

        await _client.Cypher
            .Create("(g:Godina {ID: $id, God: $god})")
            .WithParam("god", pocetakGodinaToUse.God)
            .WithParam("id", pocetakGodinaToUse.ID)
            .ExecuteWithoutResultsAsync();
    }

    // Check if the 'Godina' for the end year exists
    var krajGodina = await _client.Cypher
                                  .Match("(g:Godina)")
                                  .Where((Godina g) => g.God == dinastija.KrajVladavine.God) // Match by the end year
                                  .Return(g => g.As<Godina>())
                                  .ResultsAsync;

    Godina krajGodinaToUse;

    if (krajGodina.Any())
    {
        // If the end year 'Godina' exists, use the existing one
        krajGodinaToUse = krajGodina.First();
    }
    else
    {
        // If the end year 'Godina' does not exist, create it
        krajGodinaToUse = new Godina { God = dinastija.KrajVladavine.God,
                                       ID =  Guid.NewGuid()
                                     };

        await _client.Cypher
            .Create("(g:Godina {ID: $id, God: $god})")
            .WithParam("god", krajGodinaToUse.God)
            .WithParam("id", krajGodinaToUse.ID)
            .ExecuteWithoutResultsAsync();
    }

    // Now, create the 'Dinastija' and connect it to both the start and end 'Godina'
    await _client.Cypher
                 .Match("(pg:Godina {God: $pocetak})", "(kg:Godina {God: $kraj})")
                 .Create("(d:Dinastija {ID: $id, Naziv: $naziv, Slika: $slika}) -[:HAS_POCETAK_VLADAVINE]-> (pg), (d) -[:HAS_KRAJ_VLADAVINE]-> (kg)")
                 .WithParam("id", Guid.NewGuid())
                 .WithParam("naziv", dinastija.Naziv)
                 .WithParam("slika", dinastija.Slika)
                 .WithParam("pocetak", dinastija.PocetakVladavine.God)
                 .WithParam("kraj", dinastija.KrajVladavine.God)
                //  .WithParam("pocetak", pocetakGodinaToUse.God)
                //  .WithParam("kraj", krajGodinaToUse.God)
            // .WithParam("idp", pocetakGodinaToUse.ID)
                 
            // .WithParam("idk", krajGodinaToUse.ID)
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