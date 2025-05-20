using Microsoft.AspNetCore.Mvc;
//using Neo4j.Driver;
using Neo4jClient;
using System;
using System.Threading.Tasks;
using KrvNijeVoda.Back.Models;
using System.Reflection.Metadata;
using KrvNijeVoda.Back;
using System.Formats.Tar;
using KrvNijeVoda.Back.Helpers;

[Route("api")]
[ApiController]
public class DinastijaController : ControllerBase
{
    private readonly IGraphClient _client;
    private readonly GodinaService _godinaService;

    // Constructor: Injecting Neo4jService and getting the client
    public DinastijaController(Neo4jService neo4jService, GodinaService godinaService)
    {
        _client = neo4jService.GetClient();  // Get the Neo4jClient
        _godinaService = godinaService;
    }

    [HttpPost("CreateDinastija")]
    public async Task<IActionResult> CreateDinastija([FromBody] Dinastija dinastija)
    {
        //provere za prazna polja FRONT
        //provere godPocetka < godKraja i to FRONT 
        //godina da ne sme da bude 0 FRONT 
        //pne na false FRONT??
        try
        {
            var query = _client.Cypher.Create("(d:Dinastija {ID: $id, Naziv: $naziv, Slika: $slika})")
                                      .WithParam("id", Guid.NewGuid())
                                      .WithParam("naziv", dinastija.Naziv)
                                      .WithParam("slika", dinastija.Slika);

            if (dinastija.PocetakVladavineGod != 0)
            {
                await _godinaService.DodajGodinu(dinastija.PocetakVladavineGod, dinastija.PocetakVladavinePNE);
                query = query.With("d")
                             .Match("(pg:Godina {God: $pocetak, IsPNE: $pocetakPNE})")
                             .WithParam("pocetak", dinastija.PocetakVladavineGod)
                             .WithParam("pocetakPNE", dinastija.PocetakVladavinePNE)
                             .Create("(d)-[:POCETAK_VLADAVINE]->(pg)");
            }

            if (dinastija.KrajVladavineGod != 0)
            {
                await _godinaService.DodajGodinu(dinastija.KrajVladavineGod, dinastija.KrajVladavinePNE);
                query = query.With("d")
                             .Match("(kg:Godina {God: $kraj, IsPNE: $krajPNE})")
                             .WithParam("kraj", dinastija.KrajVladavineGod)
                             .WithParam("krajPNE", dinastija.KrajVladavinePNE)
                             .Create("(d)-[:KRAJ_VLADAVINE]->(kg)");
            }

            await query.ExecuteWithoutResultsAsync();

            return Ok($"Dinastija {dinastija.Naziv} je uspesno kreirana!");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Došlo je do greške pri radu sa Neo4j bazom: {ex.Message}");
        }

    }

    //SREDI
    [HttpGet("GetDinastija/{id}")]
    public async Task<IActionResult> GetDinastija(Guid id)
    {
        try
        {
            var din = (await _client.Cypher.Match("(d:Dinastija)")
                                           .Where((Dinastija d) => d.ID == id)
                                           .Return(d => d.As<Dinastija>())
                                           .ResultsAsync)
                                           .FirstOrDefault();
            if (din == null)
                return NotFound($"Dinastija sa ID: {id} ne postoji u bazi!");

            var result = new Dinastija
            {
                ID = din.ID,
                Naziv = din.Naziv,
                Slika = din.Slika,
                // PocetakVladavine = new GodinaStruct(din.PocetakVladavineGod, din.PocetakVladavinePNE),
                // KrajVladavine = new GodinaStruct(din.KrajVladavineGod, din.KrajVladavinePNE)
            };

            return Ok(din);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Došlo je do greške pri radu sa Neo4j bazom: {ex.Message}");
        }
    }


}