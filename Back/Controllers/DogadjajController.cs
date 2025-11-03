using Microsoft.AspNetCore.Mvc;
using Neo4jClient;
using System;
using System.Threading.Tasks;
//using KrvNijeVoda.Back.Models;
using System.Reflection.Metadata;
using KrvNijeVoda.Back;
using MongoDB.Driver;
[Route("api")]
[ApiController]
public class DogadjajController : ControllerBase
{
    private readonly IGraphClient _client;
    private readonly IMongoCollection<DogadjajMongo> _mongo;
    private readonly GodinaService _godinaService;
    // private readonly LokacijaService _lokacijaService;
    // private readonly ZemljaService _zemljaService;

    public DogadjajController(Neo4jService neo4jService, GodinaService godinaService,  MongoService mongoService/*, LokacijaService lokacijaService, ZemljaService zemljaService*/)
    {
        _client = neo4jService.GetClient();
        _godinaService = godinaService;
        _mongo = mongoService.GetCollection<DogadjajMongo>("Dogadjaji");
        // _lokacijaService = lokacijaService;
        // _zemljaService = zemljaService;
    }
    [HttpPost("CreateDogadjaj")]
public async Task<IActionResult> CreateDogadjaj([FromBody] DogadjajDto dogadjaj)
{
    try
    {
        var dog = (await _client.Cypher.Match("(d: Dogadjaj)")
                                       .Where("toLower(d.Ime) = toLower($ime)")
                                       .WithParam("ime", dogadjaj.Ime)
                                       .Return(d => d.As<DogadjajNeo>())
                                       .ResultsAsync)
                                       .FirstOrDefault();

        if (dog != null)
            return BadRequest($"Dogadjaj sa imenom {dogadjaj.Ime} vec postoji u bazi!");

        string label = dogadjaj.Tip.ToString();
        var dogadjajID = Guid.NewGuid();

        // Neo4j kreiranje
        var query = _client.Cypher
                           .Create($"(d:Dogadjaj:{label} {{ID: $id, Ime: $ime, Tip: $tip, Lokacija: $lokacija}})")
                           .WithParam("id", dogadjajID)
                           .WithParam("ime", dogadjaj.Ime)
                           .WithParam("tip", dogadjaj.Tip.ToString())
                           .WithParam("lokacija", dogadjaj.Lokacija);

        // Provera Zemlje
        if (!string.IsNullOrWhiteSpace(dogadjaj.Lokacija) && dogadjaj.Lokacija != "string")
        {
            var zemljaPostoji = (await _client.Cypher
                                                    .Match("(z:Zemlja)")
                                                    .Where("toLower(z.Naziv) = toLower($naziv)")
                                                    .WithParam("naziv", dogadjaj.Lokacija)
                                                    .Return(z => z.As<ZemljaNeo>())
                                                    .ResultsAsync)
                                                    .Any();

            if (zemljaPostoji)
            {
                query = query.With("d")
                             .Match("(z:Zemlja)")
                             .Where("toLower(z.Naziv) = toLower($lokac)")
                             .WithParam("lokac", dogadjaj.Lokacija)
                             .Create("(d)-[:DESIO_SE_U]->(z)")
                             .Set("d.Lokacija = $lokac");
            }
            else
                query = query.With("d")
                             .Set("d.Lokacija = $lok")
                             .WithParam("lok", "string");
        }

        // Provera Godine
        if (dogadjaj.Godina != null && dogadjaj.Godina.God != 0)
        {
            await _godinaService.DodajGodinu(dogadjaj.Godina.God, dogadjaj.Godina.IsPNE);
            query = query
                        .With("d")
                        .Match("(g:Godina {God: $dogGod, IsPNE: $dogPNE})")
                        .Create("(d)-[:DESIO_SE]->(g)")
                        .WithParam("dogGod", dogadjaj.Godina.God)
                        .WithParam("dogPNE", dogadjaj.Godina.IsPNE);
        }

        // Izvrši Neo4j upit
        await query.ExecuteWithoutResultsAsync();

        // MongoDB: smesti Tekst
        if (!string.IsNullOrWhiteSpace(dogadjaj.Tekst))
        {
            var mongoDoc = new DogadjajMongo
            {
                ID = dogadjajID,
                Tekst = dogadjaj.Tekst
            };
            await _mongo.InsertOneAsync(mongoDoc);
        }

        return Ok($"Uspesno dodat dogadjaj sa id:{dogadjajID} u bazu!");
    }
    catch (Exception ex)
    {
        return StatusCode(500, $"Došlo je do greške pri radu sa bazama: {ex.Message}");
    }
}


