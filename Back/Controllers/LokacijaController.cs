using Microsoft.AspNetCore.Mvc;
using Neo4jClient;
using System;
using System.Threading.Tasks;
using KrvNijeVoda.Back.Models;
using System.Reflection.Metadata;
using KrvNijeVoda.Back;
[Route("api")]
[ApiController]
public class LokacijaController : ControllerBase
{
    private readonly IGraphClient _client;
    private readonly ZemljaService _zemljaService;

    public LokacijaController(Neo4jService neo4jService, ZemljaService zemljaService)
    {
        _client = neo4jService.GetClient();
        _zemljaService = zemljaService;
    }

    [HttpPost("CreateLokacija")]
    public async Task<IActionResult> CreateLokacija([FromBody] Lokacija lokacija)
    {
        try
        {
            var zemlja = await _zemljaService.DodajZemlju(lokacija.PripadaZemlji);
            var lokacijaID = Guid.NewGuid();
            await _client.Cypher
                .Match("(z:Zemlja {Naziv: $nazivZemlje})")
                .Create("(l:Lokacija {ID: $id, Naziv: $naziv})-[:PRIPADA_ZEMLJI]->(z)")
                .WithParam("id", lokacijaID)
                .WithParam("naziv", lokacija.Naziv)
                .WithParam("nazivZemlje", lokacija.PripadaZemlji.Naziv)
                .ExecuteWithoutResultsAsync();
            
            return Ok($"Uspesno dodata lokacija sa id: {lokacijaID}");
        }
        catch (Exception ex)  
        {
            return StatusCode(500, $"Došlo je do greške pri radu sa Neo4j bazom: {ex.Message}");
        }
    }

    [HttpGet("GetLokacija/{id}")]
    public async Task<IActionResult> GetLokacija(Guid id)
    {
        try
        {
            var lokacija = (await _client.Cypher.Match("(l:Lokacija)-[:PRIPADA_ZEMLJI]->(z:Zemlja)")
                                                .Where((Lokacija l) => l.ID == id)
                                                .Return((l, z) => new {
                                                    Lokacija = l.As<Lokacija>(),
                                                    Zemlja = z.As<Zemlja>()
                                                })
                                                .ResultsAsync)
                                                .FirstOrDefault();

            if (lokacija == null)
                return BadRequest($"Lokacija sa ID {id} nije pronađena.");

            lokacija.Lokacija.PripadaZemlji = lokacija.Zemlja;
            return Ok(lokacija.Lokacija);
        }
        catch (Exception ex)  
        {
            return StatusCode(500, $"Došlo je do greške pri radu sa Neo4j bazom: {ex.Message}");
        }
    }

    [HttpGet("GetAllLokacije")]
    public async Task<IActionResult> GetAllLokacije()
    {
        try
        {
            var lokacije = (await _client.Cypher.Match("(l:Lokacija)-[:PRIPADA_ZEMLJI]->(z:Zemlja)")
                                                .Return((l, z) => new {
                                                    Lokacija = l.As<Lokacija>(),
                                                    Zemlja = z.As<Zemlja>()
                                                })
                                                .ResultsAsync)
                                                .ToList();

            if (!lokacije.Any())
                return BadRequest("Nema dostupnih lokacija.");

            var result = lokacije.Select(l => {
                l.Lokacija.PripadaZemlji = l.Zemlja;
                return l.Lokacija;
            }).ToList();

            return Ok(result);
        }
        catch (Exception ex)  
        {
            return StatusCode(500, $"Došlo je do greške pri radu sa Neo4j bazom: {ex.Message}");
        }
    }

    [HttpDelete("DeleteLokacija/{id}")]
    public async Task<IActionResult> DeleteLokacija(Guid id)
    {
        try
        {
            await _client.Cypher.Match("(l:Lokacija)")
                                .Where((Lokacija l) => l.ID == id)
                                .OptionalMatch("(l)-[r:PRIPADA_ZEMLJI]->(z:Zemlja)")
                                .Delete("r, l")
                                .ExecuteWithoutResultsAsync();
            
            return Ok($"Lokacija sa ID {id} je obrisana.");
        }
        catch (Exception ex)  
        {
            return StatusCode(500, $"Došlo je do greške pri radu sa Neo4j bazom: {ex.Message}");
        }
    }
}