using Microsoft.AspNetCore.Mvc;
using Neo4jClient;
using System;
using System.Threading.Tasks;
using System.Reflection.Metadata;
using KrvNijeVoda.Back;
using MongoDB.Driver;
[Route("api")]
[ApiController]
public class LicnostController : ControllerBase
{
    private readonly IGraphClient _client;
    private readonly GodinaService _godinaService;
    private readonly IMongoCollection<LicnostMongo> _licnostCollection;
    public LicnostController(Neo4jService neo4jService, GodinaService godinaService, MongoService mongoService)
    {
        _client = neo4jService.GetClient();
        _godinaService = godinaService;
        _licnostCollection = mongoService.GetCollection<LicnostMongo>("Licnosti");
    }

   [HttpPost("CreateLicnost")]
    public async Task<IActionResult> CreateLicnost(
        [FromForm] LicnostDto licnost,
        [FromForm] IFormFile? slika)
    {
        try
        {
            if (slika != null && slika.Length > 0)
            {
                var uploadsPath = Path.Combine("..", "front", "public", "images", "licnosti");

                uploadsPath = Path.GetFullPath(uploadsPath);

                if (!Directory.Exists(uploadsPath))
                    Directory.CreateDirectory(uploadsPath);

                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(slika.FileName)}";
                var fullPath = Path.Combine(uploadsPath, fileName);

                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await slika.CopyToAsync(stream);
                }

                licnost.Slika = $"{fileName}";
            }

            var postojecaLicnost = (await _client.Cypher
                .Match("(l:Licnost)")
                .Where("l.Titula = $titula AND l.Ime = $ime AND l.Prezime = $prezime")
                .WithParam("titula", licnost.Titula)
                .WithParam("ime", licnost.Ime)
                .WithParam("prezime", licnost.Prezime)
                .Return(l => l.As<LicnostNeo>())
                .ResultsAsync)
                .FirstOrDefault();

            if (postojecaLicnost != null)
                return BadRequest($"Licnost {licnost.Titula} {licnost.Ime} {licnost.Prezime} vec postoji u bazi sa ID: {postojecaLicnost.ID}!");

            var licnostID = Guid.NewGuid();

            var query = _client.Cypher
                .Create("(l:Licnost {ID: $id, Titula: $titula, Ime: $ime, Prezime: $prezime, Pol: $pol})")
                .WithParam("id", licnostID)
                .WithParam("titula", licnost.Titula)
                .WithParam("ime", licnost.Ime)
                .WithParam("prezime", licnost.Prezime)
                .WithParam("pol", licnost.Pol);

            if (licnost.GodinaRodjenja != 0)
            {
                await _godinaService.DodajGodinu(licnost.GodinaRodjenja, licnost.GodinaRodjenjaPNE);
                query = query.With("l")
                    .Match("(gr:Godina {God: $rodj, IsPNE: $pne})")
                    .WithParam("rodj", licnost.GodinaRodjenja)
                    .WithParam("pne", licnost.GodinaRodjenjaPNE)
                    .Set("l.GodinaRodjenja = $rodj, l.GodinaRodjenjaPNE = $pne")
                    .Create("(l)-[:RODJEN]->(gr)");
            }

            if (licnost.GodinaSmrti != 0)
            {
                await _godinaService.DodajGodinu(licnost.GodinaSmrti, licnost.GodinaSmrtiPNE);
                query = query.With("l")
                    .Match("(gs:Godina {God: $smrt, IsPNE: $pnes})")
                    .WithParam("smrt", licnost.GodinaSmrti)
                    .WithParam("pnes", licnost.GodinaSmrtiPNE)
                    .Set("l.GodinaSmrti = $smrt, l.GodinaSmrtiPNE = $pnes")
                    .Create("(l)-[:UMRO]->(gs)");
            }

