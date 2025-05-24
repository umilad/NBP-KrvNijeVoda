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
    // private readonly LokacijaService _lokacijaService;
    // private readonly ZemljaService _zemljaService;

    public RatController(Neo4jService neo4jService, GodinaService godinaService /*, LokacijaService lokacijaService, ZemljaService zemljaService*/)
    {
        _client = neo4jService.GetClient();
        _godinaService = godinaService;
        // _lokacijaService = lokacijaService;
        // _zemljaService = zemljaService;
    }

    [HttpPost("CreateRat")]
public async Task<IActionResult> CreateRat([FromBody] Rat rat)
{
    try
    {
        Godina godPocetak = null;
        Godina godKraj = null;
        var ratID = Guid.NewGuid();

        var query = _client.Cypher
            .Create($"(r:Dogadjaj:Rat {{ID: $id, Ime: $ime, Tip: 'Rat', Tekst: $tekst, Lokacija: $lokacija, Pobednik: $pobednik}})")
            .WithParam("id", ratID)
            .WithParam("ime", rat.Ime)
            .WithParam("pobednik", rat.Pobednik)
            .WithParam("tekst", rat.Tekst)
            .WithParam("lokacija", rat.Lokacija);

        if (!string.IsNullOrWhiteSpace(rat.Lokacija))
        {
            var zemljaPostoji = (await _client.Cypher
                .Match("(z:Zemlja)")
                .Where("toLower(z.Naziv) = toLower($naziv)")
                .WithParam("naziv", rat.Lokacija)
                .Return(z => z.As<Zemlja>())
                .ResultsAsync).Any();

            if (!zemljaPostoji)
                return BadRequest($"Zemlja '{rat.Lokacija}' ne postoji!");

            query = query
                .With("r")
                .Match("(z:Zemlja {Naziv: $nazivZemlje})")
                .Create("(r)-[:DESIO_SE_U]->(z)")
                .WithParam("nazivZemlje", rat.Lokacija);
        }

        if (rat.Godina != null)
        {
            godPocetak = await _godinaService.DodajGodinu(rat.Godina.God, rat.Godina.IsPNE);
            query = query
                .With("r")
                .Match("(g:Godina {ID: $idGodine})")
                .Create("(r)-[:DESIO_SE]->(g)")
                .WithParam("idGodine", godPocetak.ID);
        }

        if (rat.GodinaDo != null)
        {
            godKraj = await _godinaService.DodajGodinu(rat.GodinaDo.God, rat.GodinaDo.IsPNE);
            query = query
                .With("r")
                .Match("(g:Godina {ID: $idGodineDo})")
                .Create("(r)-[:RAT_TRAJAO_DO]->(g)")
                .WithParam("idGodineDo", godKraj.ID);
        }

        await query.ExecuteWithoutResultsAsync();

        // Poveži postojeće bitke
        if (rat.Bitke != null && rat.Bitke.Any())
        {
            foreach (var bitka in rat.Bitke)
            {
                var bitkaPostoji = (await _client.Cypher
                    .Match("(b:Bitka)")
                    .Where((Bitka b) => b.ID == bitka.ID)
                    .Return(b => b.As<Bitka>())
                    .ResultsAsync).Any();

                if (!bitkaPostoji)
                    return BadRequest($"Bitka sa ID '{bitka.ID}' ne postoji u bazi.");

                await _client.Cypher
                    .Match("(r:Rat {ID: $ratID})", "(b:Bitka {ID: $bitkaID})")
                    .Create("(b)-[:BITKA_U_RATU]->(r)")
                    .WithParams(new { ratID, bitkaID = bitka.ID })
                    .ExecuteWithoutResultsAsync();
            }
        }

        return Ok($"Rat '{rat.Ime}' je uspešno dodat i povezane su postojeće bitke.");
    }
    catch (Exception ex)
    {
        return StatusCode(500, $"Greška: {ex.Message}");
    }
}


    [HttpGet("GetRat/{id}")]
    public async Task<IActionResult> GetRat(Guid id)
    {
        try
        {
            var result = (await _client.Cypher
                .Match("(r:Dogadjaj:Rat)")
                .Where((Rat r) => r.ID == id)
                .OptionalMatch("(r)-[:DESIO_SE]->(g:Godina)")
                .OptionalMatch("(r)-[:RAT_TRAJAO_DO]->(gdo:Godina)")
                .OptionalMatch("(b:Dogadjaj:Bitka)-[:BITKA_U_RATU]->(r)")
                .OptionalMatch("(r)-[:DESIO_SE_U]->(z:Zemlja)")
                .Return((r, g, gdo, b) => new
                {
                    Rat = r.As<Rat>(),
                    GodinaOd = g.As<Godina>(),
                    GodinaDo = gdo.As<Godina>(),
                    Bitke = b.CollectAs<Bitka>()
                })
                .ResultsAsync).FirstOrDefault();

            if (result == null)
                return NotFound($"Rat sa ID {id} nije pronađen!");

            var rat = result.Rat;
            rat.Godina = result.GodinaOd;
            rat.GodinaDo = result.GodinaDo;
            rat.Bitke = result.Bitke.ToList();

            return Ok(rat);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Greška prilikom rada sa Neo4j bazom: {ex.Message}");
        }
    }

    [HttpGet("GetBitkeZaRat/{ratID}")]
