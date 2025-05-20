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

    // [HttpPost("CreateDinastija")]
    // public async Task<IActionResult> CreateDinastija([FromBody] Dinastija dinastija)
    // {
    //     //provere za prazna polja FRONT
    //     //provere godPocetka < godKraja i to FRONT 
    //     //godina da ne sme da bude 0 FRONT 
    //     //pne na false FRONT??
    //     try
    //     {
    //         if (dinastija.PocetakVladavine != null)
    //             await _godinaService.DodajGodinu(dinastija.PocetakVladavine.Value.GodS, dinastija.PocetakVladavine.Value.PneS);

    //         if (dinastija.KrajVladavine != null)
    //             await _godinaService.DodajGodinu(dinastija.KrajVladavine.Value.GodS, dinastija.KrajVladavine.Value.PneS);

    //         await _client.Cypher.Match("(pg:Godina {God: $pocetak, IsPNE: $pocetakPNE})", "(kg:Godina {God: $kraj, IsPNE: $krajPNE})")
    //                             .Create("(d:Dinastija {ID: $id, Naziv: $naziv, Slika: $slika}) - [:POCETAK_VLADAVINE] -> (pg), (d) - [:KRAJ_VLADAVINE] -> (kg)")
    //                             .WithParam("id", Guid.NewGuid())
    //                             .WithParam("naziv", dinastija.Naziv)
    //                             .WithParam("slika", dinastija.Slika)
    //                             .WithParam("pocetak", dinastija.PocetakVladavine.Value.GodS)
    //                             .WithParam("pocetakPNE", dinastija.PocetakVladavine.Value.PneS)
    //                             .WithParam("kraj", dinastija.KrajVladavine.Value.GodS)
    //                             .WithParam("krajPNE", dinastija.KrajVladavine.Value.PneS)
    //                             .ExecuteWithoutResultsAsync();

    //         return Ok($"Dinastija {dinastija.Naziv} je uspesno kreirana!");
    //     }
    //     catch (Exception ex)
    //     {
    //         return StatusCode(500, $"Došlo je do greške pri radu sa Neo4j bazom: {ex.Message}");
    //     }

    // }

    // [HttpGet("GetDinastija/{id}")]
    // public async Task<IActionResult> GetDinastija(Guid id)
    // {
    //     try
    //     {
    //         var din = (await _client.Cypher.Match("(d:Dinastija)")
    //                                        .Where((Dinastija d) => d.ID == id)
    //                                        .Return((d) => new {
    //                                             Dinastija = d.As<Dinastija>(),
    //                                             PocetakVladavine = d.PocetakVladavine.As<GodinaStruct>(),
    //                                             KrajVladavine = d.KrajVladavine.As<GodinaStruct>()
    //                                        })
    //                                        .ResultsAsync)
    //                                        .FirstOrDefault();
    //         if (din == null)
    //             return NotFound($"Dinastija sa ID: {id} ne postoji u bazi!");

    //         var result = new Dinastija
    //         {
    //             ID = din.Dinastija.ID,
    //             Naziv = din.Dinastija.Naziv,
    //             Slika = din.Dinastija.Slika,
    //             PocetakVladavine = new GodinaStruct(din.PocetakVladavine.Value.GodS, din.PocetakVladavine.Value.PneS),
    //             KrajVladavine = new GodinaStruct(din.KrajVladavine.Value.GodS, din.KrajVladavine.Value.PneS)
    //         };

    //         return Ok(din);
    //     }
    //     catch (Exception ex)
    //     {
    //         return StatusCode(500, $"Došlo je do greške pri radu sa Neo4j bazom: {ex.Message}");
    //     }
    // }


}