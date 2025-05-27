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
            var r = (await _client.Cypher.Match("(r:Dogadjaj:Rat)")
                                         .Where((Rat r) => r.Ime == rat.Ime)
                                         .Return(r => r.As<Rat>())
                                         .ResultsAsync)
                                         .FirstOrDefault();

            if (r != null)
            {
                return BadRequest($"Rat sa imenom {rat.Ime} vec postoji u bazi!");
            }

            Godina godPocetak = null;
            Godina godKraj = null;
            Guid ratID = Guid.NewGuid();

            var query = _client.Cypher
                .Create($"(r:Dogadjaj:Rat {{ID: $id, Ime: $ime, Tip: 'Rat', Tekst: $tekst, Lokacija: $lokacija, Pobednik: $pobednik}})")
                .WithParam("id", ratID)
                .WithParam("ime", rat.Ime)
                .WithParam("pobednik", rat.Pobednik)
                .WithParam("tekst", rat.Tekst)
                .WithParam("lokacija", rat.Lokacija);//valjda ostaje prazno ako se ne unese nista 

                if (!string.IsNullOrEmpty(rat.Lokacija) && rat.Lokacija != "string")
                {
                    var zemljaPostoji = (await _client.Cypher
                        .Match("(z:Zemlja)")
                        .Where("toLower(z.Naziv) = toLower($naziv)")
                        .WithParam("naziv", rat.Lokacija)
                        .Return(z => z.As<Zemlja>())
                        .ResultsAsync)
                        .Any();

                    if (zemljaPostoji)
                    {
                        query = query.With("r")
                                     .Match("(z:Zemlja)")
                                     .Where("toLower(z.Naziv) = toLower($nazivZemlje)")
                                     .Create("(r)-[:DESIO_SE_U]->(z)")
                                     .WithParam("nazivZemlje", rat.Lokacija)
                                     .Set("r.Lokacija = $nazivZemlje");
                    }
                    else
                        query = query.With("r")
                                     .Set("r.Lokacija = $nazivZemlje")
                                     .WithParam("nazivZemlje", "string"); 
                }


            if (rat.Godina != null && rat.Godina.God != 0)
            {
                godPocetak = await _godinaService.DodajGodinu(rat.Godina.God, rat.Godina.IsPNE);
                query = query
                    .With("r")
                    .Match("(g:Godina {ID: $idGodine})")
                    .Create("(r)-[:DESIO_SE]->(g)")
                    .WithParam("idGodine", godPocetak.ID);
            }

            if (rat.GodinaDo != null && rat.GodinaDo.God != 0)
            {
                godKraj = await _godinaService.DodajGodinu(rat.GodinaDo.God, rat.GodinaDo.IsPNE);
                query = query
                    .With("r")
                    .Match("(g:Godina {ID: $idGodineDo})")
                    .Create("(r)-[:RAT_TRAJAO_DO]->(g)")
                    .WithParam("idGodineDo", godKraj.ID);
            }

            await query.ExecuteWithoutResultsAsync();

            return Ok($"Rat '{rat.Ime}' je uspešno dodat u bazu!");

                // Poveži postojeće bitke
                //POPRAVLJENO ALI BEZ DODAVANJA U LISTU I TREBA DA BUDE DOGADJAJ GENERALNO NE BITKA
                // if (rat.Bitke != null && rat.Bitke.Any())
                // {
                //     foreach (var bitka in rat.Bitke)
                //     {
                //         var bitkaPostoji = (await _client.Cypher
                //                     .Match("(b:Bitka)")
                //                     .Where("toLower(b.Ime) = toLower(bitka.Ime)")
                //                     .Return(b => b.As<Bitka>())
                //                     .ResultsAsync).Any();

                //         if (bitkaPostoji)
                //             await _client.Cypher
                //                     .Match("(r:Rat {ID: $ratID})")
                //                     .Match("(b:Bitka)")
                //                     .Where("toLower(b.Ime) = toLower(bitka.Ime)")
                //                     .Create("(b)-[:BITKA_U_RATU]->(r)")
                //                     .WithParams("ratID", ratID)
                //                     .ExecuteWithoutResultsAsync();

                //     }
                // }

                //return Ok($"Rat '{rat.Ime}' je uspešno dodat i povezane su postojeće bitke.");
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
                //.OptionalMatch("(r)-[:DESIO_SE_U]->(z:Zemlja)")
                .Return((r, g, gdo, b) => new
                {
                    Rat = r.As<Rat>(),
                    GodinaOd = g.As<Godina>(),
                    GodinaDo = gdo.As<Godina>(),
                    Bitke = b.CollectAs<Bitka>()
                })
                .ResultsAsync).FirstOrDefault();

            if (result == null)
                return NotFound($"Rat sa ID: {id} nije pronađen!");

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

    //NISAM GLEDALA
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
                return NotFound($"Rat sa ID: {id} nije pronađen!");

            await _client.Cypher
                .Match("(r:Dogadjaj:Rat)")
                .Where((Rat r) => r.ID == id)
                .DetachDelete("r")
                .ExecuteWithoutResultsAsync();

            return Ok($"Rat sa ID: {id} je uspešno obrisan!");
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
                return NotFound($"Rat sa ID: {id} nije pronađen!");

            var cypher = _client.Cypher
                                .Match("(r:Dogadjaj:Rat)")
                                .Where((Rat r) => r.ID == id)
                                .Set("r.Ime = $ime, r.Tekst = $tekst, r.Tip = 'Rat', r.Lokacija = $lokacija, r.Pobednik = $pobednik")
                                .With("r")
                                .WithParams(new
                                {
                                    ime = updatedRat.Ime,
                                    tekst = updatedRat.Tekst,
                                    lokacija = rat.Rat.Lokacija,//stara lokacija da bi ona ostala ako bude neki random unos 
                                    pobednik = updatedRat.Pobednik
                                });

            if (!string.IsNullOrEmpty(updatedRat.Lokacija) && updatedRat.Lokacija != "string")//uneto je nesto
            {
                var zemljaPostoji = (await _client.Cypher
                            .Match("(z:Zemlja)")
                            .Where("toLower(z.Naziv) = toLower($naziv)")
                            .WithParam("naziv", updatedRat.Lokacija)
                            .Return(z => z.As<Zemlja>())
                            .ResultsAsync)
                            .Any();
                if (zemljaPostoji)//ta lokacija postoji u bazi 
                {
                    if (!string.IsNullOrEmpty(rat.Rat.Lokacija) && rat.Rat.Lokacija != "string")//postoji vec neka lokacija u bazi
                    {
                        if (rat.Rat.Lokacija.ToLower() != updatedRat.Lokacija.ToLower())//promenjena lokacija
                        {
                            cypher = cypher
                                .With("r")
                                .OptionalMatch("(r)-[rel:DESIO_SE_U]->()")
                                .Match("(z:Zemlja)")
                                .Where("toLower(z.Naziv) = toLower($nazivZemlje)")
                                .Delete("rel")
                                .WithParam("nazivZemlje", updatedRat.Lokacija)
                                .Create("(r)-[:DESIO_SE_U]->(z)")
                                .Set("r.Lokacija = $nazivZemlje");
                        }
                        //else ista je lokacija
                    }
                    else//nista nije bilo samo dodajemo 
                    {
                        cypher = cypher.With("r")
                                .Match("(z:Zemlja)")
                                .Where("toLower(z.Naziv) = toLower($nazivZemlje)")
                                .WithParam("nazivZemlje", updatedRat.Lokacija)
                                .Create("(r)-[:DESIO_SE_U]->(z)")
                                .Set("r.Lokacija = $nazivZemlje");
                    }
                }
                //else//lokacija nije nadjena u bazi ostavlja staru lokaciju jer sam na nju postavila na pocetku 
            }
            else //nije uneto nista 
            {
                cypher = cypher
                    .With("r")
                    .OptionalMatch("(r)-[rel:DESIO_SE_U]->()")
                    .Delete("rel")
                    .Set("r.Lokacija = 'string'");
            }

            if (updatedRat.Godina != null && updatedRat.Godina.God != 0)//uneta godina 
            {
                if (rat.PocetnaGodina != null && rat.PocetnaGodina.God != 0)//vec je bila neka godina 
                {
                    if (rat.PocetnaGodina.God != updatedRat.Godina.God || rat.PocetnaGodina.IsPNE != updatedRat.Godina.IsPNE)
                    {//promenjena je 
                        var godina = await _godinaService.DodajGodinu(updatedRat.Godina.God, updatedRat.Godina.IsPNE);
                        cypher = cypher
                            .With("r")
                            .OptionalMatch("(r)-[rel:DESIO_SE]->()")
                            .Match("(g:Godina {ID: $godinaId})")
                            .Delete("rel")
                            .WithParam("godinaId", godina.ID)
                            .Create("(r)-[:DESIO_SE]->(g)");
                    }
                    //else nije promenjena nista ne diramo 
                }
                else//nije bila godina samo unosimo novu 
                {
                    var godina = await _godinaService.DodajGodinu(updatedRat.Godina.God, updatedRat.Godina.IsPNE);
                    cypher = cypher
                        .With("r")
                        .Match("(g:Godina {ID: $godinaId})")
                        .WithParam("godinaId", godina.ID)
                        .Create("(r)-[:DESIO_SE]->(g)");
                }
            }
            else
            {
                cypher = cypher
                    .With("r")
                    .OptionalMatch("(r)-[rel:DESIO_SE]->()")
                    .Delete("rel");
            }

            if (updatedRat.GodinaDo != null && updatedRat.GodinaDo.God != 0)//uneta godina 
            {
                if (rat.KrajnjaGodina != null && rat.KrajnjaGodina.God != 0)//vec je bila neka godina 
                {
                    if (rat.KrajnjaGodina.God != updatedRat.GodinaDo.God || rat.KrajnjaGodina.IsPNE != updatedRat.GodinaDo.IsPNE)
                    {//promenjena je 
                        var godina = await _godinaService.DodajGodinu(updatedRat.GodinaDo.God, updatedRat.GodinaDo.IsPNE);
                        cypher = cypher
                            .With("r")
                            .OptionalMatch("(r)-[rel:RAT_TRAJAO_DO]->()")
                            .Match("(gdo:Godina {ID: $godinaId})")
                            .Delete("rel")
                            .WithParam("godinaId", godina.ID)
                            .Create("(r)-[:RAT_TRAJAO_DO]->(gdo)");
                    }
                    //else nije promenjena nista ne diramo 
                }
                else//nije bila godina samo unosimo novu 
                {
                    var godina = await _godinaService.DodajGodinu(updatedRat.GodinaDo.God, updatedRat.GodinaDo.IsPNE);
                    cypher = cypher
                        .With("r")
                        .Match("(gdo:Godina {ID: $godinaId})")
                        .WithParam("godinaId", godina.ID)
                        .Create("(r)-[:RAT_TRAJAO_DO]->(gdo)");
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