public async Task<IActionResult> GetBitkeZaRat(Guid ratID)
{
    try
    {
        var result = await _client.Cypher
            .Match("(r:Rat)<-[:BITKA_U_RATU]-(b:Bitka)-[:DESIO_SE]->(g:Godina)")
            .Where((Rat r) => r.ID == ratID)
            .Return((b, g) => new
            {
                Bitka = b.As<Bitka>(),
                Godina = g.As<Godina>()
            })
            .ResultsAsync;

        if (!result.Any())
            return NotFound($"Nema bitki za rat sa ID: {ratID}");

        var bitkeSaGodinom = result.Select(r =>
        {
            r.Bitka.Godina = r.Godina;
            return r.Bitka;
        });

        return Ok(bitkeSaGodinom);
    }
    catch (Exception ex)
    {
        return StatusCode(500, $"Greška pri dohvatanju bitki: {ex.Message}");
    }
}


    [HttpDelete("DeleteRat/{id}")]
    public async Task<IActionResult> DeleteRat(Guid id)
    {
        try
        {
            var rat = (await _client.Cypher
                .Match("(r:Dogadjaj:Rat)")
                .Where((Rat r) => r.ID == id)
                .Return(r => r.As<Rat>())
                .ResultsAsync).FirstOrDefault();

            if (rat == null)
                return NotFound($"Rat sa ID {id} nije pronađen!");

            await _client.Cypher
                .Match("(r:Dogadjaj:Rat)")
                .Where((Rat r) => r.ID == id)
                .DetachDelete("r")
                .ExecuteWithoutResultsAsync();

            return Ok($"Rat sa ID {id} je uspešno obrisan!");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Došlo je do greške pri radu sa Neo4j bazom: {ex.Message}");
        }
    }

