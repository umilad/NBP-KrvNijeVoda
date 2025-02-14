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
    private readonly LokacijaService _lokacijaService;
    private readonly ZemljaService _zemljaService;

    public DogadjajController(Neo4jService neo4jService, GodinaService godinaService, LokacijaService lokacijaService, ZemljaService zemljaService)
    {
        _client = neo4jService.GetClient();
        _godinaService = godinaService;
        _lokacijaService = lokacijaService;
        _zemljaService = zemljaService;
    }

    [HttpPost("CreateDogadjaj")]
    public async Task<IActionResult> CreateDogadjaj([FromBody] Dogadjaj dogadjaj)
    {
        await _godinaService.DodajGodinu(dogadjaj.Godina.God);
        await _zemljaService.DodajZemlju(dogadjaj.Lokacija.PripadaZemlji);
        var lokacija = await _lokacijaService.DodajLokaciju(dogadjaj.Lokacija.Naziv, dogadjaj.Lokacija.PripadaZemlji);
        
       string label = dogadjaj.Tip.ToString();
        var dogadjajID = Guid.NewGuid();
        await _client.Cypher
            .Match("(g:Godina {God: $godina})", "(l:Lokacija {ID: $idLokacije})")
            .Create($"(d:Dogadjaj:{label} {{ID: $id, Ime: $ime, Tip: $tip, Tekst: $tekst}}) -[:DOGADJAJ_U_GODINI]-> (g), (d) -[:DOGADJAJ_NA_LOKACIJI]-> (l)")
            .WithParam("id", dogadjajID)
            .WithParam("ime", dogadjaj.Ime)
            .WithParam("tip", dogadjaj.Tip.ToString())
            .WithParam("tekst", dogadjaj.Tekst)
            .WithParam("godina", dogadjaj.Godina.God)
            .WithParam("idLokacije", lokacija.ID)
            .ExecuteWithoutResultsAsync();
        
         return Ok($"Uspesno dodat dogadjaj sa id:{dogadjajID} u bazu!");
    }

    [HttpGet("GetDogadjaj/{id}")]
    public async Task<IActionResult> GetDogadjaj(Guid id)
    {
        var dog = (await _client.Cypher.Match("(d:Dogadjaj)")
                                       .Where((Dogadjaj d) => d.ID == id)
                                       .OptionalMatch("(d)-[:DOGADJAJ_U_GODINI]->(g:Godina)") 
                                       .OptionalMatch("(d)-[:DOGADJAJ_NA_LOKACIJI]->(l:Lokacija)-[:PRIPADA_ZEMLJI]->(z:Zemlja)")
                                       .Return((d, g, l, z) => new {
                                                 Dogadjaj = d.As<Dogadjaj>(),
                                                 DogadjajUGodini = g.As<Godina>(),
                                                 DogadjajNaLokaciji = l.As<Lokacija>(),
                                                 Zemlja = z.As<Zemlja>() 
                                             }) 
                                       .ResultsAsync)
                                       .FirstOrDefault();
        if (dog == null)
        {
            return BadRequest($"Nije pronadjen nijedan dogadjaj sa id: {id}");
        }
       if(dog.DogadjajNaLokaciji != null)
            dog.DogadjajNaLokaciji.PripadaZemlji = dog.Zemlja ?? new Zemlja();
        var result = new Dogadjaj {
                    ID = dog.Dogadjaj.ID,
                    Ime = dog.Dogadjaj.Ime,
                    Tip = dog.Dogadjaj.Tip,
                    Tekst= dog.Dogadjaj.Tekst,
                    Godina = dog.DogadjajUGodini ?? new Godina(),
                    Lokacija= dog.DogadjajNaLokaciji ?? new Lokacija()
                };
        return Ok(result);
    }
     [HttpDelete("DeleteDogadjaj/{id}")]
    public async Task<IActionResult> DeleteDogadjaj(Guid id)
    {
        await _client.Cypher.Match("(d:Dogadjaj)")
                            .Where((Dogadjaj d) => d.ID == id)
                            .OptionalMatch("(d)-[r:DOGADJAJ_U_GODINI]->(g:Godina)")
                            .OptionalMatch("(d)-[r2:DOGADJAJ_NA_LOKACIJI]->(l:Lokacija)")
                            .Delete("r, r2, d")//r3
                            .ExecuteWithoutResultsAsync();
        return Ok($"Dogadjaj sa id {id} je obrisan!");
    }
    [HttpGet("GetAllDogadjaje")]
public async Task<IActionResult> GetDogadjaje()
{
    var dogadjaji = (await _client.Cypher.Match("(d:Dogadjaj)")
                                       .OptionalMatch("(d)-[:DOGADJAJ_U_GODINI]->(g:Godina)") 
                                       .OptionalMatch("(d)-[:DOGADJAJ_NA_LOKACIJI]->(l:Lokacija)-[:PRIPADA_ZEMLJI]->(z:Zemlja)")
                                       .Return((d, g, l, z) => new {
                                                 Dogadjaj = d.As<Dogadjaj>(),
                                                 DogadjajUGodini = g.As<Godina>(),
                                                 DogadjajNaLokaciji = l.As<Lokacija>(),
                                                 Zemlja = z.As<Zemlja>() 
                                             }) 
                                       .ResultsAsync)
                                       .ToList();

    if (dogadjaji == null || !dogadjaji.Any())
    {
        return BadRequest("Nije pronađen nijedan događaj");
    }

    var result = dogadjaji.Select(item =>
    {
        if (item.DogadjajNaLokaciji != null)
        {
            item.DogadjajNaLokaciji.PripadaZemlji = item.Zemlja ?? new Zemlja();
        }

        return new Dogadjaj
        {
            ID = item.Dogadjaj.ID,
            Ime = item.Dogadjaj.Ime,
            Tip = item.Dogadjaj.Tip,
            Tekst = item.Dogadjaj.Tekst,
            Godina = item.DogadjajUGodini ?? new Godina(),
            Lokacija = item.DogadjajNaLokaciji ?? new Lokacija()
        };
    }).ToList();

    return Ok(result);
}

}