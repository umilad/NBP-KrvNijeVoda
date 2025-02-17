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
        try
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
        catch (Exception ex)  
        {
            return StatusCode(500, $"Došlo je do greške pri radu sa Neo4j bazom: {ex.Message}");
        }
    }
    
    [HttpGet("GetGodinaByGod/{god}")]
    public async Task<IActionResult> GetGodinaByGod(int god)
    {
        try
        {
            var godina = (await _client.Cypher.Match("(g:Godina)")
                                            .Where((Godina g) => g.God == god)
                                            .Return(g => g.As<Godina>())
                                            .ResultsAsync)
                                            .FirstOrDefault();
            if (godina == null)
            {
                return NotFound($"Godina {god}. ne postoji u bazi!");
            }
            return Ok(godina);
        }
        catch (Exception ex)  
        {
            return StatusCode(500, $"Došlo je do greške pri radu sa Neo4j bazom: {ex.Message}");
        }
    }
    
    [HttpGet("GetGodina/{id}")]
    public async Task<IActionResult> GetGodina(Guid id)
    {
        try
        {
            var godina = (await _client.Cypher.Match("(g:Godina)")
                                            .Where((Godina g) => g.ID == id)
                                            .Return(g => g.As<Godina>())
                                            .ResultsAsync)
                                            .FirstOrDefault();
            if (godina == null)
            {
                return NotFound($"Godina sa id: {id} ne postoji u bazi!");
            }
            return Ok(godina);
        }
        catch (Exception ex)  
        {
            return StatusCode(500, $"Došlo je do greške pri radu sa Neo4j bazom: {ex.Message}");
        }
    }

    [HttpGet("GetAllGodine")]
    public async Task<IActionResult> GetAllGodine()
    {
        try
        {
            var godine = (await _client.Cypher.Match("(g:Godina)")
                                            .Return(g => g.As<Godina>())
                                            .ResultsAsync)
                                            .ToList();

            if (godine == null || !godine.Any())
            {
                return NotFound("Nema godina u bazi!");
            }

            return Ok(godine);
        }
        catch (Exception ex)  
        {
            return StatusCode(500, $"Došlo je do greške pri radu sa Neo4j bazom: {ex.Message}");
        }
    }

    [HttpDelete("DeleteGodina/{godina}")]
    public async Task<IActionResult> DeleteGodina(int godina)
    {
        try
        {
            var god = (await _client.Cypher.Match("(g:Godina)")
                                            .Where((Godina g) => g.God == godina)
                                            .Return(g => g.As<Godina>())
                                            .ResultsAsync)
                                            .FirstOrDefault();
            if (god == null)
            {
                return NotFound($"Godina {godina} ne postoji u bazi!");
            }

            await _client.Cypher.Match("(g:Godina)")
                                .Where((Godina g) => g.God == godina)
                                .DetachDelete("g")  // Briše sve veze pre nego što obriše čvor
                                .ExecuteWithoutResultsAsync();

            return Ok($"Godina {godina} je uspešno obrisana iz baze!");
        }
        catch (Exception ex)  
        {
            return StatusCode(500, $"Došlo je do greške pri radu sa Neo4j bazom: {ex.Message}");
        }
    }
}