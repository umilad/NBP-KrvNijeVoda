using Microsoft.AspNetCore.Mvc;
using Neo4jClient;
//using KrvNijeVoda.Back.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
[Route("api")]
[ApiController]
public class BitkaController : ControllerBase
{
    private readonly IGraphClient _client;
    private readonly GodinaService _godinaService;
    // private readonly LokacijaService _lokacijaService;
    // private readonly ZemljaService _zemljaService;
    // private readonly RatService _ratService;
     private readonly IMongoCollection<DogadjajMongo> _dogadjajiCollection;
    public BitkaController(Neo4jService neo4jService, GodinaService godinaService, MongoService mongoService /*, LokacijaService lokacijaService, RatService ratService, ZemljaService zemljaService*/)
    {
        _client = neo4jService.GetClient();
        _godinaService = godinaService;
        _dogadjajiCollection = mongoService.GetCollection<DogadjajMongo>("Dogadjaji");
        // _lokacijaService = lokacijaService;
        // _ratService = ratService;
        // _zemljaService=zemljaService;
    }

    [HttpPost("CreateBitka")]
    public async Task<IActionResult> CreateBitka([FromBody] BitkaDto bitka)
    {
        try
        {
            //ovo ide na front
            // if (string.IsNullOrEmpty(bitka.Pobednik))
            //     return BadRequest("Polje 'Pobednik' je obavezno.");

            var bit = (await _client.Cypher.Match("(b:Dogadjaj:Bitka)")
                                           .Where((BitkaNeo b) => b.Ime == bitka.Ime)
                                           .Return(b => b.As<BitkaNeo>())
                                           .ResultsAsync)
                                           .FirstOrDefault();

            if (bit != null)
            {
                return BadRequest($"Bitka sa imenom {bitka.Ime} vec postoji u bazi!");
            }
            var bitkaID = Guid.NewGuid();
            var query = _client.Cypher
                .Create("(b:Dogadjaj:Bitka {ID: $id, Ime: $ime, Tip: 'Bitka', Pobednik: $pobednik, BrojZrtava: $brojZrtava})")
                .WithParam("id", bitkaID)
                .WithParam("ime", bitka.Ime)
                .WithParam("pobednik", bitka.Pobednik)
                .WithParam("brojZrtava", bitka.BrojZrtava);
                //MOZE DA SE DODA DA SETUJE I LOKACIJU OVDE ODMAH NA "string" 
                // DA AKO SE NE SETUJE NA NES NORMALNO OSTANE STRING OVAKO SE NE SECAM DA L OSTAJE NULL ILI STA 

            if (!string.IsNullOrEmpty(bitka.Lokacija) && bitka.Lokacija != "string")
            {
                var zemljaPostoji = (await _client.Cypher
                    .Match("(z:Zemlja)")
                    .Where("toLower(z.Naziv) = toLower($naziv)")
                    .WithParam("naziv", bitka.Lokacija)
                    .Return(z => z.As<ZemljaNeo>())
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
                    .Return(r => r.As<RatNeo>())
                    .ResultsAsync)
                    .Any();

                if (ratPostoji)
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

            if (!string.IsNullOrWhiteSpace(bitka.Tekst))
            {
                var dogadjajMongo = new DogadjajMongo
                {
                    ID =  bitkaID,
                    Tekst = bitka.Tekst
                };
                await _dogadjajiCollection.InsertOneAsync(dogadjajMongo);
            }

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
                .Where((BitkaNeo b) => b.ID == id)
                .Return(b => b.As<BitkaNeo>())
                .ResultsAsync).FirstOrDefault();

            if (bitka == null)
                return NotFound($"Bitka sa ID {id} nije pronađena!");

            await _client.Cypher
                .Match("(b:Dogadjaj:Bitka)")
                .Where((BitkaNeo b) => b.ID == id)
                .DetachDelete("b")
                .ExecuteWithoutResultsAsync();
            await _dogadjajiCollection.DeleteOneAsync(d => d.ID == id);
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
                .Where((BitkaNeo b) => b.ID == id)
                .OptionalMatch("(b)-[:DESIO_SE]->(g:Godina)")
                .OptionalMatch("(b)-[:DESIO_SE_U]->(z:Zemlja)")
                .OptionalMatch("(b)-[:BITKA_U_RATU]->(r:Dogadjaj:Rat)")
                .Return((b, g , z, r) => new
                {
                    Bitka = b.As<BitkaNeo>(),
                    Zemlja = z.As<ZemljaNeo>(),
                    Rat = r.As<DogadjajNeo>(),
                    Godina = g.As<GodinaNeo>()
                })
                .ResultsAsync)
                .FirstOrDefault();

            if (bitkaResult == null)
                return BadRequest($"Nije pronađena bitka sa ID: {id}");

            var mongoDoc = await _dogadjajiCollection.Find(d => d.ID == id).FirstOrDefaultAsync();

            var dto = new BitkaDto
            {
                ID = bitkaResult.Bitka.ID,
                Ime = bitkaResult.Bitka.Ime,
                //Tip = bitkaResult.Bitka.Tip,
                Pobednik = bitkaResult.Bitka.Pobednik,
                BrojZrtava = bitkaResult.Bitka.BrojZrtava,
                Rat = bitkaResult.Rat.Ime,
                Godina = bitkaResult.Godina,
                Lokacija = bitkaResult.Zemlja.Naziv,
                Tekst = mongoDoc?.Tekst
            };


            return Ok(dto);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Došlo je do greške pri radu sa Neo4j bazom: {ex.Message}");
        }
    }