            if (!string.IsNullOrWhiteSpace(licnost.MestoRodjenja) && licnost.MestoRodjenja != "string")
            {
                var z = (await _client.Cypher
                    .Match("(z:Zemlja)")
                    .Where("toLower(z.Naziv) = toLower($naziv)")
                    .WithParam("naziv", licnost.MestoRodjenja)
                    .Return(z => z.As<ZemljaNeo>())
                    .ResultsAsync)
                    .FirstOrDefault();

                if (z != null)
                    query = query.With("l")
                        .Match("(z:Zemlja)")
                        .Where("toLower(z.Naziv) = toLower($naziv)")
                        .WithParam("naziv", licnost.MestoRodjenja)
                        .Create("(l)-[:RODJEN_U]->(z)")
                        .Set("l.MestoRodjenja = $naziv");
            }

            await query.ExecuteWithoutResultsAsync();

            if (string.IsNullOrWhiteSpace(licnost.Slika))
            {
                licnost.Slika = licnost.Pol == "M" ? "placeholder_muski.png" : "placeholder_zenski.png";
            }

            if (!string.IsNullOrWhiteSpace(licnost.Tekst) || !string.IsNullOrWhiteSpace(licnost.Slika))
            {
                var licnostMongo = new LicnostMongo
                {
                    ID = licnostID,
                    Tekst = licnost.Tekst,
                    Slika = licnost.Slika
                };

                await _licnostCollection.InsertOneAsync(licnostMongo);
            }

