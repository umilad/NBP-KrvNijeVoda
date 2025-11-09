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
public class RatController : ControllerBase
{
    private readonly IGraphClient _client;
    private readonly GodinaService _godinaService;
    // private readonly LokacijaService _lokacijaService;
    // private readonly ZemljaService _zemljaService;
    private readonly IMongoCollection<DogadjajMongo> _dogadjajiCollection;

    public RatController(Neo4jService neo4jService, GodinaService godinaService, MongoService mongoService/*, LokacijaService lokacijaService, ZemljaService zemljaService*/)
    {
        _client = neo4jService.GetClient();
        _godinaService = godinaService;
        // _lokacijaService = lokacijaService;
        // _zemljaService = zemljaService;
        _dogadjajiCollection = mongoService.GetCollection<DogadjajMongo>("Dogadjaji");
    }

    [HttpPost("CreateRat")]
    public async Task<IActionResult> CreateRat([FromBody] RatDto rat)
    {
        try
        {
            var r = (await _client.Cypher.Match("(r:Dogadjaj:Rat)")
                                         .Where((RatNeo r) => r.Ime == rat.Ime)
                                         .Return(r => r.As<RatNeo>())
                                         .ResultsAsync)
                                         .FirstOrDefault();

            if (r != null)
            {
                return BadRequest($"Rat sa imenom {rat.Ime} vec postoji u bazi!");
            }

            GodinaNeo godPocetak = null;
            GodinaNeo godKraj = null;
            Guid ratID = Guid.NewGuid();

            var query = _client.Cypher
                .Create($"(r:Dogadjaj:Rat {{ID: $id, Ime: $ime, Tip: 'Rat', Lokacija: $lokacija, Pobednik: $pobednik}})")
                .WithParam("id", ratID)
                .WithParam("ime", rat.Ime)
                .WithParam("pobednik", rat.Pobednik)
                .WithParam("lokacija", rat.Lokacija);//valjda ostaje prazno ako se ne unese nista 

                if (!string.IsNullOrEmpty(rat.Lokacija) && rat.Lokacija != "string")
                {
                    var zemljaPostoji = (await _client.Cypher
                        .Match("(z:Zemlja)")
                        .Where("toLower(z.Naziv) = toLower($naziv)")
                        .WithParam("naziv", rat.Lokacija)
                        .Return(z => z.As<ZemljaNeo>())
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

            // var kreiranRat = (await _client.Cypher.Match("(r:Dogadjaj:Rat {ID: $ratID})")
            //                                       .WithParam("ratID", ratID)
            //                                       .Return(r => r.As<Rat>())
            //                                       .ResultsAsync)
            //                                       .FirstOrDefault();

            //Poveži postojeće bitke
            //POPRAVLJENO ALI BEZ DODAVANJA U LISTU I TREBA DA BUDE DOGADJAJ GENERALNO NE BITKA
            if (rat.Bitke != null && rat.Bitke.Any())
            {
                foreach (var bitka in rat.Bitke)
                {
                    var bitkaPostoji = (await _client.Cypher
                                .Match("(b:Bitka)")
                                .Where("toLower(b.Ime) = toLower($imeBitke)")
                                .WithParam("imeBitke", bitka)
                                .Return(b => b.As<BitkaNeo>())
                                .ResultsAsync).Any();

                    if (bitkaPostoji)
                    {
                        await _client.Cypher
                                .Match("(r:Rat {ID: $ratID})")
                                .Match("(b:Bitka)")
                                .Where("toLower(b.Ime) = toLower($imeBitke)")
                                .Create("(b)-[:BITKA_U_RATU]->(r)")
                                .WithParam("ratID", ratID)
                                .WithParam("imeBitke", bitka)
                                .ExecuteWithoutResultsAsync();

                        //kreiranRat.Bitke.Add(bitka);
                    }

                }
            }
            if (!string.IsNullOrWhiteSpace(rat.Tekst))
            {
                var dogadjajMongo = new DogadjajMongo
                {
                    ID = ratID,
                    Tekst = rat.Tekst
                };
                await _dogadjajiCollection.InsertOneAsync(dogadjajMongo);
            }
                
            return Ok($"Rat '{rat.Ime}' je uspešno dodat u bazu!");

            
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
                .Where((RatNeo r) => r.ID == id)
                .OptionalMatch("(r)-[:DESIO_SE]->(g:Godina)")
                .OptionalMatch("(r)-[:RAT_TRAJAO_DO]->(gdo:Godina)")
                .OptionalMatch("(b:Dogadjaj:Bitka)-[:BITKA_U_RATU]->(r)")
                .With("r, g, gdo, collect(b.Ime) as imenaBitki") // <- pravi alias imena
                .Return((r, g, gdo, imenaBitki) => new
                {
                    rat = r.As<RatNeo>(),
                    godinaOd = g.As<GodinaNeo>(),
                    godinaDo = gdo.As<GodinaNeo>(),
                    imenaBitki = imenaBitki.As<List<string>>() // <- koristi alias
                })
                .ResultsAsync).FirstOrDefault();

            if (result == null)
                return NotFound($"Rat sa ID: {id} nije pronađen!");

            var rat = result.rat;
            rat.Godina = result.godinaOd;
            rat.GodinaDo = result.godinaDo;
            rat.Bitke = result.imenaBitki ?? new List<string>();

            var mongoDoc = await _dogadjajiCollection.Find(d => d.ID == id).FirstOrDefaultAsync();

            // 🔹 Kreiranje DTO objekta
            var dto = new RatDto
            {
                ID = rat.ID,
                Ime = rat.Ime,
                Tip = rat.Tip,
                Lokacija = rat.Lokacija,
                Godina = rat.Godina,
                GodinaDo = rat.GodinaDo,
                Pobednik = rat.Pobednik,
                Bitke = rat.Bitke,
                Tekst = mongoDoc?.Tekst
            };

            return Ok(dto);

        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Greška prilikom rada sa Neo4j bazom: {ex.Message}");
        }
    }

    [HttpGet("GetAllRatovi")]
    public async Task<IActionResult> GetAllRatovi()
    {
        try
        {
            // 1. Dohvatanje svih ratova iz Neo4j
            var ratovi = (await _client.Cypher
                .Match("(r:Dogadjaj:Rat)")
                .OptionalMatch("(r)-[:DESIO_SE]->(g:Godina)")
                .OptionalMatch("(r)-[:RAT_TRAJAO_DO]->(gdo:Godina)")
                .OptionalMatch("(b:Dogadjaj:Bitka)-[:BITKA_U_RATU]->(r)")
                .With("r, g, gdo, collect(b.Ime) as imenaBitki")
                .Return((r, g, gdo, imenaBitki) => new
                {
                    Rat = r.As<RatNeo>(),
                    GodinaOd = g.As<GodinaNeo>(),
                    GodinaDo = gdo.As<GodinaNeo>(),
                    ImenaBitki = imenaBitki.As<List<string>>()
                })
                .ResultsAsync)
                .ToList();

            if (!ratovi.Any())
                return NotFound("Nije pronađen nijedan rat u bazi!");

            // 2. Dohvatanje teksta iz MongoDB
            var ids = ratovi.Select(r => r.Rat.ID).ToList();
            var mongoList = await _dogadjajiCollection.Find(d => ids.Contains(d.ID)).ToListAsync();

            // 3. Kombinovanje u DTO
            var result = ratovi.Select(r =>
            {
                var mongo = mongoList.FirstOrDefault(m => m.ID == r.Rat.ID);
                r.Rat.Godina = r.GodinaOd;
                r.Rat.GodinaDo = r.GodinaDo;
                r.Rat.Bitke = r.ImenaBitki ?? new List<string>();

                return new RatDto
                {
                    ID = r.Rat.ID,
                    Ime = r.Rat.Ime,
                    Tip = r.Rat.Tip,
                    Lokacija = r.Rat.Lokacija,
                    Godina = r.Rat.Godina,
                    GodinaDo = r.Rat.GodinaDo,
                    Pobednik = r.Rat.Pobednik,
                    Bitke = r.Rat.Bitke,
                    Tekst = mongo?.Tekst
                };
            }).ToList();

            return Ok(result);
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
                .Where((RatNeo r) => r.ID == ratID)
                .Return((b, g) => new
                {
                    Bitka = b.As<BitkaNeo>(),
                    Godina = g.As<GodinaNeo>()
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
                .Where((RatNeo r) => r.ID == id)
                .Return(r => r.As<RatNeo>())
                .ResultsAsync).FirstOrDefault();

            if (rat == null)
                return NotFound($"Rat sa ID: {id} nije pronađen!");

            await _client.Cypher
                .Match("(r:Dogadjaj:Rat)")
                .Where((RatNeo r) => r.ID == id)
                .DetachDelete("r")
                .ExecuteWithoutResultsAsync();

            await _dogadjajiCollection.DeleteOneAsync(d => d.ID == id);
            return Ok($"Rat sa ID: {id} je uspešno obrisan!");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Došlo je do greške pri radu sa Neo4j bazom: {ex.Message}");
        }
    }

   [HttpPut("UpdateRat/{id}")]
public async Task<IActionResult> UpdateRat(Guid id, [FromBody] RatDto updatedRat)
{
    try
    {
        var rat = (await _client.Cypher
                                    .Match("(r:Dogadjaj:Rat)")
                                    .Where((RatNeo r) => r.ID == id)
                                    .OptionalMatch("(r)-[:DESIO_SE]->(g1:Godina)")          
                                    .OptionalMatch("(r)-[:RAT_TRAJAO_DO]->(g2:Godina)")     
                                    .Return((r, g1, g2) => new
                                    {
                                        Rat = r.As<RatNeo>(),
                                        PocetnaGodina = g1.As<GodinaNeo>(),
                                        KrajnjaGodina = g2.As<GodinaNeo>()
                                    })
                                    .ResultsAsync)
                                    .FirstOrDefault();

        if (rat == null)
            return NotFound($"Rat sa ID: {id} nije pronađen!");

        // Provera duplikata imena
        var duplikat = (await _client.Cypher
            .Match("(r:Dogadjaj:Rat)")
            .Where("toLower(r.Ime) = toLower($naziv) AND r.ID <> $id")
            .WithParam("naziv", updatedRat.Ime)
            .WithParam("id", id)
            .Return(r => r.As<RatNeo>())
            .ResultsAsync)
            .Any();

        if (duplikat)
            return BadRequest($"Rat sa nazivom '{updatedRat.Ime}' već postoji u bazi!");

        // Osnovni update u Neo4j (osim Tekst koji ide u Mongo)
        var cypher = _client.Cypher
                            .Match("(r:Dogadjaj:Rat)")
                            .Where((RatNeo r) => r.ID == id)
                            .Set("r.Ime = $ime, r.Tip = 'Rat', r.Lokacija = $lokacija, r.Pobednik = $pobednik")
                            .With("r")
                            .WithParams(new
                            {
                                ime = updatedRat.Ime,
                                lokacija = rat.Rat.Lokacija,
                                pobednik = updatedRat.Pobednik
                            });

        // Lokacija
        if (!string.IsNullOrEmpty(updatedRat.Lokacija) && updatedRat.Lokacija != "string")
        {
            var zemljaPostoji = (await _client.Cypher
                        .Match("(z:Zemlja)")
                        .Where("toLower(z.Naziv) = toLower($naziv)")
                        .WithParam("naziv", updatedRat.Lokacija)
                        .Return(z => z.As<ZemljaNeo>())
                        .ResultsAsync)
                        .Any();

            if (zemljaPostoji)
            {
                if (!string.IsNullOrEmpty(rat.Rat.Lokacija) && rat.Rat.Lokacija != "string")
                {
                    if (rat.Rat.Lokacija.ToLower() != updatedRat.Lokacija.ToLower())
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
                }
                else
                {
                    cypher = cypher.With("r")
                            .Match("(z:Zemlja)")
                            .Where("toLower(z.Naziv) = toLower($nazivZemlje)")
                            .WithParam("nazivZemlje", updatedRat.Lokacija)
                            .Create("(r)-[:DESIO_SE_U]->(z)")
                            .Set("r.Lokacija = $nazivZemlje");
                }
            }
        }
        else
        {
            cypher = cypher
                .With("r")
                .OptionalMatch("(r)-[rel:DESIO_SE_U]->()")
                .Delete("rel")
                .Set("r.Lokacija = 'string'");
        }

        // Početna godina
        if (updatedRat.Godina != null && updatedRat.Godina.God != 0)
        {
            var godinaOd = await _godinaService.DodajGodinu(updatedRat.Godina.God, updatedRat.Godina.IsPNE);

            cypher = cypher
                .With("r")
                .OptionalMatch("(r)-[rel:DESIO_SE]->()")
                .Delete("rel")
                .With("r")
                .Match("(g:Godina {ID: $godinaIdOd})")
                .Create("(r)-[:DESIO_SE]->(g)")
                .WithParam("godinaIdOd", godinaOd.ID);
        }
        else
        {
            cypher = cypher
                .With("r")
                .OptionalMatch("(r)-[rel:DESIO_SE]->()")
                .Delete("rel");
        }

        // Krajnja godina
        if (updatedRat.GodinaDo != null && updatedRat.GodinaDo.God != 0)
        {
            var godinaDo = await _godinaService.DodajGodinu(updatedRat.GodinaDo.God, updatedRat.GodinaDo.IsPNE);

            cypher = cypher
                .With("r")
                .OptionalMatch("(r)-[rel:RAT_TRAJAO_DO]->()")
                .Delete("rel")
                .With("r")
                .Match("(gdo:Godina {ID: $godinaIdDo})")
                .Create("(r)-[:RAT_TRAJAO_DO]->(gdo)")
                .WithParam("godinaIdDo", godinaDo.ID);
        }
        else
        {
            cypher = cypher
                .With("r")
                .OptionalMatch("(r)-[rel:RAT_TRAJAO_DO]->()")
                .Delete("rel");
        }

        // Izvrši update u Neo4j
        await cypher.ExecuteWithoutResultsAsync();

        // Ažuriranje teksta u MongoDB
        if (!string.IsNullOrWhiteSpace(updatedRat.Tekst))
        {
            var filter = Builders<DogadjajMongo>.Filter.Eq(d => d.ID, id);
            var update = Builders<DogadjajMongo>.Update.Set(d => d.Tekst, updatedRat.Tekst);
            await _dogadjajiCollection.UpdateOneAsync(filter, update);
        }

        return Ok($"Rat '{updatedRat.Ime}' uspešno ažuriran!");
    }
    catch (Exception ex)
    {
        return StatusCode(500, $"Došlo je do greške pri radu sa bazom: {ex.Message}");
    }
}



}