[HttpGet("GetAllBitke")]
public async Task<IActionResult> GetAllBitke()
{
    try
    {
        // 1. Dohvatanje svih bitki iz Neo4j
        var neoResults = (await _client.Cypher
            .Match("(b:Dogadjaj:Bitka)")
            .OptionalMatch("(b)-[:DESIO_SE]->(g:Godina)")
            .Return((b, g) => new
            {
                BitkaData = b.As<BitkaNeo>(),
                GodinaData = g.As<GodinaNeo>()  // bez ternarnog operatora
            })
            .ResultsAsync)
            .ToList();

        if (!neoResults.Any())
            return NotFound("Nije pronađena nijedna bitka u bazi!");

        // 2. Dohvatanje teksta iz MongoDB
        var ids = neoResults.Select(r => r.BitkaData.ID).ToList();
        var mongoList = await _dogadjajiCollection.Find(d => ids.Contains(d.ID)).ToListAsync();

        // 3. Kombinovanje u DTO
        var result = neoResults.Select(r =>
        {
            var mongo = mongoList.FirstOrDefault(m => m.ID == r.BitkaData.ID);
            return new BitkaDto
            {
                ID = r.BitkaData.ID,
                Ime = r.BitkaData.Ime,
                Pobednik = r.BitkaData.Pobednik,
                BrojZrtava = r.BitkaData.BrojZrtava,
                Lokacija = r.BitkaData.Lokacija,
                Godina = r.GodinaData,  // može biti null
                Tekst = mongo?.Tekst
            };
        }).ToList();

        return Ok(result);
    }
    catch (Exception ex)
    {
        return StatusCode(500, $"Došlo je do greške pri radu sa Neo4j bazom: {ex.Message}");
    }
}

    // OVU NISAM GLEDALA
    // [HttpGet("GetRatForBitka/{bitkaId}")]
    // public async Task<IActionResult> GetRatForBitka(Guid bitkaId)
    // {
    //     try
    //     {
    //         var result = (await _client.Cypher
    //             .Match("(b:Dogadjaj:Bitka)-[:BITKA_U_RATU]->(r:Dogadjaj:Rat)")
    //             .Where((Bitka b) => b.ID == bitkaId)
    //             .OptionalMatch("(r)-[:DESIO_SE]->(g:Godina)")
    //             .OptionalMatch("(r)-[:RAT_TRAJAO_DO]->(gdo:Godina)")
    //             .OptionalMatch("(bitka:Dogadjaj:Bitka)-[:BITKA_U_RATU]->(r)")
    //             .Return((r, g, gdo, bitka) => new
    //             {
    //                 Rat = r.As<Rat>(),
    //                 GodinaOd = g.As<Godina>(),
    //                 GodinaDo = gdo.As<Godina>(),
    //                 Bitke = bitka.CollectAs<Bitka>()
    //             })
    //             .ResultsAsync)
    //             .FirstOrDefault();

    //         if (result == null)
    //             return NotFound($"Rat za bitku sa ID {bitkaId} nije pronađen!");

    //         var rat = result.Rat;
    //         rat.Godina = result.GodinaOd;
    //         rat.GodinaDo = result.GodinaDo;
    //         rat.Bitke = result.Bitke.ToList();

    //         return Ok(rat);
    //     }
    //     catch (Exception ex)
    //     {
    //         return StatusCode(500, $"Greška prilikom rada sa Neo4j bazom: {ex.Message}");
    //     }
    // }

    [HttpPut("UpdateBitka/{id}")]
