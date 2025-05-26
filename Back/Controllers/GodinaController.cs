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
                                           .Where((Godina g) => g.God == godina.God && g.IsPNE == godina.IsPNE)
                                           .Return(g => g.As<Godina>())
                                           .ResultsAsync)
                                           .FirstOrDefault();

            if (god != null)
            {
                return BadRequest($"Godina {god.God}. vec postoji u bazi!");
            }

            await _client.Cypher.Create("(g:Godina {ID: $id, God: $god, IsPNE: $ispne})")
                                .WithParam("god", godina.God)
                                .WithParam("id", Guid.NewGuid())
                                .WithParam("ispne", godina.IsPNE)
                                .ExecuteWithoutResultsAsync();
            return Ok($"Godina {godina.God}. je uspesno dodata u bazu!");
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

    [HttpDelete("DeleteGodina/{id}")]
    public async Task<IActionResult> DeleteGodina(Guid id)
    {
        try
        {
            var god = (await _client.Cypher.Match("(g:Godina)")
                                            .Where((Godina g) => g.ID == id)
                                            .Return(g => g.As<Godina>())
                                            .ResultsAsync)
                                            .FirstOrDefault();
            if (god == null)
            {
                return NotFound($"Godina sa ID-em {id} ne postoji u bazi!");
            }

            await _client.Cypher.Match("(g:Godina)")
                                .Where((Godina g) => g.ID == id)
                                .DetachDelete("g") 
                                .ExecuteWithoutResultsAsync();

            return Ok($"Godina sa ID: {id} je uspešno obrisana iz baze!");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Došlo je do greške pri radu sa Neo4j bazom: {ex.Message}");
        }
    }
    [HttpPut("UpdateGodina/{id}")]
    public async Task<IActionResult> UpdateGodina(Guid id, [FromBody] Godina updatedGodina)
    {
        try 
        {
    
            var god = (await _client.Cypher.Match("(g:Godina)")
                                    .Where((Godina g) => g.ID == id)
                                    .Return(g => g.As<Godina>())
                                    .ResultsAsync)
                                    .FirstOrDefault();
                

            if (god == null)
                return NotFound($"Godina sa ID: {id} nije pronađena.");

            var god1 = (await _client.Cypher.Match("(g:Godina)")
                                           .Where((Godina g) => g.God == updatedGodina.God && g.IsPNE == updatedGodina.IsPNE)
                                           .Return(g => g.As<Godina>())
                                           .ResultsAsync)
                                           .FirstOrDefault();

            if (god1 != null)
            {
                return BadRequest($"Godina {updatedGodina.God}. vec postoji u bazi!");
            }
            await _client.Cypher.Match("(g:Godina)")
                                .Where((Godina g) => g.ID == id)
                                .Set("g.God = $godina, g.IsPNE = $ispne")
                                .WithParam("godina", updatedGodina.God)
                                .WithParam("ispne", updatedGodina.IsPNE)
                                .ExecuteWithoutResultsAsync();

            return Ok($"Godina sa ID: {id} uspešno ažurirana.");
        }

        catch (Exception ex)  
        {
            return StatusCode(500, $"Došlo je do greške pri radu sa Neo4j bazom: {ex.Message}");
        }
    }


}