    [HttpGet("GetDogadjaj/{id}")]
public async Task<IActionResult> GetDogadjaj(Guid id)
{
    try
    {
        // 1. Dohvatanje iz Neo4j (bez Teksta)
        var dog = (await _client.Cypher
            .Match("(d:Dogadjaj)")
            .Where((DogadjajNeo d) => d.ID == id)
            .OptionalMatch("(d)-[:DESIO_SE]->(g:Godina)")
            //.OptionalMatch("(d)-[:DESIO_SE_U]->(z:Zemlja)")
            .Return((d, g) => new
            {
                Dogadjaj = d.As<DogadjajNeo>(),
                Godina = g.As<GodinaNeo>()
            })
            .ResultsAsync)
            .FirstOrDefault();

        if (dog == null)
            return NotFound($"Nije pronađen nijedan događaj sa ID: {id}");

        // 2. Dohvatanje Teksta iz Mongo
        var mongo = await _mongo.Find(m => m.ID == id).FirstOrDefaultAsync();

        // 3. Kombinovanje u DTO
        var result = new DogadjajDto
        {
            ID = dog.Dogadjaj.ID,
            Ime = dog.Dogadjaj.Ime,
            Tip = dog.Dogadjaj.Tip,
            Lokacija = dog.Dogadjaj.Lokacija,
            Godina = dog.Godina,
            Tekst = mongo?.Tekst
        };

        return Ok(result);
    }
    catch (Exception ex)
    {
        return StatusCode(500, $"Došlo je do greške: {ex.Message}");
    }
}
    [HttpDelete("DeleteDogadjaj/{id}")]
    public async Task<IActionResult> DeleteDogadjaj(Guid id)
    {
        try
        {
            // Brisanje iz Neo4j
            await _client.Cypher.Match("(d:Dogadjaj)")
                                .Where((DogadjajNeo d) => d.ID == id)
                                .DetachDelete("d")
                                .ExecuteWithoutResultsAsync();

            // Brisanje iz Mongo
            await _mongo.DeleteOneAsync(m => m.ID == id);

            return Ok($"Dogadjaj sa id {id} je obrisan iz obe baze!");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Došlo je do greške pri radu sa bazama: {ex.Message}");
        }
    }

