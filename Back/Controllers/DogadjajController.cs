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
    private readonly GodinaService _godinaService;
    // private readonly LokacijaService _lokacijaService;
    // private readonly ZemljaService _zemljaService;

    public DogadjajController(Neo4jService neo4jService, GodinaService godinaService /*, LokacijaService lokacijaService, ZemljaService zemljaService*/)
    {
        _client = neo4jService.GetClient();
        _godinaService = godinaService;
        // _lokacijaService = lokacijaService;
        // _zemljaService = zemljaService;
    }
    [HttpPost("CreateDogadjaj")]
    public async Task<IActionResult> CreateDogadjaj([FromBody] Dogadjaj dogadjaj)
    {
        //Provera za tip dogadjaja da li je broj veci od 5 ili manji od 0
        try
        {
            Godina god = null;
            if (dogadjaj.Godina != null)
            {
                god = await _godinaService.DodajGodinu(dogadjaj.Godina.God, dogadjaj.Godina.IsPNE);
            }
            //await _zemljaService.DodajZemlju(dogadjaj.Lokacija.PripadaZemlji);
            if (dogadjaj.Lokacija != null)
            {
                var zemljaPostoji = (await _client.Cypher
                                                        .Match("(z:Zemlja)")
                                                        .Where("toLower(z.Naziv) = toLower($naziv)")
                                                        .WithParam("naziv", dogadjaj.Lokacija)
                                                        .Return(z => z.As<Zemlja>())
                                                        .ResultsAsync)
                                                        .Any();

                if (!zemljaPostoji)
                {
                    return BadRequest($"Zemlja sa nazivom '{dogadjaj.Lokacija}' ne postoji u bazi!");
                }
            }

            string label = dogadjaj.Tip.ToString();
            var dogadjajID = Guid.NewGuid();

                
            var query = _client.Cypher
                               .Create($"(d:Dogadjaj:{label} {{ID: $id, Ime: $ime, Tip: $tip, Tekst: $tekst, Lokacija: $lokacija}})")
                               .WithParam("id", dogadjajID)
                               .WithParam("ime", dogadjaj.Ime)
                               .WithParam("tip", dogadjaj.Tip.ToString())
                               .WithParam("tekst", dogadjaj.Tekst)
                               .WithParam("lokacija", dogadjaj.Lokacija);

                
                if (god != null && god.ID != Guid.Empty)
                {
                    query = query
                                .With("d")
                                .Match("(g:Godina {ID: $idGodine})")
                                .Create("(d)-[:DESIO_SE]->(g)")
                                .WithParam("idGodine", god.ID);

                }

                if (!string.IsNullOrWhiteSpace(dogadjaj.Lokacija))
                {
                    query = query
                        .With("d")
                        .Match("(z:Zemlja)")
                        .Where("toLower(z.Naziv) = toLower($nazivZemlje)")
                        .Create("(d)-[:DESIO_SE_U]->(z)")
                        .WithParam("nazivZemlje", dogadjaj.Lokacija);
                }

                await query.ExecuteWithoutResultsAsync();

                return Ok($"Uspesno dodat dogadjaj sa id:{dogadjajID} u bazu!");

        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Došlo je do greške pri radu sa Neo4j bazom: {ex.Message}");
        }
    }



    [HttpGet("GetDogadjaj/{id}")]
    public async Task<IActionResult> GetDogadjaj(Guid id)
    {
        try
        {
            var dog = (await _client.Cypher.Match("(d:Dogadjaj)")
                                           .Where((Dogadjaj d) => d.ID == id)
                                           .OptionalMatch("(d)-[:DESIO_SE]->(g:Godina)")
                                           .OptionalMatch("(d)-[:DESIO_SE_U]->(z:Zemlja)")
                                           .Return((d, g) => new
                                           {
                                               Dogadjaj = d.As<Dogadjaj>(),
                                               DogadjajUGodini = g.As<Godina>(),
                                           })
                                           .ResultsAsync)
                                           .FirstOrDefault();
            if (dog == null)
            {
                return BadRequest($"Nije pronadjen nijedan dogadjaj sa id: {id}");
            }
            var result = new Dogadjaj
            {
                ID = dog.Dogadjaj.ID,
                Ime = dog.Dogadjaj.Ime,
                Tip = dog.Dogadjaj.Tip,
                Tekst = dog.Dogadjaj.Tekst,
                Godina = dog.DogadjajUGodini,
                Lokacija = dog.Dogadjaj.Lokacija
            };
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Došlo je do greške pri radu sa Neo4j bazom: {ex.Message}");
        }
    }
    [HttpDelete("DeleteDogadjaj/{id}")]
    public async Task<IActionResult> DeleteDogadjaj(Guid id)
    {
        try
        {
            await _client.Cypher.Match("(d:Dogadjaj)")
                                .Where((Dogadjaj d) => d.ID == id)
                                .DetachDelete("d")
                                .ExecuteWithoutResultsAsync();
            return Ok($"Dogadjaj sa id {id} je obrisan!");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Došlo je do greške pri radu sa Neo4j bazom: {ex.Message}");
        }
    }

    [HttpPut("UpdateDogadjaj/{id}")]
    public async Task<IActionResult> UpdateDogadjaj(Guid id, [FromBody] Dogadjaj updatedDogadjaj)
    {
        //isto provera za tip kao i za create
        var dog = (await _client.Cypher
            .Match("(d:Dogadjaj)")
            .Where((Dogadjaj d) => d.ID == id)
            .Return(d => d.As<Dogadjaj>())
            .ResultsAsync)
            .FirstOrDefault();

        if (dog == null)
            return NotFound($"Dogadjaj sa ID {id} nije pronađen!");

        var cypher = _client.Cypher
                    .Match("(d:Dogadjaj)")
                    .Where((Dogadjaj d) => d.ID == id)
                    .Set("d.Ime = $ime, d.Tekst = $tekst, d.Tip=$tip, d.Lokacija= $lokacija")
                    .Remove("d:Bitka:Rat:Sporazum:Savez:Dokument:Ustanak")
                    .Set($"d:{updatedDogadjaj.Tip}") 
                    .WithParam("ime", updatedDogadjaj.Ime)
                    .WithParam("tip", updatedDogadjaj.Tip)
                    .WithParam("tekst", updatedDogadjaj.Tekst)
                    .WithParam("lokacija", updatedDogadjaj.Lokacija);

        if (updatedDogadjaj.Godina != null)
        {
            var god = await _godinaService.DodajGodinu(updatedDogadjaj.Godina.God, updatedDogadjaj.Godina.IsPNE);
            cypher = cypher.With("d").Match("(g:Godina {ID: $idGodine})")
                            .OptionalMatch("(d)-[rel:DESIO_SE]->()")
                            .Delete("rel")
                            .WithParam("idGodine", god.ID)
                            .Merge("(d)-[:DESIO_SE]->(g)");
        }

        else
        {
            cypher = cypher.With("d")
                        .OptionalMatch("(d)-[rel:DESIO_SE]->()")
                        .Delete("rel");
        }
        if (updatedDogadjaj.Lokacija != null)
        {

            var zemljaPostoji = await _client.Cypher
                                             .Match("(z:Zemlja)")
                                             .Where("toLower(z.Naziv) = toLower($naziv)")
                                             .WithParam("naziv", updatedDogadjaj.Lokacija)
                                             .Return(z => z.As<Zemlja>())
                                             .ResultsAsync;

            if (zemljaPostoji.Any())
            {
                cypher = cypher
                         .With("d")
                         .Match("(z:Zemlja {Naziv: $naziv})")
                         .OptionalMatch("(d)-[rel:DESIO_SE_U]->()")
                         .Delete("rel")
                         .WithParam("naziv", updatedDogadjaj.Lokacija)
                         .Merge("(d)-[:DESIO_SE_U]->(z)");
            }
            else
            {
                return BadRequest($"Zemlja sa nazivom {updatedDogadjaj.Lokacija} ne postoji!");
            }
        }
        else
        {
            
            cypher = cypher.With("d")
                        .OptionalMatch("(d)-[rel:DESIO_SE_U]->()")
                        .Delete("rel");
        }

        
        await cypher.ExecuteWithoutResultsAsync();
        return Ok($"Uspešno ažuriran događaj '{updatedDogadjaj.Ime}' sa ID: {id}!");
    }

}