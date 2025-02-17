using Microsoft.AspNetCore.Mvc;
using Neo4jClient;
using KrvNijeVoda.Back.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

[Route("api")]
[ApiController]
public class BitkaController : ControllerBase
{
    private readonly IGraphClient _client;
    private readonly GodinaService _godinaService;
    private readonly LokacijaService _lokacijaService;
    private readonly ZemljaService _zemljaService;
    private readonly RatService _ratService;

    public BitkaController(Neo4jService neo4jService, GodinaService godinaService, LokacijaService lokacijaService, RatService ratService, ZemljaService zemljaService)
    {
        _client = neo4jService.GetClient();
        _godinaService = godinaService;
        _lokacijaService = lokacijaService;
        _ratService = ratService;
        _zemljaService=zemljaService;
    }

[HttpPost("CreateBitka")]
public async Task<IActionResult> CreateBitka([FromBody] Bitka bitka)
{
    try
    {
        var postojecaBitka = (await _client.Cypher
            .Match("(b:Bitka)")
            .Where("b.Ime = $ime")
            .WithParam("ime", bitka.Ime)
            .Return(b => b.As<Bitka>())
            .ResultsAsync)
            .FirstOrDefault();

        if (postojecaBitka != null)
            return BadRequest($"Bitka '{bitka.Ime}' već postoji u bazi!");

        await _godinaService.DodajGodinu(bitka.Godina.God);
        
        if (bitka.GodinaDo != null)
        {
            await _godinaService.DodajGodinu(bitka.GodinaDo.God);
        }
        await _zemljaService.DodajZemlju(bitka.Lokacija.PripadaZemlji);
        
        var lokacija = await _lokacijaService.DodajLokaciju(bitka.Lokacija.Naziv, bitka.Lokacija.PripadaZemlji);
        if (lokacija == null)
            return BadRequest("Greška pri dodavanju lokacije!");

        Guid? ratID = null;

        if (bitka.Rat != null)
        {
            var rat = await _ratService.DodajRat(bitka.Rat, bitka.Lokacija);
            if (rat == null)
                return BadRequest("Greška pri dodavanju rata!");
            ratID = rat.ID;
        }

        var bitkaID = Guid.NewGuid();

        if (bitka.Godina == null)
            return BadRequest("Godina bitke nije navedena!");

        if (bitka.Lokacija == null)
            return BadRequest("Lokacija bitke nije navedena!");

        var cypher = _client.Cypher
            .Match("(g:Godina {God: $godina})", "(l:Lokacija {ID: $idLokacije})")
            .WithParam("id", bitkaID)
            .WithParam("ime", bitka.Ime)
            .WithParam("tekst", bitka.Tekst)
            .WithParam("pobednik", bitka.Pobednik)
            .WithParam("godina", bitka.Godina.God)
            .WithParam("idLokacije", lokacija.ID)
            .With("g, l");  
        if (ratID != null)
        {
            cypher = cypher.Match("(r:Rat {ID: $idRata})")
                        .WithParam("idRata", ratID)
                        .Create("(b:Dogadjaj:Bitka {ID: $id, Ime: $ime, Tip: 'Bitka', Tekst: $tekst, Pobednik: $pobednik}) " +
                                "-[:DESIO_SE]-> (g), (b) -[:DESIO_SE_U]-> (l), (b) -[:PRIPADA_RATU]-> (r)");
        }
        else
        {
            cypher = cypher.Create("(b:Dogadjaj:Bitka {ID: $id, Ime: $ime, Tip: 'Bitka', Tekst: $tekst, Pobednik: $pobednik}) " +
                                "-[:DESIO_SE]-> (g), (b) -[:DESIO_SE_U]-> (l)");
        }

        if (bitka.GodinaDo != null)
        {
            cypher = cypher.With("b")
                        .Match("(g2:Godina {God: $godinaDo})")
                        .Create("(b) -[:BITKA_TRAJALA_DO]-> (g2)")
                        .WithParam("godinaDo", bitka.GodinaDo.God);    
        }

        await cypher.ExecuteWithoutResultsAsync();
        return Ok($"Uspešno dodata bitka '{bitka.Ime}' sa ID: {bitkaID}!");
    }
    catch (Exception ex)
    {
        return StatusCode(500, $"Došlo je do greške pri ažuriranju u Neo4j bazi: {ex.Message}");
    }
}