public async Task<IActionResult> UpdateBitka(Guid id, [FromBody] BitkaDto updatedBitka)
{
    try
    {
        // 1️⃣ Učitaj postojeću bitku
        var bitkaResult = (await _client.Cypher
            .Match("(b:Dogadjaj:Bitka)")
            .Where((BitkaNeo b) => b.ID == id)
            .OptionalMatch("(b)-[:DESIO_SE]->(g:Godina)")
            .Return((b, g) => new
            {
                Bitka = b.As<BitkaNeo>(),
                Godina = g.As<GodinaNeo>()
            })
            .ResultsAsync)
            .FirstOrDefault();

        if (bitkaResult == null)
            return NotFound($"Bitka sa ID: {id} nije pronađena!");

        var bitka = bitkaResult.Bitka;

        // 2️⃣ Provera duplikata imena
        var duplikat = (await _client.Cypher
            .Match("(b:Dogadjaj:Bitka)")
            .Where("toLower(b.Ime) = toLower($ime) AND b.ID <> $id")
            .WithParams(new { ime = updatedBitka.Ime, id })
            .Return(b => b.As<BitkaNeo>())
            .ResultsAsync)
            .Any();

        if (duplikat)
            return BadRequest($"Bitka sa nazivom '{updatedBitka.Ime}' već postoji!");

        // 3️⃣ Ažuriraj osnovna polja u Bitka nodu
        await _client.Cypher
            .Match("(b:Dogadjaj:Bitka)")
            .Where((BitkaNeo b) => b.ID == id)
            .Set("b.Ime = $ime, b.Tip = 'Bitka', b.Pobednik = $pobednik, b.BrojZrtava = $brojZrtava")
            .WithParams(new
            {
                ime = updatedBitka.Ime,
                pobednik = updatedBitka.Pobednik,
                brojZrtava = updatedBitka.BrojZrtava
            })
            .ExecuteWithoutResultsAsync();

        // 4️⃣ Ažuriranje Lokacije (Zemlja)
        // Obriši postojeće veze
        await _client.Cypher
            .Match("(b:Dogadjaj:Bitka)-[rel:DESIO_SE_U]->()")
            .Where((BitkaNeo b) => b.ID == id)
            .Delete("rel")
            .ExecuteWithoutResultsAsync();

        if (!string.IsNullOrEmpty(updatedBitka.Lokacija) && updatedBitka.Lokacija != "string")
        {
            await _client.Cypher
                .Match("(b:Dogadjaj:Bitka)", "(z:Zemlja)")
                .Where((BitkaNeo b) => b.ID == id)
                .AndWhere("toLower(z.Naziv) = toLower($imeZemlje)")
                .WithParam("imeZemlje", updatedBitka.Lokacija)
                .Create("(b)-[:DESIO_SE_U]->(z)")
                .Set("b.Lokacija = $imeZemlje")
                .ExecuteWithoutResultsAsync();
        }
        else
        {
            // Ako ništa nije uneto, postavi "string" ili ostavi staru
            await _client.Cypher
                .Match("(b:Dogadjaj:Bitka)")
                .Where((BitkaNeo b) => b.ID == id)
                .Set("b.Lokacija = 'string'")
                .ExecuteWithoutResultsAsync();
        }

        // 5️⃣ Ažuriranje veze ka Ratu
        // Obriši sve postojeće veze
        await _client.Cypher
            .Match("(b:Dogadjaj:Bitka)-[rel:BITKA_U_RATU]->()")
            .Where((BitkaNeo b) => b.ID == id)
            .Delete("rel")
            .ExecuteWithoutResultsAsync();

        // Kreiraj novu vezu ako je smislen unos
        if (!string.IsNullOrEmpty(updatedBitka.Rat) && updatedBitka.Rat != "string")
        {
            await _client.Cypher
                .Match("(b:Dogadjaj:Bitka)", "(r:Dogadjaj:Rat)")
                .Where((BitkaNeo b) => b.ID == id)
                .AndWhere("toLower(r.Ime) = toLower($imeRata)")
                .WithParam("imeRata", updatedBitka.Rat)
                .Create("(b)-[:BITKA_U_RATU]->(r)")
                .Set("b.Rat = $imeRata")
                .ExecuteWithoutResultsAsync();
        }

        // 6️⃣ Ažuriranje Godine
        if (updatedBitka.Godina != null)
        {
            var godina = await _godinaService.DodajGodinu(updatedBitka.Godina.God, updatedBitka.Godina.IsPNE);

            // Obriši staru vezu
            await _client.Cypher
                .Match("(b:Dogadjaj:Bitka)-[rel:DESIO_SE]->()")
                .Where((BitkaNeo b) => b.ID == id)
                .Delete("rel")
                .ExecuteWithoutResultsAsync();

            // Kreiraj novu
            await _client.Cypher
                .Match("(b:Dogadjaj:Bitka)", "(g:Godina)")
                .Where((BitkaNeo b) => b.ID == id)
                .AndWhere((GodinaNeo g) => g.ID == godina.ID)
                .Create("(b)-[:DESIO_SE]->(g)")
                .ExecuteWithoutResultsAsync();
        }

        // 7️⃣ Ažuriranje Teksta u MongoDB
        if (!string.IsNullOrWhiteSpace(updatedBitka.Tekst))
        {
            var filter = Builders<DogadjajMongo>.Filter.Eq(d => d.ID, id);
            var update = Builders<DogadjajMongo>.Update.Set(d => d.Tekst, updatedBitka.Tekst);
            await _dogadjajiCollection.UpdateOneAsync(filter, update);
        }

        return Ok($"Bitka '{updatedBitka.Ime}' je uspešno ažurirana!");
    }
    catch (Exception ex)
    {
        return StatusCode(500, $"Došlo je do greške pri radu sa bazom: {ex.Message}");
    }
}

    
    // [HttpGet("GetAllBitke")]
    // public async Task<IActionResult> GetAllBitke()
    // {
    //     try
    //     {
    //         var bitke = (await _client.Cypher
    //             .Match("(b:Dogadjaj:Bitka)")
    //             //.Where("NOT (b:Dogadjaj OR b:Dogadjaj:Rat)")
    //             .OptionalMatch("(b)-[:DESIO_SE]->(g:Godina)")
    //             //.OptionalMatch("(b)-[:DESIO_SE_U]->(z:Zemlja)")
    //             //.OptionalMatch("(b)-[:BITKA_U_RATU]->(r:Dogadjaj:Rat)")
    //             .Return((b, g/*, z, r*/) => new
    //             {
    //                 Bitka = b.As<BitkaNeo>(),
    //                 //Zemlja = z.As<Zemlja>(),
    //                 //RatID = r.As<Rat>().ID,
    //                 Godina = g.As<GodinaNeo>()                    
    //             })
    //             .ResultsAsync)
    //             .ToList();

    //         if (!bitke.Any())
    //             return BadRequest($"Nije pronađena nijedna bitka u bazi!");

    //         var ids = bitke.Select(b => b.Bitka.ID).ToList();
    //         var mongoList = await _dogadjajiCollection.Find(m => ids.Contains(m.ID)).ToListAsync();

    //         var result = bitke.Select(b =>
    //         {
    //             var mongo = mongoList.FirstOrDefault(m => m.ID == b.Bitka.ID);
    //             return new BitkaDto
    //             {
    //                 ID = b.Bitka.ID,
    //                 Ime = b.Bitka.Ime,
    //                 //Tip = bitkaResult.Bitka.Tip,
    //                 Pobednik = b.Bitka.Pobednik,
    //                 BrojZrtava = b.Bitka.BrojZrtava,
    //                 Rat = b.Bitka.Rat,
    //                 Godina = b.Godina,
    //                 Lokacija = b.Bitka.Lokacija,
    //                 Tekst = mongo?.Tekst
    //             };
    //         }).ToList();          
                
    //         return Ok(result);
    //     }
    //     catch (Exception ex)
    //     {
    //         return StatusCode(500, $"Došlo je do greške pri radu sa Neo4j bazom: {ex.Message}");
    //     }
    // }
}
