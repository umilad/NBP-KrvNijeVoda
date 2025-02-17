using Microsoft.AspNetCore.Mvc;
using Neo4jClient;
using System;
using System.Threading.Tasks;
using KrvNijeVoda.Back.Models;
using System.Reflection.Metadata;
using KrvNijeVoda.Back;
[Route("api")]
[ApiController]
public class RatController : ControllerBase
{
    private readonly IGraphClient _client;
    private readonly GodinaService _godinaService;
    private readonly LokacijaService _lokacijaService;
    private readonly ZemljaService _zemljaService;

    public RatController(Neo4jService neo4jService, GodinaService godinaService, LokacijaService lokacijaService, ZemljaService zemljaService)
    {
        _client = neo4jService.GetClient();
        _godinaService = godinaService;
        _lokacijaService = lokacijaService;
        _zemljaService = zemljaService;
    }

    [HttpPost("CreateRat")]
    public async Task<IActionResult> CreateRat([FromBody] Rat rat)
    {
        try
        {
            var postojiRat = await _client.Cypher.Match("(r:Rat)")
                                                 .Where((Rat r) => r.Ime == rat.Ime)
                                                 .Return(r => r.As<Rat>())
                                                 .ResultsAsync;

            if (postojiRat!=null)
            {
                return BadRequest($"Rat sa imenom '{rat.Ime}' već postoji u bazi!");
            }

            await _godinaService.DodajGodinu(rat.Godina.God);
            await _godinaService.DodajGodinu(rat.GodinaDo.God);
            await _zemljaService.DodajZemlju(rat.Lokacija.PripadaZemlji);
            var lokacija = await _lokacijaService.DodajLokaciju(rat.Lokacija.Naziv, rat.Lokacija.PripadaZemlji);
            
            var ratID = Guid.NewGuid();
            await _client.Cypher
                        .Match("(g:Godina {God: $godina})", "(gd:Godina {God: $godinaDo})", "(l:Lokacija {ID: $idLokacije})")
                        .Create("(r:Dogadjaj:Rat {ID: $id, Ime: $ime, Tip: 'Rat', Tekst: $tekst}) -[:DESIO_SE]-> (g), " +
                                "(r) -[:RAT_TRAJAO_DO]-> (gd), (r) -[:DESIO_SE_U]-> (l)")
                        .WithParam("id", ratID)
                        .WithParam("ime", rat.Ime)
                        .WithParam("tekst", rat.Tekst)
                        .WithParam("godina", rat.Godina.God)
                        .WithParam("godinaDo", rat.GodinaDo.God)
                        .WithParam("idLokacije", lokacija.ID)
                        .ExecuteWithoutResultsAsync();
            
            return Ok($"Uspesno dodat rat sa id:{ratID} u bazu!");
        }
        catch (Exception ex)  
        {
            return StatusCode(500, $"Došlo je do greške pri radu sa Neo4j bazom: {ex.Message}");
        }
    }

    [HttpGet("GetRat/{id}")]
    public async Task<IActionResult> GetRat(Guid id)
    {
        try
        {
            var rat = (await _client.Cypher.Match("(r:Rat)")
                                           .Where((Rat r) => r.ID == id)
                                           .OptionalMatch("(r)-[:DESIO_SE]->(g:Godina)")
                                           .OptionalMatch("(r)-[:RAT_TRAJAO_DO]->(gd:Godina)")
                                           .OptionalMatch("(r)-[:DESIO_SE_U]->(l:Lokacija)-[:PRIPADA_ZEMLJI]->(z:Zemlja)")
                                           .Return((r, g, gd, l, z) => new {
                                               Rat = r.As<Rat>(),
                                               PocetnaGodina = g.As<Godina>(),
                                               ZavrsnaGodina = gd.As<Godina>(),
                                               Lokacija = l.As<Lokacija>(),
                                               Zemlja = z.As<Zemlja>()
                                           })
                                           .ResultsAsync)
                                           .FirstOrDefault();
            if (rat == null)
            {
                return BadRequest($"Nije pronadjen nijedan rat sa id: {id}");
            }
            
            if (rat.Lokacija != null)
                rat.Lokacija.PripadaZemlji = rat.Zemlja ?? new Zemlja();
            
            var result = new Rat {
                ID = rat.Rat.ID,
                Ime = rat.Rat.Ime,
                Tekst = rat.Rat.Tekst,
                Tip = TipDogadjaja.Rat,
                Godina = rat.PocetnaGodina ?? new Godina(),
                GodinaDo = rat.ZavrsnaGodina ?? new Godina(),
                Lokacija = rat.Lokacija ?? new Lokacija()
            };

            return Ok(result);
        }
        catch (Exception ex)  
        {
            return StatusCode(500, $"Došlo je do greške pri radu sa Neo4j bazom: {ex.Message}");
        }
    }

    [HttpDelete("DeleteRat/{id}")]
    public async Task<IActionResult> DeleteRat(Guid id)
    {
        try
        {
            await _client.Cypher.Match("(r:Rat)")
                                .Where((Rat r) => r.ID == id)
                                .OptionalMatch("(r)-[r1:DESIO_SE]->(g:Godina)")
                                .OptionalMatch("(r)-[r2:RAT_TRAJAO_DO]->(gd:Godina)")
                                .OptionalMatch("(r)-[r3:DESIO_SE_U]->(l:Lokacija)")
                                .Delete("r1, r2, r3, r")
                                .ExecuteWithoutResultsAsync();
            return Ok($"Rat sa id {id} je obrisan!");
        }
        catch (Exception ex)  
        {
            return StatusCode(500, $"Došlo je do greške pri radu sa Neo4j bazom: {ex.Message}");
        }
    }

    [HttpGet("GetAllRatovi")]
    public async Task<IActionResult> GetAllRatovi()
    {
        try
        {
            var ratovi = (await _client.Cypher.Match("(r:Rat)")
                                              .OptionalMatch("(r)-[:DESIO_SE]->(g:Godina)")
                                              .OptionalMatch("(r)-[:RAT_TRAJAO_DO]->(gd:Godina)")
                                              .OptionalMatch("(r)-[:DESIO_SE_U]->(l:Lokacija)-[:PRIPADA_ZEMLJI]->(z:Zemlja)")
                                              .Return((r, g, gd, l, z) => new {
                                                  Rat = r.As<Rat>(),
                                                  PocetnaGodina = g.As<Godina>(),
                                                  ZavrsnaGodina = gd.As<Godina>(),
                                                  Lokacija = l.As<Lokacija>(),
                                                  Zemlja = z.As<Zemlja>()
                                              })
                                              .ResultsAsync)
                                              .ToList();
            
            if (ratovi == null || !ratovi.Any())
            {
                return BadRequest("Nije pronađen nijedan rat");
            }
            
            var result = ratovi.Select(item => {
                if (item.Lokacija != null)
                    item.Lokacija.PripadaZemlji = item.Zemlja ?? new Zemlja();
                
                return new Rat
                {
                    ID = item.Rat.ID,
                    Ime = item.Rat.Ime,
                    Tekst = item.Rat.Tekst,
                    Tip = TipDogadjaja.Rat,
                    Godina = item.PocetnaGodina ?? new Godina(),
                    GodinaDo = item.ZavrsnaGodina ?? new Godina(),
                    Lokacija = item.Lokacija ?? new Lokacija()
                };
            }).ToList();
            
            return Ok(result);
        }
        catch (Exception ex)  
        {
            return StatusCode(500, $"Došlo je do greške pri radu sa Neo4j bazom: {ex.Message}");
        }
    }
}