    // *** READ - Get Bitka by ID ***
    [HttpGet("GetBitka/{id}")]
    public async Task<IActionResult> GetBitka(Guid id)
    {
        try
        {
            var bitka = (await _client.Cypher.Match("(b:Bitka)")
                                            .Where((Bitka b) => b.ID == id)
                                            .OptionalMatch("(b)-[:DESIO_SE]->(g:Godina)")
                                            .OptionalMatch("(b)-[:BITKA_TRAJALA_DO]->(gd:Godina)")
                                            .OptionalMatch("(b)-[:DESIO_SE_U]->(l:Lokacija)-[:PRIPADA_ZEMLJI]->(z:Zemlja)")
                                            .OptionalMatch("(b)-[:PRIPADA_RATU]->(r:Rat)")
                                            .Return((b, g, gd, l, z, r) => new {
                                                Bitka = b.As<Bitka>(),
                                                PocetnaGodina = g.As<Godina>(),
                                                ZavrsnaGodina = gd.As<Godina>(),
                                                Lokacija = l.As<Lokacija>(),
                                                Zemlja = z.As<Zemlja>(),
                                                Rat = r.As<Rat>()
                                            })
                                            .ResultsAsync)
                                            .FirstOrDefault();

            if (bitka == null)
                return BadRequest($"Bitka sa ID {id} nije pronađena!");

            if (bitka.Lokacija != null)
                bitka.Lokacija.PripadaZemlji = bitka.Zemlja ?? new Zemlja();

            var result = new Bitka
            {
                ID = bitka.Bitka.ID,
                Ime = bitka.Bitka.Ime,
                Tip= bitka.Bitka.Tip,
                Tekst = bitka.Bitka.Tekst,
                Pobednik = bitka.Bitka.Pobednik,
                Godina = bitka.PocetnaGodina ?? new Godina(),
                GodinaDo = bitka.ZavrsnaGodina,
                Lokacija = bitka.Lokacija ?? new Lokacija(),
                Rat = bitka.Rat
            };
            return Ok(result);
        }
    catch (Exception ex)
    {
        return StatusCode(500, $"Došlo je do greške pri ažuriranju u Neo4j bazi: {ex.Message}");
    }
    }

    // *** DELETE ***
    [HttpDelete("DeleteBitka/{id}")]
    public async Task<IActionResult> DeleteBitka(Guid id)
    {
        try 
        {
            await _client.Cypher.Match("(b:Bitka)")
                                .Where((Bitka b) => b.ID == id)
                                .OptionalMatch("(b)-[r1:DESIO_SE]->(g:Godina)")
                                .OptionalMatch("(b)-[r2:BITKA_TRAJALA_DO]->(gd:Godina)")
                                .OptionalMatch("(b)-[r3:DESIO_SE_U]->(l:Lokacija)")
                                .OptionalMatch("(b)-[r4:PRIPADA_RATU]->(r:Rat)")
                                .Delete("r1, r2, r3, r4, b")
                                .ExecuteWithoutResultsAsync();
            return Ok($"Bitka sa ID {id} je obrisana!");
        }
    catch (Exception ex)
    {
        return StatusCode(500, $"Došlo je do greške pri ažuriranju u Neo4j bazi: {ex.Message}");
    }    
    }

