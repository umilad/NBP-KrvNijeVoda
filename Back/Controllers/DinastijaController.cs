using Microsoft.AspNetCore.Mvc;
using Neo4jClient;
using System;
using System.Threading.Tasks;
using System.Reflection.Metadata;
using KrvNijeVoda.Back;
using System.Formats.Tar;
using KrvNijeVoda.Back.Helpers;
using MongoDB.Driver;
[Route("api")]
[ApiController]
public class DinastijaController : ControllerBase
{
    private readonly IGraphClient _client;
    private readonly GodinaService _godinaService;
    private readonly IVladarService _vladarService;
    private readonly ILicnostService _licnostService;
    private readonly ITreeBuilder _treeBuilder;
    private readonly IMongoCollection<DinastijaMongo> _dinastijaCollection;
    // Constructor: Injecting Neo4jService and getting the client
    public DinastijaController(Neo4jService neo4jService, GodinaService godinaService, MongoService mongoService, IVladarService vladarService, ILicnostService licnostService, ITreeBuilder treeBuilder)
    {
        _client = neo4jService.GetClient();  // Get the Neo4jClient
        _godinaService = godinaService;
        _vladarService = vladarService;
        _licnostService = licnostService;
        _treeBuilder = treeBuilder;
        _dinastijaCollection = mongoService.GetCollection<DinastijaMongo>("Dinastije");
    }

