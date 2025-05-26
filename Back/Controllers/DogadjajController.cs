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
        //to ce front da resi bice drop list 
        try
        {
            var dog = (await _client.Cypher.Match("(d: Dogadjaj)")
                                           .Where((Dogadjaj d) => d.Ime == dogadjaj.Ime)
                                           .Return(d => d.As<Dogadjaj>())
                                           .ResultsAsync)
                                           .FirstOrDefault();

            if (dog != null)
            {
                return BadRequest($"Dogadjaj sa imenom {dogadjaj.Ime} vec postoji u bazi!");
            }

            string label = dogadjaj.Tip.ToString();
            var dogadjajID = Guid.NewGuid();

            var query = _client.Cypher
                               .Create($"(d:Dogadjaj:{label} {{ID: $id, Ime: $ime, Tip: $tip, Tekst: $tekst, Lokacija: $lokacija}})")
                               .WithParam("id", dogadjajID)
                               .WithParam("ime", dogadjaj.Ime)
                               .WithParam("tip", dogadjaj.Tip.ToString())
                               .WithParam("tekst", dogadjaj.Tekst)
                               .WithParam("lokacija", dogadjaj.Lokacija); //ovo mora pre provere za lokaciju da bi je prepravilo ako ne postoji

            //await _zemljaService.DodajZemlju(dogadjaj.Lokacija.PripadaZemlji);
            if (!string.IsNullOrWhiteSpace(dogadjaj.Lokacija) && dogadjaj.Lokacija != "string") //mora i sve provere za string 
            {
                var zemljaPostoji = (await _client.Cypher
                                                        .Match("(z:Zemlja)")
                                                        .Where("toLower(z.Naziv) = toLower($naziv)")
                                                        .WithParam("naziv", dogadjaj.Lokacija)
                                                        .Return(z => z.As<Zemlja>())
                                                        .ResultsAsync)
                                                        .Any();

                if (zemljaPostoji)
                {
                    query = query.With("d")
                                 .Match("(z:Zemlja {Naziv: $lokac})")
                                 .WithParam("lokac", dogadjaj.Lokacija)
                                 .Create("(d)-[:DESIO_SE_U]->(z)")
                                 .Set("d.Lokacija = $lokac");
                }
                else
                    query = query.With("d")
                                 .Set("d.Lokacija = $lok")
                                 .WithParam("lok", "string");
            }

            if (dogadjaj.Godina != null && dogadjaj.Godina.God != 0)//uneta godina 
            {
                await _godinaService.DodajGodinu(dogadjaj.Godina.God, dogadjaj.Godina.IsPNE);
                query = query
                            .With("d")
                            .Match("(g:Godina {God: $dogGod, IsPNE: $dogPNE})")
                            .Create("(d)-[:DESIO_SE]->(g)")
                            .WithParam("dogGod", dogadjaj.Godina.God)
                            .WithParam("dogPNE", dogadjaj.Godina.IsPNE);
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
                                           //.OptionalMatch("(d)-[:DESIO_SE_U]->(z:Zemlja)")
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
                                .OptionalMatch("(d)-[:DESIO_SE]->(g:Godina)")
                                .Return((d, g) => new
                                {
                                    Dogadjaj = d.As<Dogadjaj>(),
                                    Godina = g.As<Godina>()
                                })
                                .ResultsAsync)
                                .FirstOrDefault();

        if (dog == null)
            return NotFound($"Dogadjaj sa ID {id} nije pronađen!");

        var cypher = _client.Cypher
                    .Match("(d:Dogadjaj)")
                    .Where((Dogadjaj d) => d.ID == id)
                    .Set("d.Ime = $ime, d.Tekst = $tekst, d.Tip=$tip")
                    .Remove("d:Bitka:Rat:Sporazum:Savez:Dokument:Ustanak")//mora remove
                    .Set($"d:{updatedDogadjaj.Tip}")
                    .WithParam("ime", updatedDogadjaj.Ime)
                    .WithParam("tip", updatedDogadjaj.Tip)
                    .WithParam("tekst", updatedDogadjaj.Tekst);

        if (updatedDogadjaj.Godina != null)//uneta nova 
        {
            if (dog.Godina != null)//da li je postojala godina u bazi 
            {
                if (dog.Godina.God != updatedDogadjaj.Godina.God || dog.Godina.IsPNE != updatedDogadjaj.Godina.IsPNE)
                {//promenjena je 
                    await _godinaService.DodajGodinu(updatedDogadjaj.Godina.God, updatedDogadjaj.Godina.IsPNE);
                    cypher = cypher.With("d")
                                   .Match("(g:Godina {God: $dogGod, IsPNE: $dogPNE})")
                                   .Match("(d)-[rel:DESIO_SE]->()")
                                   .Delete("rel")
                                   .Create("(d)-[:DESIO_SE]->(g)")
                                   .WithParam("dogGod", updatedDogadjaj.Godina.God)
                                   .WithParam("dogPNE", updatedDogadjaj.Godina.IsPNE);
                }
                //nije promenjena ostaje isto 
            }
            else
            {
                await _godinaService.DodajGodinu(updatedDogadjaj.Godina.God, updatedDogadjaj.Godina.IsPNE);
                cypher = cypher.With("d")
                               .Match("(g:Godina {God: $dogGod, IsPNE: $dogPNE})")
                               .Create("(d)-[:DESIO_SE]->(g)")
                               .WithParam("dogGod", updatedDogadjaj.Godina.God)
                               .WithParam("dogPNE", updatedDogadjaj.Godina.IsPNE);       
            }
            
            
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
                if (!string.IsNullOrWhiteSpace(dog.Dogadjaj.Lokacija) && dog.Dogadjaj.Lokacija != "string")
                {//postoji vec neka
                    if (dog.Dogadjaj.Lokacija != updatedDogadjaj.Lokacija)
                    {//promenjena je
                        cypher = cypher
                                 .With("d")
                                 .Match("(z:Zemlja {Naziv: $naziv})")
                                 .OptionalMatch("(d)-[rel:DESIO_SE_U]->()")
                                 .Delete("rel")
                                 .WithParam("naziv", updatedDogadjaj.Lokacija)
                                 .Create("(d)-[:DESIO_SE_U]->(z)")
                                 .Set("d.Lokacija = $naziv");
                    }
                    //else ista je
                }
                else //nije postojala
                    cypher = cypher
                                 .With("d")
                                 .Match("(z:Zemlja {Naziv: $naziv})")
                                 .WithParam("naziv", updatedDogadjaj.Lokacija)
                                 .Create("(d)-[:DESIO_SE_U]->(z)")
                                 .Set("d.Lokacija = $naziv");
            }
        }
        else
        {
            cypher = cypher.With("d")
                        .OptionalMatch("(d)-[rel:DESIO_SE_U]->()")
                        .Delete("rel")
                        .Set("d.Lokacija = $naziv")
                        .WithParam("naziv", "string");
        }

        
        await cypher.ExecuteWithoutResultsAsync();
        return Ok($"Uspešno ažuriran događaj '{updatedDogadjaj.Ime}' sa ID: {id}!");
    }

}