    // *** READ - Get All Bitke ***
    [HttpGet("GetAllBitke")]
    public async Task<IActionResult> GetAllBitke()
    {
        try
        {
            var bitke = (await _client.Cypher.Match("(b:Bitka)")
                                            .OptionalMatch("(b)-[:DESIO_SE]->(g:Godina)")
                                            .OptionalMatch("(b)-[:BITKA_TRAJALA_DO]->(gd:Godina)")
                                            .OptionalMatch("(b)-[:DESIO_SE_U]->(l:Lokacija)-[:PRIPADA_ZEMLJI]->(z:Zemlja)")
                                            .OptionalMatch("(b)-[:PRIPADA_RATU]->(r:Rat)")
                                            .Return((b, g, gd, l, z, r) => new {
                                                Bitka = b.As<Bitka>(),
                                                PocetnaGodina = g.As<Godina>(),
                                                ZavrsnaGodina = gd.As<Godina>(),
                                                Lokacija = l.As<Lokacija>(),
                                                Zemlja = z.As<Zemlja>(),
                                                Rat = r.As<Rat>()
                                            })
                                            .ResultsAsync)
                                            .ToList();

            if (!bitke.Any())
                return BadRequest("Nema bitaka u bazi!");

            var result = bitke.Select(item =>
            {
                if (item.Lokacija != null)
                    item.Lokacija.PripadaZemlji = item.Zemlja ?? new Zemlja();

                if (item.Lokacija != null)
                                item.Lokacija.PripadaZemlji = item.Zemlja ?? new Zemlja();
                return new Bitka
                {
                    ID = item.Bitka.ID,
                    Ime = item.Bitka.Ime,
                    Tip= item.Bitka.Tip,
                    Tekst = item.Bitka.Tekst,
                    Pobednik = item.Bitka.Pobednik,
                    Godina = item.PocetnaGodina ?? new Godina(),
                    GodinaDo = item.ZavrsnaGodina,
                    Lokacija = item.Lokacija ?? new Lokacija(),
                    Rat = item.Rat
                };
            }).ToList();

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Došlo je do greške pri ažuriranju u Neo4j bazi: {ex.Message}");
        }
    }

[HttpPut("UpdateBitka/{id}")]
public async Task<IActionResult> UpdateBitka(Guid id, [FromBody] Bitka updatedBitka)
{
    try
    {
        if (updatedBitka == null)
            return BadRequest("Podaci za ažuriranje nisu poslati.");

        var bitka = (await _client.Cypher
            .Match("(b:Bitka)")
            .Where((Bitka b) => b.ID == id)
            .Return(b => b.As<Bitka>())
            .ResultsAsync)
            .FirstOrDefault();

        if (bitka == null)
            return NotFound($"Bitka sa ID: {id} nije pronađena.");

        var cypher = _client.Cypher
            .Match("(b:Bitka)")
            .Where((Bitka b) => b.ID == id)
            .Set("b.Ime = $ime, b.Tekst = $tekst, b.Pobednik = $pobednik")
            .WithParam("ime", updatedBitka.Ime)
            .WithParam("tekst", updatedBitka.Tekst)
            .WithParam("pobednik", updatedBitka.Pobednik);

        // Ažuriranje godine
        if (updatedBitka.Godina != null)
        {
            await _godinaService.DodajGodinu(updatedBitka.Godina.God);
            cypher = cypher
                .With("b") // Potrebno da bi nastavili sa MATCH
                .Match("(g:Godina {God: $godina})")
                .OptionalMatch("(b)-[rel:DESIO_SE]->()")
                .Delete("rel")
                .Merge("(b)-[:DESIO_SE]->(g)")
                .WithParam("godina", updatedBitka.Godina.God);
        }

        if (updatedBitka.GodinaDo != null)
        {
            await _godinaService.DodajGodinu(updatedBitka.GodinaDo.God);
            cypher = cypher
                .With("b")
                .Match("(gd:Godina {God: $godinaDo})")
                .OptionalMatch("(b)-[rel:BITKA_TRAJALA_DO]->()")
                .Delete("rel")
                .Merge("(b)-[:BITKA_TRAJALA_DO]->(gd)")
                .WithParam("godinaDo", updatedBitka.GodinaDo.God);
        }

        // Ažuriranje lokacije
        if (updatedBitka.Lokacija != null)
        {
            await _zemljaService.DodajZemlju(updatedBitka.Lokacija.PripadaZemlji);
            var lokacija = await _lokacijaService.DodajLokaciju(updatedBitka.Lokacija.Naziv, updatedBitka.Lokacija.PripadaZemlji);
            cypher = cypher
                .With("b")
                .Match("(l:Lokacija {ID: $idLokacije})")
                .OptionalMatch("(b)-[rel:DESIO_SE_U]->()")
                .Delete("rel")
                .Merge("(b)-[:DESIO_SE_U]->(l)")
                .WithParam("idLokacije", lokacija.ID);
        }
    Guid? ratID = null;
    if (updatedBitka.Rat != null)
    {
        var rat = await _ratService.DodajRat(updatedBitka.Rat, updatedBitka.Lokacija);
        ratID = rat.ID;
    }
    if (ratID != null)
    {
        cypher = cypher.With("b")
                        .Match("(r:Rat {ID: $idRata})")
                        .OptionalMatch("(b)-[rel:PRIPADA_RATU]->()")
                        .Delete("rel")
                        .WithParam("idRata", ratID)
                        .Merge("(b)-[:PRIPADA_RATU]-> (r)");
                       //.With("r");
    }

    await cypher.ExecuteWithoutResultsAsync();
    return Ok($"Uspešno ažurirana bitka '{bitka.Ime}' sa ID: {id}!");
    }
    catch (Exception ex)
    {
        return StatusCode(500, $"Došlo je do greške pri ažuriranju u Neo4j bazi: {ex.Message}");
    }
}

}