            return Ok($"Uspesno dodata licnost sa ID:{licnostID} u bazu!");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Greška: {ex.Message}");
        }
    }


    [HttpGet("GetLicnost/{id}")]
    public async Task<IActionResult> GetLicnost(Guid id)
    {
        try
        {
            var lic = (await _client.Cypher.Match("(l:Licnost)")
                                           .Where((LicnostNeo l) => l.ID == id)
                                           .Return(l => l.As<LicnostNeo>())
                                           .ResultsAsync)
                                           .FirstOrDefault();

            if (lic == null)
            {
                return BadRequest($"Licnost sa ID: {id} nije pronadjena u bazi!");
            }
            var mongoDoc = await _licnostCollection.Find(d => d.ID == id).FirstOrDefaultAsync();
            var dto = new LicnostDto
            {
                ID = lic.ID,
                Titula = lic.Titula,
                Ime = lic.Ime,
                Prezime = lic.Prezime,
                GodinaRodjenja = lic.GodinaRodjenja,
                GodinaRodjenjaPNE = lic.GodinaRodjenjaPNE,
                GodinaSmrti = lic.GodinaSmrti,
                GodinaSmrtiPNE = lic.GodinaSmrtiPNE,
                Pol = lic.Pol,
                Slika = mongoDoc?.Slika,
                MestoRodjenja = lic.MestoRodjenja ?? "",
                Tekst = mongoDoc?.Tekst
            };

            return Ok(dto);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Došlo je do greške pri radu sa Neo4j bazom: {ex.Message}");
        }
    }
    
    [HttpPut("UpdateLicnost/{id}")]
    public async Task<IActionResult> UpdateLicnost([FromForm] LicnostDto licnost, Guid id, [FromForm] IFormFile? slika)
    {
        try 
        {
            var lic = (await _client.Cypher.Match("(l:Licnost)")
                                        .Where((LicnostNeo l) => l.ID == id)
                                        .Return(l => l.As<LicnostNeo>())
                                        .ResultsAsync)
                                        .FirstOrDefault();

            if(lic == null)
                return BadRequest($"Licnost sa ID: {id} nije pronadjena u bazi!");

            var postojecaLicnost = (await _client.Cypher.Match("(l:Licnost)")
                                                        .Where("l.Titula = $titula AND l.Ime = $ime AND l.Prezime = $prezime AND l.ID <> $id")
                                                        .WithParam("titula", licnost.Titula)
                                                        .WithParam("ime", licnost.Ime)
                                                        .WithParam("prezime", licnost.Prezime)
                                                        .WithParam("id", id)
                                                        .Return(l => l.As<LicnostNeo>())
                                                        .ResultsAsync)
                                                        .FirstOrDefault();

            if (postojecaLicnost != null)
                return BadRequest($"Licnost {licnost.Titula} {licnost.Ime} {licnost.Prezime} vec postoji u bazi sa ID: {postojecaLicnost.ID}!");

            var query = _client.Cypher.Match("(l:Licnost)")
                                    .Where((LicnostNeo l) => l.ID == id)
                                    .Set("l.Titula = $titula, l.Ime = $ime, l.Prezime = $prezime, l.Pol = $pol")
                                    .WithParam("titula", licnost.Titula)
                                    .WithParam("ime", licnost.Ime)
                                    .WithParam("prezime", licnost.Prezime)
                                    .WithParam("pol", licnost.Pol);

            var uploadsPath = Path.Combine("..", "front", "public", "images", "licnosti");
            uploadsPath = Path.GetFullPath(uploadsPath);
            if (!Directory.Exists(uploadsPath))
                Directory.CreateDirectory(uploadsPath);

            if (slika != null && slika.Length > 0)
            {
                var staraMongo = (await _licnostCollection.Find(d => d.ID == id).FirstOrDefaultAsync())?.Slika;

                if (!string.IsNullOrWhiteSpace(staraMongo) && 
                    staraMongo != "placeholder_muski.png" && 
                    staraMongo != "placeholder_zenski.png")
                {
                    var staraPutanja = Path.Combine(uploadsPath, staraMongo);
                    if (System.IO.File.Exists(staraPutanja))
                        System.IO.File.Delete(staraPutanja);
                }

                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(slika.FileName)}";
                var fullPath = Path.Combine(uploadsPath, fileName);

                using (var stream = new FileStream(fullPath, FileMode.Create))
                    await slika.CopyToAsync(stream);

                licnost.Slika = fileName;
            }

            if (licnost.GodinaRodjenja != 0)
            {
                if (lic.GodinaRodjenja != licnost.GodinaRodjenja || lic.GodinaRodjenjaPNE != licnost.GodinaRodjenjaPNE)
                {
                    await _godinaService.DodajGodinu(licnost.GodinaRodjenja, licnost.GodinaRodjenjaPNE);
                    query = query.With("l")
                                .Match("(l)-[rRodjen:RODJEN]->(sg:Godina)")
                                .Match("(gRodj:Godina {God: $godRodjenje, IsPNE: $pneRodjenje})")
                                .WithParam("godRodjenje", licnost.GodinaRodjenja)
                                .WithParam("pneRodjenje", licnost.GodinaRodjenjaPNE)
                                .Delete("rRodjen")
                                .Create("(l)-[:RODJEN]->(gRodj)")
                                .Set("l.GodinaRodjenja = $godRodjenje, l.GodinaRodjenjaPNE = $pneRodjenje");
                }
            }
            else
            {
                query = query.With("l")
                            .OptionalMatch("(l)-[rRodjen:RODJEN]->()")
                            .Delete("rRodjen")
                            .Set("l.GodinaRodjenja = 0, l.GodinaRodjenjaPNE = false");
            }

            if (licnost.GodinaSmrti != 0)
            {
                if (lic.GodinaSmrti != licnost.GodinaSmrti || lic.GodinaSmrtiPNE != licnost.GodinaSmrtiPNE)
                {
                    await _godinaService.DodajGodinu(licnost.GodinaSmrti, licnost.GodinaSmrtiPNE);
                    query = query.With("l")
                                .Match("(l)-[rUmro:UMRO]->(sgs:Godina)")
                                .Match("(gSmrt:Godina {God: $godSmrti, IsPNE: $pneSmrti})")
                                .WithParam("godSmrti", licnost.GodinaSmrti)
                                .WithParam("pneSmrti", licnost.GodinaSmrtiPNE)
                                .Delete("rUmro")
                                .Create("(l)-[:UMRO]->(gSmrt)")
                                .Set("l.GodinaSmrti = $godSmrti, l.GodinaSmrtiPNE = $pneSmrti");
                }
            }
            else
            {
                query = query.With("l")
                            .OptionalMatch("(l)-[rUmro:UMRO]->()")
                            .Delete("rUmro")
                            .Set("l.GodinaSmrti = 0, l.GodinaSmrtiPNE = false");
            }

            if (!string.IsNullOrWhiteSpace(licnost.MestoRodjenja) && licnost.MestoRodjenja != "string")
            {
                var z = (await _client.Cypher.Match("(z:Zemlja)")
                                            .Where("toLower(z.Naziv) = toLower($naziv)")
                                            .WithParam("naziv", licnost.MestoRodjenja)
                                            .Return(z => z.As<ZemljaNeo>())
                                            .ResultsAsync)
                                            .FirstOrDefault();

                if (z != null)
                {
                    if (lic.MestoRodjenja != licnost.MestoRodjenja)
                    {
                        query = query.With("l")
                                    .Match("(z:Zemlja)")
                                    .Where("toLower(z.Naziv) = toLower($nazivMesto)")
                                    .Match("(l)-[rMesto:RODJEN_U]->(sz:Zemlja)")
                                    .WithParam("nazivMesto", licnost.MestoRodjenja)
                                    .Delete("rMesto")
                                    .Create("(l)-[:RODJEN_U]->(z)")
                                    .Set("l.MestoRodjenja = $nazivMesto");
                    }
                }
            }
            else
            {
                query = query.With("l")
                            .OptionalMatch("(l)-[rMesto:RODJEN_U]->()")
                            .Delete("rMesto")
                            .Set("l.MestoRodjenja = 'string'");
            }

            if (!string.IsNullOrWhiteSpace(licnost.Tekst) || !string.IsNullOrWhiteSpace(licnost.Slika))
            {
                var filter = Builders<LicnostMongo>.Filter.Eq(d => d.ID, id);
                var update = Builders<LicnostMongo>.Update.Combine(
                    Builders<LicnostMongo>.Update.Set(d => d.Tekst, licnost.Tekst),
                    Builders<LicnostMongo>.Update.Set(d => d.Slika, licnost.Slika)
                );
                await _licnostCollection.UpdateOneAsync(filter, update);
            }

            await query.ExecuteWithoutResultsAsync();

            return Ok($"Licnost sa id: {id} je uspesno promenjena!");
        }
        catch (Exception ex)  
        {
            return StatusCode(500, $"Došlo je do greške pri radu sa Neo4j bazom: {ex.Message}");
        }        
    }



    [HttpDelete("DeleteLicnost/{id}")]
    public async Task<IActionResult> DeleteLicnost(Guid id)
    {
        try
        {
            var lic = (await _client.Cypher.Match("(l:Licnost)")
                                           .Where((LicnostNeo l) => l.ID == id)
                                           .Return(l => l.As<LicnostNeo>())
                                           .ResultsAsync)
                                           .FirstOrDefault();

            if (lic == null)
            {
                return BadRequest($"Licnost sa ID: {id} nije pronadjena u bazi!");
            }

            await _client.Cypher
                            .Match("(l:Licnost)")
                            .Where((LicnostNeo l) => l.ID == id)
                            .DetachDelete("l") 
                            .ExecuteWithoutResultsAsync();
            await _licnostCollection.DeleteOneAsync(d => d.ID == id);
            return Ok($"Licnost sa id:{id} uspesno obrisana iz baze!");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Došlo je do greške pri radu sa Neo4j bazom: {ex.Message}");
        }
    }
    
    [HttpGet("GetAllLicnosti")]
    public async Task<IActionResult> GetAllLicnosti()
    {
        try
        {
            var licnosti = (await _client.Cypher.Match("(l:Licnost)")
                                           .Where("NOT l:Vladar")
                                           .Return(l => l.As<LicnostNeo>())
                                           .ResultsAsync)
                                           .ToList();

            if (!licnosti.Any())
            {
                return BadRequest($"Nije pronađena nijedna ličnost u bazi!");
            }
            var ids = licnosti.Select(l => l.ID).ToList();
            var mongoList = await _licnostCollection.Find(m => ids.Contains(m.ID)).ToListAsync();
            var result = licnosti.Select(l =>
            {
                var mongo = mongoList.FirstOrDefault(m => m.ID == l.ID);
                return new LicnostDto
                {
                    ID = l.ID,
                    Titula = l.Titula,
                    Ime = l.Ime,
                    Prezime = l.Prezime,
                    GodinaRodjenja = l.GodinaRodjenja,
                    GodinaRodjenjaPNE = l.GodinaRodjenjaPNE,
                    GodinaSmrti = l.GodinaSmrti,
                    GodinaSmrtiPNE = l.GodinaSmrtiPNE,
                    Pol = l.Pol,
                    Slika = mongo?.Slika,
                    MestoRodjenja = l.MestoRodjenja ?? "",
                    Tekst = mongo?.Tekst
                };
            }).ToList();

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Došlo je do greške: {ex.Message}");
        }
    }

    [HttpPost("DodajLicnostUDinastiju")]
        public async Task<IActionResult> DodajLicnostUDinastiju([FromQuery] Guid sinId, [FromQuery] Guid roditeljId)
        {
        try
        {
            if (sinId == roditeljId)
                return BadRequest("Sin i roditelj ne mogu biti ista osoba.");

            var sin = (await _client.Cypher
                .Match("(s:Licnost)")
                .Where((LicnostNeo s) => s.ID == sinId)
                .Return(s => s.As<LicnostNeo>())
                .ResultsAsync)
                .FirstOrDefault();

            if (sin == null)
                return BadRequest($"Licnost (sin) sa ID: {sinId} nije pronađena.");

            var roditelj = (await _client.Cypher
                .Match("(r:Licnost)")
                .Where((LicnostNeo r) => r.ID == roditeljId)
                .Return(r => r.As<LicnostNeo>())
                .ResultsAsync)
                .FirstOrDefault();

            if (roditelj == null)
                return BadRequest($"Licnost (roditelj) sa ID: {roditeljId} nije pronađena.");

            var postojiVeza = (await _client.Cypher
                .Match("(r:Licnost)-[rel:JE_RODITELJ]->(s:Licnost)")
                .Where((LicnostNeo r) => r.ID == roditeljId)
                .AndWhere((LicnostNeo s) => s.ID == sinId)
                .Return(rel => rel.As<object>())
                .ResultsAsync)
                .Any();

            if (postojiVeza)
                return BadRequest("Veza JE_RODITELJ već postoji između ove dve ličnosti.");

            var brojRoditelja = (await _client.Cypher
                .Match("(r:Licnost)-[:JE_RODITELJ]->(s:Licnost)")
                .Where((LicnostNeo s) => s.ID == sinId)
                .Return(r => r.As<LicnostNeo>())
                .ResultsAsync)
                .Count();

            if (brojRoditelja >= 2)
                return BadRequest("Sin već ima dva roditelja. Nije moguće dodati trećeg.");
                
            await _client.Cypher
                .Match("(r:Licnost)", "(s:Licnost)")
                .Where((LicnostNeo r) => r.ID == roditeljId)
                .AndWhere((LicnostNeo s) => s.ID == sinId)
                .Create("(r)-[:JE_RODITELJ]->(s)")
                .ExecuteWithoutResultsAsync();

            return Ok($"Veza JE_RODITELJ kreirana između roditelja (ID: {roditeljId}) i sina (ID: {sinId}).");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Došlo je do greške: {ex.Message}");
        }
    }
}







