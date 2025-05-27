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
    [HttpPost("CreateZemlja")]
    //broj stanovnika >0 front 
    public async Task<IActionResult> CreateZemlja([FromBody] Zemlja zemlja)
    {
        try
        {
            var zem = (await _client.Cypher.Match("(z:Zemlja)")
                                           .Where("toLower(z.Naziv) = toLower($naziv)")
                                           .WithParam("naziv", zemlja.Naziv)
                                           .Return(z => z.As<Zemlja>())
                                           .ResultsAsync)
                                           .FirstOrDefault();
            if (zem != null)
            {
                return BadRequest($"Zemlja {zemlja.Naziv} već postoji u bazi!");
            }

            await _client.Cypher.Create("(z:Zemlja {ID: $id, Naziv: $naziv, Trajanje: $trajanje, Grb: $grb, BrojStanovnika: $brojstanovnika})")
                                .WithParam("naziv", zemlja.Naziv)
                                .WithParam("trajanje", zemlja.Trajanje)
                                .WithParam("grb", zemlja.Grb)
                                .WithParam("brojstanovnika", zemlja.BrojStanovnika)
                                .WithParam("id", Guid.NewGuid())
                                .ExecuteWithoutResultsAsync();

            return Ok($"Zemlja {zemlja.Naziv} je uspešno dodata u bazu!");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Došlo je do greške pri radu sa Neo4j bazom: {ex.Message}");
        }
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
                return NotFound($"Zemlja sa ID: {id} ne postoji u bazi!");
            }

            return Ok(zemlja);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Došlo je do greške pri radu sa Neo4j bazom: {ex.Message}");
        }
    }

    [HttpGet("GetZemljaPoNazivu/{naziv}")]
    public async Task<IActionResult> GetZemlja(string naziv)
    {
        try
        {
            var zemlja = (await _client.Cypher.Match("(z:Zemlja)")
                                            .Where("toLower(z.Naziv) = toLower($naziv)")
                                            .WithParam("naziv", naziv)
                                            .Return(z => z.As<Zemlja>())
                                            .ResultsAsync)
                                            .FirstOrDefault();
            if (zemlja == null)
            {
                return NotFound($"Zemlja sa nazivom {naziv} ne postoji u bazi!");
            }

            return Ok(zemlja);
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
                return NotFound($"Zemlja sa ID: {id} ne postoji u bazi!");
            }

            await _client.Cypher.Match("(z:Zemlja)")
                                .Where((Zemlja z) => z.ID == id)
                                .DetachDelete("z")
                                .ExecuteWithoutResultsAsync();

            return Ok($"Zemlja sa ID: {id} je uspešno obrisana iz baze!");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Došlo je do greške pri radu sa Neo4j bazom: {ex.Message}");
        }
    }

    [HttpDelete("DeleteZemljaPoNazivu/{naziv}")]
    public async Task<IActionResult> DeleteZemlja(string naziv)
    {
        try
        {
            var zemlja = (await _client.Cypher.Match("(z:Zemlja)")
                                            .Where("toLower(z.Naziv) = toLower($naziv)")
                                            .WithParam("naziv", naziv)
                                            .Return(z => z.As<Zemlja>())
                                            .ResultsAsync)
                                            .FirstOrDefault();
            if (zemlja == null)
            {
                return NotFound($"Zemlja sa nazivom {naziv} ne postoji u bazi!");
            }

            await _client.Cypher.Match("(z:Zemlja)")
                                .Where("toLower(z.Naziv) = toLower($naziv)")
                                .WithParam("naziv", naziv)
                                .DetachDelete("z")
                                .ExecuteWithoutResultsAsync();

            return Ok($"Zemlja sa nazivom {naziv} je uspešno obrisana iz baze!");
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
            
            //ako naziv ostaje isti to je ta ista zemlja 
            var duplikat = (await _client.Cypher
                .Match("(z:Zemlja)")
                .Where("toLower(z.Naziv) = toLower($naziv) AND z.ID <> $id")
                .WithParam("naziv", updatedZemlja.Naziv)
                .WithParam("id", id)
                .Return(z => z.As<Zemlja>())
                .ResultsAsync)
                .Any();

            if (duplikat)
            {
                return BadRequest($"Zemlja sa nazivom '{updatedZemlja.Naziv}' već postoji u bazi!");
            }
            

            await _client.Cypher.Match("(z:Zemlja)")
                                .Where((Zemlja z) => z.ID == id)
                                .Set("z.Naziv = $naziv, z.Trajanje = $trajanje, z.Grb = $grb, z.BrojStanovnika = $brojstanovnika")
                                .WithParam("naziv", updatedZemlja.Naziv)
                                .WithParam("trajanje", updatedZemlja.Trajanje)
                                .WithParam("grb", updatedZemlja.Grb)
                                .WithParam("brojstanovnika", updatedZemlja.BrojStanovnika)
                                .ExecuteWithoutResultsAsync();

            return Ok($"Zemlja sa ID: {id} je uspešno ažurirana.");
        }
        catch (Exception ex) 
        {
            return StatusCode(500, $"Došlo je do greške pri radu sa Neo4j bazom: {ex.Message}");
        }
    }
}