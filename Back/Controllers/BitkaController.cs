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
    // private readonly LokacijaService _lokacijaService;
    // private readonly ZemljaService _zemljaService;
    // private readonly RatService _ratService;

    public BitkaController(Neo4jService neo4jService, GodinaService godinaService /*, LokacijaService lokacijaService, RatService ratService, ZemljaService zemljaService*/)
    {
        _client = neo4jService.GetClient();
        _godinaService = godinaService;
        // _lokacijaService = lokacijaService;
        // _ratService = ratService;
        // _zemljaService=zemljaService;
    }

    [HttpPost("CreateBitka")]
    public async Task<IActionResult> CreateBitka([FromBody] Bitka bitka)
    {
        try
        {
            //ovo ide na front
            // if (string.IsNullOrEmpty(bitka.Pobednik))
            //     return BadRequest("Polje 'Pobednik' je obavezno.");

            var bit = (await _client.Cypher.Match("(b:Dogadjaj:Bitka)")
                                           .Where((Bitka b) => b.Ime == bitka.Ime)
                                           .Return(b => b.As<Bitka>())
                                           .ResultsAsync)
                                           .FirstOrDefault();

            if (bit != null)
            {
                return BadRequest($"Bitka sa imenom {bitka.Ime} vec postoji u bazi!");
            }

            var query = _client.Cypher
                .Create("(b:Dogadjaj:Bitka {ID: $id, Ime: $ime, Tip: 'Bitka', Tekst: $tekst, Pobednik: $pobednik, BrojZrtava: $brojZrtava})")
                .WithParam("id", Guid.NewGuid())
                .WithParam("ime", bitka.Ime)
                .WithParam("pobednik", bitka.Pobednik)
                .WithParam("tekst", bitka.Tekst)
                .WithParam("brojZrtava", bitka.BrojZrtava);
                //MOZE DA SE DODA DA SETUJE I LOKACIJU OVDE ODMAH NA "string" 
                // DA AKO SE NE SETUJE NA NES NORMALNO OSTANE STRING OVAKO SE NE SECAM DA L OSTAJE NULL ILI STA 

            if (!string.IsNullOrEmpty(bitka.Lokacija) && bitka.Lokacija != "string")
            {
                var zemljaPostoji = (await _client.Cypher
                    .Match("(z:Zemlja)")
                    .Where("toLower(z.Naziv) = toLower($naziv)")
                    .WithParam("naziv", bitka.Lokacija)
                    .Return(z => z.As<Zemlja>())
                    .ResultsAsync)
                    .Any();

                if (zemljaPostoji)
                {
                    query = query.With("b")
                                 .Match("(z:Zemlja)")
                                 .Where("toLower(z.Naziv) = toLower($nazivZemlje)")
                                 .Create("(b)-[:DESIO_SE_U]->(z)")
                                 .WithParam("nazivZemlje", bitka.Lokacija)
                                 .Set("b.Lokacija = $nazivZemlje");
                }
                else
                    query = query.With("b")
                                 .Set("b.Lokacija = $nazivZemlje")
                                 .WithParam("nazivZemlje", "string");
            }
            
            if (!string.IsNullOrEmpty(bitka.Rat) && bitka.Rat != "string")
            {
                var ratPostoji = (await _client.Cypher
                    .Match("(r:Dogadjaj:Rat)")
                    .Where("toLower(r.Ime) = toLower($ratIme)")
                    .WithParam("ratIme", bitka.Rat)
                    .Return(r => r.As<Rat>())
                    .ResultsAsync)
                    .Any();

                if (ratPostoji != null)
                    query = query
                    .With("b")
                    .Match("(r:Rat)")
                    .Where("toLower(r.Ime) = toLower($ratIme)")
                    .WithParam("ratIme", bitka.Rat)
                    .Create("(b)-[:BITKA_U_RATU]->(r)")
                    .Set("b.Rat = $ratIme");
                else
                    query = query.With("b")
                                 .Set("b.Rat = $ratIme")
                                 .WithParam("ratIme", "string");
            }

            if (bitka.Godina != null && bitka.Godina.God != 0)
            {
                var godina = await _godinaService.DodajGodinu(bitka.Godina.God, bitka.Godina.IsPNE);
                query = query
                    .With("b")
                    .Match("(g:Godina {ID: $idGodine})")
                    .Create("(b)-[:DESIO_SE]->(g)")
                    .WithParam("idGodine", godina.ID);
            }


            await query.ExecuteWithoutResultsAsync();

            return Ok($"Bitka '{bitka.Ime}' je uspešno dodata!");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Greška: {ex.Message}");
        }
    }


    [HttpDelete("DeleteBitka/{id}")]
    public async Task<IActionResult> DeleteBitka(Guid id)
    {
        try
        {
            var bitka = (await _client.Cypher
                .Match("(b:Dogadjaj:Bitka)")
                .Where((Bitka b) => b.ID == id)
                .Return(b => b.As<Bitka>())
                .ResultsAsync).FirstOrDefault();

            if (bitka == null)
                return NotFound($"Bitka sa ID {id} nije pronađena!");

            await _client.Cypher
                .Match("(b:Dogadjaj:Bitka)")
                .Where((Bitka b) => b.ID == id)
                .DetachDelete("b")
                .ExecuteWithoutResultsAsync();

            return Ok($"Bitka sa ID: {id} je uspešno obrisana!");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Došlo je do greške pri radu sa Neo4j bazom: {ex.Message}");
        }
    }

    [HttpGet("GetBitka/{id}")]
    public async Task<IActionResult> GetBitka(Guid id)
    {
        try
        {
            var bitkaResult = (await _client.Cypher
                .Match("(b:Dogadjaj:Bitka)")
                .Where((Bitka b) => b.ID == id)
                .OptionalMatch("(b)-[:DESIO_SE]->(g:Godina)")
                //.OptionalMatch("(b)-[:DESIO_SE_U]->(z:Zemlja)")
                //.OptionalMatch("(b)-[:BITKA_U_RATU]->(r:Dogadjaj:Rat)")
                .Return((b, g/*, z, r*/) => new
                {
                    Bitka = b.As<Bitka>(),
                    //Zemlja = z.As<Zemlja>(),
                    //RatID = r.As<Rat>().ID,
                    Godina = g.As<Godina>()                    
                })
                .ResultsAsync)
                .FirstOrDefault();

            if (bitkaResult == null)
                return BadRequest($"Nije pronađena bitka sa ID: {id}");

            var bitka = new Bitka
            {
                ID = bitkaResult.Bitka.ID,
                Ime = bitkaResult.Bitka.Ime,
                Tip = bitkaResult.Bitka.Tip,
                Tekst = bitkaResult.Bitka.Tekst,
                Pobednik = bitkaResult.Bitka.Pobednik,
                BrojZrtava = bitkaResult.Bitka.BrojZrtava,
                Rat = bitkaResult.Bitka.Rat,
                Godina = bitkaResult.Godina,
                Lokacija = bitkaResult.Bitka.Lokacija
            };

            return Ok(bitka);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Došlo je do greške pri radu sa Neo4j bazom: {ex.Message}");
        }
    }

    //OVU NISAM GLEDALA
    [HttpGet("GetRatForBitka/{bitkaId}")]
    public async Task<IActionResult> GetRatForBitka(Guid bitkaId)
    {
        try
        {
            var result = (await _client.Cypher
                .Match("(b:Dogadjaj:Bitka)-[:BITKA_U_RATU]->(r:Dogadjaj:Rat)")
                .Where((Bitka b) => b.ID == bitkaId)
                .OptionalMatch("(r)-[:DESIO_SE]->(g:Godina)")
                .OptionalMatch("(r)-[:RAT_TRAJAO_DO]->(gdo:Godina)")
                .OptionalMatch("(bitka:Dogadjaj:Bitka)-[:BITKA_U_RATU]->(r)")
                .Return((r, g, gdo, bitka) => new
                {
                    Rat = r.As<Rat>(),
                    GodinaOd = g.As<Godina>(),
                    GodinaDo = gdo.As<Godina>(),
                    Bitke = bitka.CollectAs<Bitka>()
                })
                .ResultsAsync)
                .FirstOrDefault();

            if (result == null)
                return NotFound($"Rat za bitku sa ID {bitkaId} nije pronađen!");

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

    [HttpPut("UpdateBitka/{id}")]
    public async Task<IActionResult> UpdateBitka(Guid id, [FromBody] Bitka updatedBitka)
    {
        try
        {
            var bitka = (await _client.Cypher
                                        .Match("(b:Dogadjaj:Bitka)")
                                        .Where((Bitka b) => b.ID == id)
                                        .OptionalMatch("(b)-[:DESIO_SE]->(g1:Godina)")                  
                                        //.OptionalMatch("(b)-[:BITKA_U_RATU]->(r:Rat)")             
                                        .Return((b, g1/*, r*/) => new
                                        {
                                            Bitka = b.As<Bitka>(),
                                            //Rat = r.As<Rat>(),
                                            Godina = g1.As<Godina>()                                            
                                        })
                                        .ResultsAsync)
                                        .FirstOrDefault();

            if (bitka == null)
                return NotFound($"Bitka sa ID: {id} nije pronađena!");

            var cypher = _client.Cypher
                .Match("(b:Dogadjaj:Bitka)")
                .Where((Bitka b) => b.ID == id)
                .Set("b.Ime = $ime, b.Tekst = $tekst, b.Tip = 'Bitka', b.Pobednik = $pobednik, b.BrojZrtava = $brojZrtava, b.Lokacija = $lokacija, b.Rat = $rat")
                .WithParams(new
                {
                    ime = updatedBitka.Ime,
                    tekst = updatedBitka.Tekst,
                    pobednik = updatedBitka.Pobednik,
                    lokacija = bitka.Bitka.Lokacija, //NAMERNO STARA DA BI OSTALA ONA AKO UNETA NE POSTOJI
                    rat = bitka.Bitka.Rat,//ISTO KAO ZA LOKACIJU 
                    brojZrtava = updatedBitka.BrojZrtava
                });

            if (!string.IsNullOrEmpty(updatedBitka.Lokacija) && updatedBitka.Lokacija != "string")//uneto je nesto
            {
                var zemljaPostoji = (await _client.Cypher
                            .Match("(z:Zemlja)")
                            .Where("toLower(z.Naziv) = toLower($naziv)")
                            .WithParam("naziv", updatedBitka.Lokacija)
                            .Return(z => z.As<Zemlja>())
                            .ResultsAsync)
                            .Any();

                if (zemljaPostoji)//ta lokacija postoji u bazi 
                {
                    if (!string.IsNullOrEmpty(bitka.Bitka.Lokacija) && bitka.Bitka.Lokacija != "string")//postoji vec neka lokacija u bazi
                    {
                        if (bitka.Bitka.Lokacija.ToLower() != updatedBitka.Lokacija.ToLower())//promenjena lokacija
                        {
                            cypher = cypher
                                .With("b")
                                .OptionalMatch("(b)-[rel:DESIO_SE_U]->()")
                                .Match("(z:Zemlja)")
                                .Where("toLower(z.Naziv) = toLower($nazivZemlje)")
                                .Delete("rel")
                                .WithParam("nazivZemlje", updatedBitka.Lokacija)
                                .Create("(b)-[:DESIO_SE_U]->(z)")
                                .Set("b.Lokacija = $nazivZemlje");
                        }
                        //else ista je lokacija
                    }
                    else//nista nije bilo samo dodajemo 
                    {
                        cypher = cypher
                                .With("b")
                                .Match("(z:Zemlja)")
                                .Where("toLower(z.Naziv) = toLower($nazivZemlje)")
                                .WithParam("nazivZemlje", updatedBitka.Lokacija)
                                .Create("(b)-[:DESIO_SE_U]->(z)")
                                .Set("b.Lokacija = $nazivZemlje");
                    }
                }
                //else//lokacija nije nadjena u bazi ostavlja staru lokaciju jer sam na nju postavila na pocetku 
            }
            else //nije uneto nista 
            {
                cypher = cypher
                    .With("b")
                    .OptionalMatch("(b)-[rel:DESIO_SE_U]->()")
                    .Delete("rel")
                    .Set("b.Lokacija = 'string'");
            }

            if (!string.IsNullOrEmpty(updatedBitka.Rat) && updatedBitka.Rat != "string")//nesto je uneto 
            {//da li to ima smisla
                var ratPostoji = (await _client.Cypher.Match("(r:Dogadjaj:Rat)")
                                                      .Where("toLower(r.Ime) = toLower($imeRata)")
                                                      .WithParam("imeRata", updatedBitka.Rat)
                                                      .Return(r => r.As<Rat>())
                                                      .ResultsAsync)
                                                      .FirstOrDefault();
                if (ratPostoji != null)//smislen unos
                {
                    if (!string.IsNullOrEmpty(bitka.Bitka.Rat) && bitka.Bitka.Rat != "string")//nesto je postojalo u bazi 
                    {//da li je isto kao novo 
                        if (bitka.Bitka.Rat.ToLower() != updatedBitka.Rat.ToLower())
                        {//promenjeno je 
                            cypher = cypher
                                .With("b")
                                .OptionalMatch("(b)-[rel:BITKA_U_RATU]->()")
                                .Match("(r:Dogadjaj:Rat)")
                                .Where("toLower(r.Ime) = toLower($imeRata)")
                                .Delete("rel")
                                .WithParam("imeRata", updatedBitka.Rat)
                                .Create("(b)-[:BITKA_U_RATU]->(r)")
                                .Set("b.Rat = $imeRata");
                        }
                        //else isto je 
                    }
                    else //samo unesi update
                    {
                        cypher = cypher
                                .With("b")
                                .Match("(r:Dogadjaj:Rat)")
                                .Where("toLower(r.Ime) = toLower($imeRata)")
                                .WithParam("imeRata", updatedBitka.Rat)
                                .Create("(b)-[:BITKA_U_RATU]->(r)")
                                .Set("b.Rat = $imeRata");
                    }
                }
                //else nije smislen unos treba da ostane ono sto je bilo                 
            }
            else //nista nije uneto 
            {
                cypher = cypher
                    .With("b")
                    .OptionalMatch("(b)-[rel:BITKA_U_RATU]->()")
                    .Delete("rel")
                    .Set("b.Rat = 'string'");
            }

            if (updatedBitka.Godina != null)//uneta godina 
            {
                if (bitka.Godina != null)//vec je bila neka godina 
                {
                    if (bitka.Godina.God != updatedBitka.Godina.God || bitka.Godina.IsPNE != updatedBitka.Godina.IsPNE)
                    {//promenjena je 
                        var godina = await _godinaService.DodajGodinu(updatedBitka.Godina.God, updatedBitka.Godina.IsPNE);
                        cypher = cypher
                            .With("b")
                            .OptionalMatch("(b)-[rel:DESIO_SE]->()")
                            .Match("(g:Godina {ID: $godinaId})")
                            .Delete("rel")
                            .WithParam("godinaId", godina.ID)
                            .Create("(b)-[:DESIO_SE]->(g)");
                    }
                    //else nije promenjena nista ne diramo 
                }
                else//nije bila godina samo unosimo novu 
                {
                    var godina = await _godinaService.DodajGodinu(updatedBitka.Godina.God, updatedBitka.Godina.IsPNE);
                    cypher = cypher
                        .With("b")
                        .Match("(g:Godina {ID: $godinaId})")
                        .WithParam("godinaId", godina.ID)
                        .Create("(b)-[:DESIO_SE]->(g)");
                }                
            }
            else
            {
                cypher = cypher
                    .With("b")
                    .OptionalMatch("(b)-[rel:DESIO_SE]->()")
                    .Delete("rel");
            }

            await cypher.ExecuteWithoutResultsAsync();

            return Ok($"Bitka '{updatedBitka.Ime}' je uspešno ažurirana!");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Došlo je do greške pri radu sa bazom: {ex.Message}");
        }
    }
}
