using Microsoft.AspNetCore.Mvc;
using Neo4jClient;
using System;
using System.Threading.Tasks;
using KrvNijeVoda.Back.Models;
using System.Reflection.Metadata;
using KrvNijeVoda.Back;
[Route("api")]
[ApiController]
public class ZemljaController : ControllerBase
{
    private readonly IGraphClient _client;

    public ZemljaController(Neo4jService neo4jService)
    {
        _client = neo4jService.GetClient();  
    }

[HttpGet("GetZemlja/{id}")]
public async Task<IActionResult> GetZemlja(Guid id)
{
    try
    {
        var zemlja = (await _client.Cypher.Match("(z:Zemlja)")
                                          .Where((Zemlja z) => z.ID == id)
                                          .Return(z => z.As<Zemlja>())
                                          .ResultsAsync)
                                          .FirstOrDefault();
        if (zemlja == null)
        {
            return NotFound($"Zemlja sa ID-em {id} ne postoji u bazi!");
        }

        return Ok(zemlja);
    }
    catch (Exception ex)  
    {
        return StatusCode(500, $"Došlo je do greške pri radu sa Neo4j bazom: {ex.Message}");
    }
}

[HttpPost("CreateZemlja")]
public async Task<IActionResult> CreateZemlja([FromBody] Zemlja zemlja)
{
    try
    {
        var zem = (await _client.Cypher.Match("(z:Zemlja)")
                                       .Where((Zemlja z) => z.ID == zemlja.ID)
                                       .Return(z => z.As<Zemlja>())
                                       .ResultsAsync)
                                       .FirstOrDefault();
        if (zem != null)
        {
            return BadRequest($"Zemlja sa ID-em {zemlja.ID} i nazivom {zemlja.Naziv} već postoji u bazi!");
        }
        
        await _client.Cypher.Create("(z:Zemlja {ID: $id, Naziv: $naziv, Trajanje: $trajanje, Grb: $grb})")
                            .WithParam("naziv", zemlja.Naziv)
                            .WithParam("trajanje", zemlja.Trajanje)
                            .WithParam("grb", zemlja.Grb)
                            .WithParam("id", Guid.NewGuid())
                            .ExecuteWithoutResultsAsync();

        return Ok($"Zemlja {zemlja.Naziv} je uspešno dodata u bazu!");
    }
    catch (Exception ex)  
    {
        return StatusCode(500, $"Došlo je do greške pri radu sa Neo4j bazom: {ex.Message}");
    }
}

[HttpPut("UpdateZemlja/{id}")]
public async Task<IActionResult> UpdateZemlja(Guid id, [FromBody] Zemlja updatedZemlja)
{
    try
    {
        var zemlja = (await _client.Cypher.Match("(z:Zemlja)")
                                          .Where((Zemlja z) => z.ID == id)
                                          .Return(z => z.As<Zemlja>())
                                          .ResultsAsync)
                                          .FirstOrDefault();
        if (zemlja == null)
        {
            return NotFound($"Zemlja sa ID {id} ne postoji u bazi!");
        }

        await _client.Cypher.Match("(z:Zemlja)")
                            .Where((Zemlja z) => z.ID == id)
                            .Set("z.Naziv = $naziv, z.Trajanje = $trajanje, z.Grb = $grb")
                            .WithParam("naziv", updatedZemlja.Naziv)
                            .WithParam("trajanje", updatedZemlja.Trajanje)
                            .WithParam("grb", updatedZemlja.Grb)
                            .ExecuteWithoutResultsAsync();

        return Ok($"Zemlja sa ID-em {id} je uspešno ažurirana.");
    }
    catch (Exception ex) 
    {
        return StatusCode(500, $"Došlo je do greške pri radu sa Neo4j bazom: {ex.Message}");
    }
}

[HttpDelete("DeleteZemlja/{id}")]
public async Task<IActionResult> DeleteZemlja(Guid id)
{
    try
    {
        var zemlja = (await _client.Cypher.Match("(z:Zemlja)")
                                          .Where((Zemlja z) => z.ID == id)
                                          .Return(z => z.As<Zemlja>())
                                          .ResultsAsync)
                                          .FirstOrDefault();
        if (zemlja == null)
        {
            return NotFound($"Zemlja sa ID {id} ne postoji u bazi!");
        }

        await _client.Cypher.Match("(z:Zemlja)")
                            .Where((Zemlja z) => z.ID == id)
                            .Delete("z")
                            .ExecuteWithoutResultsAsync();

        return Ok($"Zemlja sa ID-em {id} je uspešno obrisana iz baze!");
    }
    catch (Exception ex) 
    {
        return StatusCode(500, $"Došlo je do greške pri radu sa Neo4j bazom: {ex.Message}");
    }
}

[HttpGet("GetAllZemlje")]
public async Task<IActionResult> GetAllZemlje()
{
    try
    {
        var zemlje = await _client.Cypher.Match("(z:Zemlja)")
                                         .Return(z => z.As<Zemlja>())
                                         .ResultsAsync;

        if (zemlje == null || !zemlje.Any())
        {
            return NotFound("Nema zemalja u bazi!");
        }

        return Ok(zemlje);
    }
    catch (Exception ex)
    {
        return StatusCode(500, $"Došlo je do greške pri radu sa Neo4j bazom: {ex.Message}");
    }
}
}