    [HttpPut("UpdateDogadjaj/{id}")]
public async Task<IActionResult> UpdateDogadjaj(Guid id, [FromBody] DogadjajDto updatedDogadjaj)
{
    // Provera za postojanje u Neo4j
    var dog = (await _client.Cypher
                            .Match("(d:Dogadjaj)")
                            .Where((DogadjajNeo d) => d.ID == id)
                            .OptionalMatch("(d)-[:DESIO_SE]->(g:Godina)")
                            .Return((d, g) => new
                            {
                                Dogadjaj = d.As<DogadjajNeo>(),
                                Godina = g.As<GodinaNeo>()
                            })
                            .ResultsAsync)
                            .FirstOrDefault();

    if (dog == null)
        return NotFound($"Dogadjaj sa ID {id} nije pronađen!");

    // Provera duplikata po imenu
    var duplikat = (await _client.Cypher
        .Match("(d:Dogadjaj)")
        .Where("toLower(d.Ime) = toLower($naziv) AND d.ID <> $id")
        .WithParam("naziv", updatedDogadjaj.Ime)
        .WithParam("id", id)
        .Return(d => d.As<DogadjajNeo>())
        .ResultsAsync)
        .Any();

    if (duplikat)
        return BadRequest($"Dogadjaj sa nazivom '{updatedDogadjaj.Ime}' već postoji u bazi!");

    // Neo4j update
    var cypher = _client.Cypher
                .Match("(d:Dogadjaj)")
                .Where((DogadjajNeo d) => d.ID == id)
                .Set("d.Ime = $ime, d.Tip=$tip")
                .Remove("d:Bitka:Rat:Sporazum:Savez:Dokument:Ustanak")
                .Set($"d:{updatedDogadjaj.Tip}")
                .WithParam("ime", updatedDogadjaj.Ime)
                .WithParam("tip", updatedDogadjaj.Tip);

    // Godina logika ostaje ista
    if (updatedDogadjaj.Godina != null && updatedDogadjaj.Godina.God != 0)
    {
        if (dog.Godina != null && dog.Godina.God != 0)
        {
            if (dog.Godina.God != updatedDogadjaj.Godina.God || dog.Godina.IsPNE != updatedDogadjaj.Godina.IsPNE)
            {
                await _godinaService.DodajGodinu(updatedDogadjaj.Godina.God, updatedDogadjaj.Godina.IsPNE);
                cypher = cypher.With("d")
                               .Match("(g:Godina {God: $dogGod, IsPNE: $dogPNE})")
                               .Match("(d)-[rel:DESIO_SE]->()")
                               .Delete("rel")
                               .Create("(d)-[:DESIO_SE]->(g)")
                               .WithParam("dogGod", updatedDogadjaj.Godina.God)
                               .WithParam("dogPNE", updatedDogadjaj.Godina.IsPNE);
            }
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

    // Lokacija logika ostaje ista
    if (!string.IsNullOrEmpty(updatedDogadjaj.Lokacija) && updatedDogadjaj.Lokacija != "string")
    {
        var zemljaPostoji = await _client.Cypher
                                         .Match("(z:Zemlja)")
                                         .Where("toLower(z.Naziv) = toLower($naziv)")
                                         .WithParam("naziv", updatedDogadjaj.Lokacija)
                                         .Return(z => z.As<ZemljaNeo>())
                                         .ResultsAsync;
        if (zemljaPostoji.Any())
        {
            if (!string.IsNullOrWhiteSpace(dog.Dogadjaj.Lokacija) && dog.Dogadjaj.Lokacija != "string")
            {
                if (dog.Dogadjaj.Lokacija != updatedDogadjaj.Lokacija)
                {
                    cypher = cypher
                             .With("d")
                             .Match("(z:Zemlja)")
                             .Where("toLower(z.Naziv) = toLower($naziv)")
                             .OptionalMatch("(d)-[rel:DESIO_SE_U]->()")
                             .Delete("rel")
                             .WithParam("naziv", updatedDogadjaj.Lokacija)
                             .Create("(d)-[:DESIO_SE_U]->(z)")
                             .Set("d.Lokacija = $naziv");
                }
            }
            else
            {
                cypher = cypher
                         .With("d")
                         .Match("(z:Zemlja)")
                         .Where("toLower(z.Naziv) = toLower($naziv)")
                         .WithParam("naziv", updatedDogadjaj.Lokacija)
                         .Create("(d)-[:DESIO_SE_U]->(z)")
                         .Set("d.Lokacija = $naziv");
            }
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

    // Izvrši Neo4j update
    await cypher.ExecuteWithoutResultsAsync();

    // MongoDB update Teksta
    if (!string.IsNullOrWhiteSpace(updatedDogadjaj.Tekst))
    {
        var update = Builders<DogadjajMongo>.Update.Set(m => m.Tekst, updatedDogadjaj.Tekst);
        await _mongo.UpdateOneAsync(m => m.ID == id, update, new MongoDB.Driver.UpdateOptions { IsUpsert = true });
    }

    return Ok($"Uspešno ažuriran događaj '{updatedDogadjaj.Ime}' sa ID: {id}!");
}

}