[HttpPut("UpdateRat/{id}")]
public async Task<IActionResult> UpdateRat(Guid id, [FromBody] Rat updatedRat)
{
    try
    {
        var rat = (await _client.Cypher
                                    .Match("(r:Dogadjaj:Rat)")
                                    .Where((Rat r) => r.ID == id)
                                    .OptionalMatch("(r)-[:DESIO_SE]->(g1:Godina)")          
                                    .OptionalMatch("(r)-[:RAT_TRAJAO_DO]->(g2:Godina)")     
                                    .Return((r, g1, g2) => new
                                    {
                                        Rat = r.As<Rat>(),
                                        PocetnaGodina = g1.As<Godina>(),
                                        KrajnjaGodina = g2.As<Godina>()
                                    })
                                    .ResultsAsync)
                                    .FirstOrDefault();

        if (rat == null)
            return NotFound($"Rat sa ID {id} nije pronađen!");

        var cypher = _client.Cypher
                            .Match("(r:Dogadjaj:Rat)")
                            .Where((Rat r) => r.ID == id)
                            .Set("r.Ime = $ime, r.Tekst = $tekst, r.Tip = 'Rat', r.Lokacija = $lokacija, r.Pobednik = $pobednik")
                            .With("r")
                            .WithParams(new
                            {
                                ime = updatedRat.Ime,
                                tekst = updatedRat.Tekst,
                                lokacija = updatedRat.Lokacija,
                                pobednik = updatedRat.Pobednik
                            });
        
        if (!string.IsNullOrEmpty(updatedRat.Lokacija))
        {
                if (rat.Rat.Lokacija != updatedRat.Lokacija)
                {
                    var zemljaPostoji = (await _client.Cypher
                        .Match("(z:Zemlja)")
                        .Where("toLower(z.Naziv) = toLower($naziv)")
                        .WithParam("naziv", updatedRat.Lokacija)
                        .Return(z => z.As<Zemlja>())
                        .ResultsAsync)
                        .Any();

                    if (!zemljaPostoji)
                        return BadRequest($"Zemlja sa nazivom '{updatedRat.Lokacija}' ne postoji!");

                    cypher = cypher
                        .With("r")
                        .OptionalMatch("(r)-[rel:DESIO_SE_U]->()")
                        .Delete("rel")
                        .With("r")
                        .Match("(z:Zemlja {Naziv: $nazivZemlje})")
                        .WithParam("nazivZemlje", updatedRat.Lokacija)
                        .Merge("(r)-[:DESIO_SE_U]->(z)");
                }
        }
                else
                {
                    cypher = cypher
                        .With("r")
                        .OptionalMatch("(r)-[rel:DESIO_SE_U]->()")
                        .Delete("rel");

                }

            if (updatedRat.Godina != null)
            {
                if (rat.PocetnaGodina.God != updatedRat.Godina.God || rat.PocetnaGodina.IsPNE != updatedRat.Godina.IsPNE)
                {
                    var god = await _godinaService.DodajGodinu(updatedRat.Godina.God, updatedRat.Godina.IsPNE);
                    cypher = cypher
                        .With("r")
                        .OptionalMatch("(r)-[rel:DESIO_SE]->()")
                        .Delete("rel")
                        .With("r")
                        .Match("(g:Godina {ID: $idGodine})")
                        .WithParam("idGodine", god.ID)
                        .Merge("(r)-[:DESIO_SE]->(g)");
                }
            }
            else
            {
                cypher = cypher
                    .With("r")
                    .OptionalMatch("(r)-[rel:DESIO_SE]->()")
                    .Delete("rel");
            }
        if (updatedRat.GodinaDo != null)
        {
                if (rat.KrajnjaGodina.God != updatedRat.Godina.God || rat.KrajnjaGodina.IsPNE != updatedRat.Godina.IsPNE)
                   {
                        var godDo = await _godinaService.DodajGodinu(updatedRat.GodinaDo.God, updatedRat.GodinaDo.IsPNE);
                        cypher = cypher
                            .With("r")
                            .OptionalMatch("(r)-[rel:RAT_TRAJAO_DO]->()")
                            .Delete("rel")
                            .With("r")
                            .Match("(gdo:Godina {ID: $idGodineDo})")
                            .WithParam("idGodineDo", godDo.ID)
                            .Merge("(r)-[:RAT_TRAJAO_DO]->(gdo)");
                    }
        }
        else
                {
                     cypher = cypher
                         .With("r")
                        .OptionalMatch("(r)-[rel:RAT_TRAJAO_DO]->()")
                         .Delete("rel");
                }
        await cypher.ExecuteWithoutResultsAsync();
        return Ok($"Rat '{updatedRat.Ime}' uspešno ažuriran!");
    }
    catch (Exception ex)
    {
        return StatusCode(500, $"Došlo je do greške pri radu sa bazom: {ex.Message}");
    }
}


}