    [HttpPost("CreateDinastija")]
public async Task<IActionResult> CreateDinastija(
    [FromForm] DinastijaDto dinastija,
    [FromForm] IFormFile? slika)
{
    try
    {
        // =========================
        // 1. UPLOAD SLIKE (NOVO)
        // =========================
        if (slika != null && slika.Length > 0)
        {
            // relativna putanja od back foldera
            var uploadsPath = Path.Combine("..", "front", "public", "images", "dinastije");

            // apsolutna putanja
            uploadsPath = Path.GetFullPath(uploadsPath);

            if (!Directory.Exists(uploadsPath))
                Directory.CreateDirectory(uploadsPath);

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(slika.FileName)}";
            var fullPath = Path.Combine(uploadsPath, fileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await slika.CopyToAsync(stream);
            }

            // string za React/Mongo
            dinastija.Slika = fileName;
        }

        // =========================
        // 2. PROVERA POSTOJANJA
        // =========================
        var din = (await _client.Cypher.Match("(d:Dinastija)")
                                       .Where("toLower(d.Naziv) = toLower($naziv)")
                                       .WithParam("naziv", dinastija.Naziv)
                                       .Return(d => d.As<DinastijaNeo>())
                                       .ResultsAsync)
                                       .FirstOrDefault();

        if (din != null)
            return BadRequest($"Dinastija sa imenom {dinastija.Naziv} već postoji u bazi!");

        // =========================
        // 3. CREATE DINASTIJA
        // =========================
        var dinID = Guid.NewGuid();
        var query = _client.Cypher.Create("(d:Dinastija {ID: $id, Naziv: $naziv})")
                                  .WithParam("id", dinID)
                                  .WithParam("naziv", dinastija.Naziv);

        // =========================
        // 4. GODINE
        // =========================
        if (dinastija.PocetakVladavineGod != 0)
        {
            await _godinaService.DodajGodinu(dinastija.PocetakVladavineGod, dinastija.PocetakVladavinePNE);
            query = query.With("d")
                         .Match("(pg:Godina {God: $pocetak, IsPNE: $pocetakPNE})")
                         .WithParam("pocetak", dinastija.PocetakVladavineGod)
                         .WithParam("pocetakPNE", dinastija.PocetakVladavinePNE)
                         .Set("d.PocetakVladavineGod = $pocetak, d.PocetakVladavinePNE = $pocetakPNE")
                         .Create("(d)-[:POCETAK_VLADAVINE]->(pg)");
        }

        if (dinastija.KrajVladavineGod != 0)
        {
            await _godinaService.DodajGodinu(dinastija.KrajVladavineGod, dinastija.KrajVladavinePNE);
            query = query.With("d")
                         .Match("(kg:Godina {God: $kraj, IsPNE: $krajPNE})")
                         .WithParam("kraj", dinastija.KrajVladavineGod)
                         .WithParam("krajPNE", dinastija.KrajVladavinePNE)
                         .Set("d.KrajVladavineGod = $kraj, d.KrajVladavinePNE = $krajPNE")
                         .Create("(d)-[:KRAJ_VLADAVINE]->(kg)");
        }

        await query.ExecuteWithoutResultsAsync();

        // =========================
        // 5. MONGO
        // =========================
        if (string.IsNullOrWhiteSpace(dinastija.Slika))
        {
            // Ako nije izabrana slika, dodaj placeholder
            dinastija.Slika = "placeholder_dinastija.png";
        }

        var dinastijaMongo = new DinastijaMongo
        {
            ID = dinID,
            Slika = dinastija.Slika
        };
        await _dinastijaCollection.InsertOneAsync(dinastijaMongo);

        return Ok($"Dinastija {dinastija.Naziv} je uspešno kreirana!");
    }
    catch (Exception ex)
    {
        return StatusCode(500, $"Došlo je do greške pri radu sa Neo4j bazom: {ex.Message}");
    }
}


    //SREDI
    [HttpGet("GetDinastija/{id}")]
    public async Task<IActionResult> GetDinastija(Guid id)
    {
        try
        {
            var din = (await _client.Cypher.Match("(d:Dinastija)")
                                           .Where((DinastijaNeo d) => d.ID == id)
                                           .Return(d => d.As<DinastijaNeo>())
                                           .ResultsAsync)
                                           .FirstOrDefault();
            if (din == null)
                return NotFound($"Dinastija sa ID: {id} ne postoji u bazi!");

            //Da se sredi prikaz 

            // var result = new Dinastija
            // {
            //     ID = din.ID,
            //     Naziv = din.Naziv,
            //     Slika = din.Slika,
            //     PocetakVladavine = $"{din.PocetakVladavineGod }"
            //     // PocetakVladavine = new GodinaStruct(din.PocetakVladavineGod, din.PocetakVladavinePNE),
            //     // KrajVladavine = new GodinaStruct(din.KrajVladavineGod, din.KrajVladavinePNE)
            // };
            var mongoDoc = await _dinastijaCollection.Find(d => d.ID == id).FirstOrDefaultAsync();
            var dto = new DinastijaDto
            {
                ID = din.ID,
                Naziv = din.Naziv,
                PocetakVladavineGod = din.PocetakVladavineGod,
                PocetakVladavinePNE = din.PocetakVladavinePNE,
                KrajVladavineGod = din.KrajVladavineGod,
                KrajVladavinePNE = din.KrajVladavinePNE,
                Slika = mongoDoc?.Slika
            };
            return Ok(dto);//uredjen prikaz kasnije za pne i to 
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Došlo je do greške pri radu sa Neo4j bazom: {ex.Message}");
        }
    }

    [HttpPut("UpdateDinastija/{id}")]
public async Task<IActionResult> UpdateDinastija([FromForm] DinastijaDto dinastija, Guid id, [FromForm] IFormFile? slika)
{
    try
    {
        var din = (await _client.Cypher.Match("(d:Dinastija)")
                                       .Where((DinastijaNeo d) => d.ID == id)
                                       .Return(d => d.As<DinastijaNeo>())
                                       .ResultsAsync)
                                       .FirstOrDefault();

        if (din == null)
            return BadRequest($"Dinastija sa ID: {id} ne postoji u bazi!");

        // Provera duplikata naziva
        var duplikat = (await _client.Cypher
            .Match("(d:Dinastija)")
            .Where("toLower(d.Naziv) = toLower($naziv) AND d.ID <> $id")
            .WithParam("naziv", dinastija.Naziv)
            .WithParam("id", id)
            .Return(d => d.As<DinastijaNeo>())
            .ResultsAsync)
            .Any();

        if (duplikat)
            return BadRequest($"Dinastija sa nazivom '{dinastija.Naziv}' već postoji u bazi!");

        // =========================
        // 1. UPLOAD NOVE SLIKE
        // =========================
        var uploadsPath = Path.Combine("..", "front", "public", "images", "dinastije");
        uploadsPath = Path.GetFullPath(uploadsPath);
        if (!Directory.Exists(uploadsPath))
            Directory.CreateDirectory(uploadsPath);

        if (slika != null && slika.Length > 0)
        {
            // Dohvati staru sliku iz Mongo-a
            var staraMongo = (await _dinastijaCollection.Find(d => d.ID == id).FirstOrDefaultAsync())?.Slika;

            // Obrisi staru sliku ako nije placeholder
            if (!string.IsNullOrWhiteSpace(staraMongo) && staraMongo != "placeholder.png")
            {
                var staraPutanja = Path.Combine(uploadsPath, staraMongo);
                if (System.IO.File.Exists(staraPutanja))
                    System.IO.File.Delete(staraPutanja);
            }

            // Snimi novu sliku
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(slika.FileName)}";
            var fullPath = Path.Combine(uploadsPath, fileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
                await slika.CopyToAsync(stream);

            // Postavi u DTO za Mongo update
            dinastija.Slika = fileName;
        }

        // =========================
        // 2. Update osnovnih atributa u Neo4j
        // =========================
        var query = _client.Cypher.Match("(d:Dinastija)")
                                  .Where((DinastijaNeo d) => d.ID == id)
                                  .Set("d.Naziv = $naziv")
                                  .WithParam("naziv", dinastija.Naziv);

        bool promenjenPocetak = false;
        bool promenjenKraj = false;

        // --- Pocetak vladavine ---
        if (dinastija.PocetakVladavineGod != 0)
        {
            if (din.PocetakVladavineGod != dinastija.PocetakVladavineGod || din.PocetakVladavinePNE != dinastija.PocetakVladavinePNE)
            {
                query = query.With("d")
                             .Match("(d)-[r:POCETAK_VLADAVINE]->(pg:Godina)")
                             .Delete("r");
                promenjenPocetak = true;
            }
        }
        else
        {
            query = query.With("d")
                         .OptionalMatch("(d)-[r:POCETAK_VLADAVINE]->()")
                         .Delete("r");
        }

        // --- Kraj vladavine ---
        if (dinastija.KrajVladavineGod != 0)
        {
            if (din.KrajVladavineGod != dinastija.KrajVladavineGod || din.KrajVladavinePNE != dinastija.KrajVladavinePNE)
            {
                query = query.With("d")
                             .Match("(d)-[r1:KRAJ_VLADAVINE]->(kg:Godina)")
                             .Delete("r1");
                promenjenKraj = true;
            }
        }
        else
        {
            query = query.With("d")
                         .OptionalMatch("(d)-[r1:KRAJ_VLADAVINE]->()")
                         .Delete("r1");
        }

        // Dodavanje novih godina
        if (promenjenPocetak)
        {
            await _godinaService.DodajGodinu(dinastija.PocetakVladavineGod, dinastija.PocetakVladavinePNE);
            query = query.With("d")
                         .Match("(pg:Godina {God: $pocetak, IsPNE: $pocetakPNE})")
                         .WithParam("pocetak", dinastija.PocetakVladavineGod)
                         .WithParam("pocetakPNE", dinastija.PocetakVladavinePNE)
                         .Set("d.PocetakVladavineGod = $pocetak, d.PocetakVladavinePNE = $pocetakPNE")
                         .Create("(d)-[:POCETAK_VLADAVINE]->(pg)");
        }

        if (promenjenKraj)
        {
            await _godinaService.DodajGodinu(dinastija.KrajVladavineGod, dinastija.KrajVladavinePNE);
            query = query.With("d")
                         .Match("(kg:Godina {God: $kraj, IsPNE: $krajPNE})")
                         .WithParam("kraj", dinastija.KrajVladavineGod)
                         .WithParam("krajPNE", dinastija.KrajVladavinePNE)
                         .Set("d.KrajVladavineGod = $kraj, d.KrajVladavinePNE = $krajPNE")
                         .Create("(d)-[:KRAJ_VLADAVINE]->(kg)");
        }

        await query.ExecuteWithoutResultsAsync();

        if (!string.IsNullOrWhiteSpace(dinastija.Slika))
        {
            var filter = Builders<DinastijaMongo>.Filter.Eq(d => d.ID, id);
            var update = Builders<DinastijaMongo>.Update.Set(d => d.Slika, dinastija.Slika);
            await _dinastijaCollection.UpdateOneAsync(filter, update);
        }

        return Ok($"Dinastija '{dinastija.Naziv}' je uspešno ažurirana!");
    }
    catch (Exception ex)
    {
        return StatusCode(500, $"Došlo je do greške pri radu sa Neo4j bazom: {ex.Message}");
    }
}


    [HttpDelete("DeleteDinastija/{id}")]
    public async Task<IActionResult> DeleteDinastija(Guid id)
    {
        try
        {
            var din = (await _client.Cypher.Match("(d:Dinastija)")
                                           .Where((DinastijaNeo d) => d.ID == id)
                                           .Return(d => d.As<DinastijaNeo>())
                                           .ResultsAsync)
                                           .FirstOrDefault();
            if (din == null)
                return BadRequest($"Dinastija sa ID: {id} ne postoji u bazi!");

            await _client.Cypher.Match("(d:Dinastija)")
                                .Where((DinastijaNeo d) => d.ID == id)
                                .OptionalMatch("(d)-[r:POCETAK_VLADAVINE]->(pg:Godina)")
                                .OptionalMatch("(d)-[r2:KRAJ_VLADAVINE]->(kg:Godina)")
                                .Delete("r, r2, d")
                                .ExecuteWithoutResultsAsync();
            await _dinastijaCollection.DeleteOneAsync(d => d.ID == id);
            return Ok($"Dinastija sa ID: {id} je uspesno obrisana iz baze!");
        }

        catch (Exception ex)
        {
            return StatusCode(500, $"Došlo je do greške pri radu sa Neo4j bazom: {ex.Message}");
        }
    }
    
    [HttpGet("GetAllDinastije")]
    public async Task<IActionResult> GetAllDinastije()
    {
        try
        {
            var dinastije = (await _client.Cypher.Match("(d:Dinastija)")
                                           .Return(d => d.As<DinastijaNeo>())
                                           .ResultsAsync)
                                           .ToList();
            if (!dinastije.Any())
                return NotFound("Nije pronađena nijedna dinastija u bazi");

            var ids = dinastije.Select(d => d.ID).ToList();
            var mongoList = await _dinastijaCollection.Find(m => ids.Contains(m.ID)).ToListAsync();
            var result = dinastije.Select(din =>
            {
                var mongo = mongoList.FirstOrDefault(m => m.ID == din.ID);
                return new DinastijaDto
                {
                    ID = din.ID,
                    Naziv = din.Naziv,
                    PocetakVladavineGod = din.PocetakVladavineGod,
                    PocetakVladavinePNE = din.PocetakVladavinePNE,
                    KrajVladavineGod = din.KrajVladavineGod,
                    KrajVladavinePNE = din.KrajVladavinePNE,
                    Slika = mongo?.Slika
                };
            });
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Došlo je do greške: {ex.Message}");
        }
    }

    [HttpGet("GetDinastijaTree/{id}")]
    public async Task<IActionResult> GetDinastijaTree(Guid id)
    {
        var vladari = await _vladarService.GetVladariFlat(id);
        var licnosti = await _licnostService.GetLicnostiFlat(id);

        var flatList = vladari
            .Concat(licnosti)
            .GroupBy(p => p.ID)
            .Select(g => g.First())
            .ToList();

        var trees = _treeBuilder.BuildTrees(flatList);
        return Ok(trees);
    }

    [HttpGet("GetDinastijaClanovi/{id}")]
    public async Task<IActionResult> GetDinastijaClanovi(Guid id)
    {
        var vladari = await _vladarService.GetVladariFlat(id);
        var licnosti = await _licnostService.GetLicnostiFlat(id);

        var flatList = vladari
            .Concat(licnosti)
            .GroupBy(p => p.ID)
            .Select(g => g.First())
            .ToList();

        return Ok(flatList);
    }

}