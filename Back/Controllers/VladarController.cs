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
public class VladarController : ControllerBase
{
    private readonly IGraphClient _client;
    private readonly GodinaService _godinaService;
    // private readonly LokacijaService _lokacijaService;
    // private readonly ZemljaService _zemljaService;
    // private readonly DinastijaService _dinastijaService;
    private readonly IMongoCollection<VladarMongo> _vladarCollection;
    private readonly ITreeBuilder _treeBuilder;
    public VladarController(Neo4jService neo4jService, GodinaService godinaService, MongoService mongoService, ITreeBuilder treeBuilder/*, LokacijaService lokacijaService, ZemljaService zemljaService, DinastijaService dinastijaService*/)
    {
        _client = neo4jService.GetClient();
        _godinaService = godinaService;
        _vladarCollection = mongoService.GetCollection<VladarMongo>("Vladari");
        _treeBuilder = treeBuilder;
        // _lokacijaService = lokacijaService;
        // _zemljaService = zemljaService;
        // _dinastijaService = dinastijaService;

    }

    [HttpPost("CreateVladar")]
public async Task<IActionResult> CreateVladar([FromForm] VladarDto vladar, [FromForm] IFormFile? slika)
{
    try
    {
        // =========================
        // 1. UPLOAD SLIKE
        // =========================
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

            vladar.Slika = fileName;
        }

        // =========================
        // 2. PROVERA POSTOJANJA
        // =========================
        var postojeciVladar = (await _client.Cypher
            .Match("(l:Licnost:Vladar)")
            .Where("toLower(l.Titula) = toLower($titula) AND toLower(l.Ime) = toLower($ime) AND toLower(l.Prezime) = toLower($prezime)")
            .WithParam("titula", vladar.Titula)
            .WithParam("ime", vladar.Ime)
            .WithParam("prezime", vladar.Prezime)
            .Return(l => l.As<VladarNeo>())
            .ResultsAsync)
            .FirstOrDefault();

        if (postojeciVladar != null)
            return BadRequest($"Vladar {vladar.Titula} {vladar.Ime} {vladar.Prezime} već postoji u bazi sa ID: {postojeciVladar.ID}!");

        var vladarID = Guid.NewGuid();

        // =========================
        // 3. KREIRANJE NOD-a
        // =========================
        var query = _client.Cypher
            .Create("(v:Licnost:Vladar {ID: $id, Titula: $titula, Ime: $ime, Prezime: $prezime, Pol: $pol, MestoRodjenja: $mestoRodjenja})")
            .WithParam("id", vladarID)
            .WithParam("titula", vladar.Titula)
            .WithParam("ime", vladar.Ime)
            .WithParam("prezime", vladar.Prezime)
            .WithParam("pol", vladar.Pol)
            .WithParam("mestoRodjenja", vladar.MestoRodjenja);

        // =========================
        // 4. GODINE
        // =========================
        if (vladar.GodinaRodjenja != 0)
        {
            await _godinaService.DodajGodinu(vladar.GodinaRodjenja, vladar.GodinaRodjenjaPNE);
            query = query.With("v")
                         .Match("(gr:Godina {God: $rodj, IsPNE: $pner})")
                         .WithParam("rodj", vladar.GodinaRodjenja)
                         .WithParam("pner", vladar.GodinaRodjenjaPNE)
                         .Set("v.GodinaRodjenja = $rodj, v.GodinaRodjenjaPNE = $pner")
                         .Create("(v)-[:RODJEN]->(gr)");
        }

        if (vladar.GodinaSmrti != 0)
        {
            await _godinaService.DodajGodinu(vladar.GodinaSmrti, vladar.GodinaSmrtiPNE);
            query = query.With("v")
                         .Match("(gs:Godina {God: $smrt, IsPNE: $pnes})")
                         .WithParam("smrt", vladar.GodinaSmrti)
                         .WithParam("pnes", vladar.GodinaSmrtiPNE)
                         .Set("v.GodinaSmrti = $smrt, v.GodinaSmrtiPNE = $pnes")
                         .Create("(v)-[:UMRO]->(gs)");
        }

        // =========================
        // 5. Vladavina
        // =========================
        if (vladar.PocetakVladavineGod != 0)
        {
            await _godinaService.DodajGodinu(vladar.PocetakVladavineGod, vladar.PocetakVladavinePNE);
            query = query.With("v")
                         .Match("(gp:Godina {God: $poc, IsPNE: $pne})")
                         .WithParam("poc", vladar.PocetakVladavineGod)
                         .WithParam("pne", vladar.PocetakVladavinePNE)
                         .Set("v.PocetakVladavineGod = $poc, v.PocetakVladavinePNE = $pne")
                         .Create("(v)-[:VLADAO_OD]->(gp)");
        }

        if (vladar.KrajVladavineGod != 0)
        {
            await _godinaService.DodajGodinu(vladar.KrajVladavineGod, vladar.KrajVladavinePNE);
            query = query.With("v")
                         .Match("(gk:Godina {God: $gkv, IsPNE: $kvpne})")
                         .WithParam("gkv", vladar.KrajVladavineGod)
                         .WithParam("kvpne", vladar.KrajVladavinePNE)
                         .Set("v.KrajVladavineGod = $gkv, v.KrajVladavinePNE = $kvpne")
                         .Create("(v)-[:VLADAO_DO]->(gk)");
        }

        // =========================
        // 6. Mesto rodjenja
        // =========================
        if (!string.IsNullOrWhiteSpace(vladar.MestoRodjenja) && vladar.MestoRodjenja != "string")
        {
            var z = (await _client.Cypher.Match("(z:Zemlja)")
                                         .Where("toLower(z.Naziv) = toLower($mestoNaziv)")
                                         .WithParam("mestoNaziv", vladar.MestoRodjenja)
                                         .Return(z => z.As<ZemljaNeo>())
                                         .ResultsAsync)
                                         .FirstOrDefault();

            if (z != null)
            {
                query = query.With("v")
                             .Match("(z:Zemlja)")
                             .Where("toLower(z.Naziv) = toLower($mestoNaziv)")
                             .WithParam("mestoNaziv", vladar.MestoRodjenja)
                             .Create("(v)-[:RODJEN_U]->(z)")
                             .Set("v.MestoRodjenja = $mestoNaziv");
            }
            else
            {
                query = query.With("v")
                             .Set("v.MestoRodjenja = $mr")
                             .WithParam("mr", "string");
            }
        }

        // =========================
        // 7. Dinastija
        // =========================
        if (vladar.Dinastija != null && !string.IsNullOrWhiteSpace(vladar.Dinastija.Naziv) && vladar.Dinastija.Naziv != "string")
        {
            var din = (await _client.Cypher.Match("(d:Dinastija)")
                                           .Where("toLower(d.Naziv) = toLower($dinNaziv)")
                                           .WithParam("dinNaziv", vladar.Dinastija.Naziv)
                                           .Return(d => d.As<DinastijaNeo>())
                                           .ResultsAsync)
                                           .FirstOrDefault();

            if (din != null)
            {
                query = query.With("v")
                             .Match("(d:Dinastija)")
                             .Where("toLower(d.Naziv) = toLower($dinNaziv)")
                             .WithParam("dinNaziv", vladar.Dinastija.Naziv)
                             .Create("(v)-[:PRIPADA_DINASTIJI]->(d)");
            }
        }

        // =========================
        // 8. EXECUTE QUERY
        // =========================
        await query.ExecuteWithoutResultsAsync();

        // =========================
        // 9. MONGO
        // =========================
        if (string.IsNullOrWhiteSpace(vladar.Slika))
            vladar.Slika = vladar.Pol == "M" ? "placeholder_muski.png" : "placeholder_zenski.png";

        if (!string.IsNullOrWhiteSpace(vladar.Tekst) || !string.IsNullOrWhiteSpace(vladar.Slika) || !string.IsNullOrWhiteSpace(vladar.Teritorija))
        {
            var vladarMongo = new VladarMongo
            {
                ID = vladarID,
                Tekst = vladar.Tekst,
                Slika = vladar.Slika,
                Teritorija = vladar.Teritorija
            };
            await _vladarCollection.InsertOneAsync(vladarMongo);
        }

        return Ok($"Uspešno dodat vladar sa ID:{vladarID} u bazu!");
    }
    catch (Exception ex)
    {
        return StatusCode(500, $"Došlo je do greške pri radu sa Neo4j bazom: {ex.Message}");
    }
}



    [HttpGet("GetVladar/{id}")]
    public async Task<IActionResult> GetVladar(Guid id)
    {
        try
        {
            var vl = (await _client.Cypher.Match("(v:Licnost:Vladar)")
                                          .Where((VladarNeo v) => v.ID == id)
                                          //   .OptionalMatch("(l)-[r:RODJEN]->(gr:Godina)")
                                          //   .OptionalMatch("(l)-[r2:UMRO]->(gs:Godina)")
                                          //   .OptionalMatch("(l)-[r3:RODJEN_U]->(m:Lokacija)-[:PRIPADA_ZEMLJI]->(z:Zemlja)")
                                          //   .OptionalMatch("(l)-[r4:VLADAO_OD]->(gpv:Godina)")
                                          //   .OptionalMatch("(l)-[r5:VLADAO_DO]->(gkv:Godina)")
                                          .OptionalMatch("(v)-[r6:PRIPADA_DINASTIJI]->(d:Dinastija)")
                                          // .Return((l, gr, gs, m, z, gpv, gkv, d) => new {
                                          //     Vladar = l.As<Vladar>(),
                                          //     Rodjen = gr.As<Godina>(),
                                          //     Umro = gs.As<Godina>(),
                                          //     Mesto =  m.As<Lokacija>(),
                                          //     Zemlja = z.As<Zemlja>(),
                                          //     Pocetak = gpv.As<Godina>(),
                                          //     Kraj = gkv.As<Godina>(),
                                          //     Dinastija = d.As<Dinastija>()                                        
                                          // }) 
                                          .Return((v, d) => new
                                          {
                                              Vladar = v.As<VladarNeo>(),
                                              Dinastija = d.As<DinastijaNeo>()
                                          })
                                          //.Return(vl => vl.As<Vladar>())
                                          .ResultsAsync)
                                          .FirstOrDefault();


            if (vl.Vladar == null)
            {
                return BadRequest($"Vladar sa ID: {id} nije pronadjen u bazi!");
            }
            var mongoDoc = await _vladarCollection.Find(d => d.ID == id).FirstOrDefaultAsync();
            var dto = new VladarDto
            {
                ID = vl.Vladar.ID,
                Titula = vl.Vladar.Titula,
                Ime = vl.Vladar.Ime,
                Prezime = vl.Vladar.Prezime,
                GodinaRodjenja = vl.Vladar.GodinaRodjenja,
                GodinaRodjenjaPNE = vl.Vladar.GodinaRodjenjaPNE,
                GodinaSmrti = vl.Vladar.GodinaSmrti,
                GodinaSmrtiPNE = vl.Vladar.GodinaSmrtiPNE,
                Pol = vl.Vladar.Pol,
                //Slika = vl.Vladar.Slika,
                MestoRodjenja = vl.Vladar.MestoRodjenja,
                //Tekst = vl.Vladar.Tekst,
                Dinastija = vl.Dinastija, // ?? new Dinastija() ne moze jer mora da ima naziv da bi kreirao novu zato sad ostavljam ovako 
                PocetakVladavineGod = vl.Vladar.PocetakVladavineGod,
                PocetakVladavinePNE = vl.Vladar.PocetakVladavinePNE,
                KrajVladavineGod = vl.Vladar.KrajVladavineGod,
                KrajVladavinePNE = vl.Vladar.KrajVladavinePNE,
                //Clanovi = item.Clanovi?.ToList() ?? new List<Licnost>()  // If no Licnost found, return empty list
                Tekst = mongoDoc?.Tekst,
                Slika = mongoDoc?.Slika,
                Teritorija = mongoDoc.Teritorija
            };
            return Ok(dto);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Došlo je do greške pri radu sa Neo4j bazom: {ex.Message}");
        }
    }

   [HttpPut("UpdateVladar/{id}")]
public async Task<IActionResult> UpdateVladar([FromForm] VladarDto vladar, Guid id, [FromForm] IFormFile? slika)
{
    try
    {
        // =========================
        // 1. Pronadji Vladara i postojece veze
        // =========================
        var vl = (await _client.Cypher
            .Match("(v:Licnost:Vladar)")
            .Where((VladarNeo v) => v.ID == id)
            .OptionalMatch("(v)-[:PRIPADA_DINASTIJI]->(d)")
            .Return((v, d) => new
            {
                Dinastija = d.As<DinastijaNeo>(),
                Vladar = v.As<VladarNeo>()
            })
            .ResultsAsync)
            .FirstOrDefault();

        if (vl == null || vl.Vladar == null)
            return BadRequest($"Vladar sa ID: {id} nije pronađen u bazi!");

        // =========================
        // 2. Osnovno update atributa Vladara
        // =========================
        var query = _client.Cypher
            .Match("(v:Licnost:Vladar)")
            .Where((VladarNeo v) => v.ID == id)
            .OptionalMatch("(v)-[r:PRIPADA_DINASTIJI]->(:Dinastija)")
            .Set("v.Titula = $titula, v.Ime = $ime, v.Prezime = $prezime, v.Pol = $pol")
            .WithParam("titula", vladar.Titula)
            .WithParam("ime", vladar.Ime)
            .WithParam("prezime", vladar.Prezime)
            .WithParam("pol", vladar.Pol);

        // =========================
        // 3. UPLOAD NOVE SLIKE
        // =========================
        var uploadsPath = Path.Combine("..", "front", "public", "images", "licnosti");
        uploadsPath = Path.GetFullPath(uploadsPath);
        if (!Directory.Exists(uploadsPath))
            Directory.CreateDirectory(uploadsPath);

        if (slika != null && slika.Length > 0)
        {
            // Dohvati staru sliku iz Mongo-a
            var staraMongo = (await _vladarCollection.Find(d => d.ID == id).FirstOrDefaultAsync())?.Slika;

            // Obrisi staru sliku ako nije placeholder
            if (!string.IsNullOrWhiteSpace(staraMongo) &&
                staraMongo != "placeholder_muski.png" &&
                staraMongo != "placeholder_zenski.png")
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
            vladar.Slika = fileName;
        }

        // =========================
        // 4. Godina rođenja
        // =========================
        if (vladar.GodinaRodjenja != 0)
        {
            await _godinaService.DodajGodinu(vladar.GodinaRodjenja, vladar.GodinaRodjenjaPNE);

            if (vl.Vladar.GodinaRodjenja != vladar.GodinaRodjenja || vl.Vladar.GodinaRodjenjaPNE != vladar.GodinaRodjenjaPNE)
            {
                query = query.With("v")
                    .OptionalMatch("(v)-[r:RODJEN]->(:Godina)")
                    .Delete("r")
                    .With("v")
                    .Match("(g:Godina {God: $god, IsPNE: $pne})")
                    .WithParam("god", vladar.GodinaRodjenja)
                    .WithParam("pne", vladar.GodinaRodjenjaPNE)
                    .Create("(v)-[:RODJEN]->(g)")
                    .Set("v.GodinaRodjenja = $god, v.GodinaRodjenjaPNE = $pne");
            }
        }
        else
        {
            query = query.With("v")
                         .OptionalMatch("(v)-[r:RODJEN]->()")
                         .Delete("r")
                         .Set("v.GodinaRodjenja = 0, v.GodinaRodjenjaPNE = false");
        }

        // =========================
        // 5. Godina smrti
        // =========================
        if (vladar.GodinaSmrti != 0)
        {
            await _godinaService.DodajGodinu(vladar.GodinaSmrti, vladar.GodinaSmrtiPNE);

            if (vl.Vladar.GodinaSmrti != vladar.GodinaSmrti || vl.Vladar.GodinaSmrtiPNE != vladar.GodinaSmrtiPNE)
            {
                query = query.With("v")
                    .OptionalMatch("(v)-[r:UMRO]->(:Godina)")
                    .Delete("r")
                    .With("v")
                    .Match("(g:Godina {God: $god, IsPNE: $pne})")
                    .WithParam("god", vladar.GodinaSmrti)
                    .WithParam("pne", vladar.GodinaSmrtiPNE)
                    .Create("(v)-[:UMRO]->(g)")
                    .Set("v.GodinaSmrti = $god, v.GodinaSmrtiPNE = $pne");
            }
        }
        else
        {
            query = query.With("v")
                         .OptionalMatch("(v)-[r:UMRO]->()")
                         .Delete("r")
                         .Set("v.GodinaSmrti = 0, v.GodinaSmrtiPNE = false");
        }

        // =========================
        // 6. Mesto rodjenja
        // =========================
        if (!string.IsNullOrWhiteSpace(vladar.MestoRodjenja) && vladar.MestoRodjenja != "string")
        {
            var z = (await _client.Cypher
                .Match("(z:Zemlja)")
                .Where("toLower(z.Naziv) = toLower($n)")
                .WithParam("n", vladar.MestoRodjenja)
                .Return(z => z.As<ZemljaNeo>())
                .ResultsAsync)
                .FirstOrDefault();

            if (z != null && vl.Vladar.MestoRodjenja != vladar.MestoRodjenja)
            {
                query = query.With("v")
                    .OptionalMatch("(v)-[r:RODJEN_U]->(:Zemlja)")
                    .Delete("r")
                    .With("v")
                    .Match("(z:Zemlja)")
                    .Where("toLower(z.Naziv) = toLower($n)")
                    .WithParam("n", vladar.MestoRodjenja)
                    .Create("(v)-[:RODJEN_U]->(z)")
                    .Set("v.MestoRodjenja = $n");
            }
        }
        else
        {
            query = query.With("v")
                         .OptionalMatch("(v)-[r:RODJEN_U]->()")
                         .Delete("r")
                         .Set("v.MestoRodjenja = 'string'");
        }

        // =========================
        // 7. Početak vladavine
        // =========================
        if (vladar.PocetakVladavineGod != 0)
        {
            await _godinaService.DodajGodinu(vladar.PocetakVladavineGod, vladar.PocetakVladavinePNE);

            if (vl.Vladar.PocetakVladavineGod != vladar.PocetakVladavineGod || vl.Vladar.PocetakVladavinePNE != vladar.PocetakVladavinePNE)
            {
                query = query.With("v")
                    .OptionalMatch("(v)-[r:VLADAO_OD]->(:Godina)")
                    .Delete("r")
                    .With("v")
                    .Match("(g:Godina {God: $god, IsPNE: $pne})")
                    .WithParam("god", vladar.PocetakVladavineGod)
                    .WithParam("pne", vladar.PocetakVladavinePNE)
                    .Create("(v)-[:VLADAO_OD]->(g)")
                    .Set("v.PocetakVladavineGod = $god, v.PocetakVladavinePNE = $pne");
            }
        }

        // =========================
        // 8. Kraj vladavine
        // =========================
        if (vladar.KrajVladavineGod != 0)
        {
            await _godinaService.DodajGodinu(vladar.KrajVladavineGod, vladar.KrajVladavinePNE);

            if (vl.Vladar.KrajVladavineGod != vladar.KrajVladavineGod || vl.Vladar.KrajVladavinePNE != vladar.KrajVladavinePNE)
            {
                query = query.With("v")
                    .OptionalMatch("(v)-[r:VLADAO_DO]->(:Godina)")
                    .Delete("r")
                    .With("v")
                    .Match("(g:Godina {God: $god, IsPNE: $pne})")
                    .WithParam("god", vladar.KrajVladavineGod)
                    .WithParam("pne", vladar.KrajVladavinePNE)
                    .Create("(v)-[:VLADAO_DO]->(g)")
                    .Set("v.KrajVladavineGod = $god, v.KrajVladavinePNE = $pne");
            }
        }

        // =========================
        // 9. Dinastija
        // =========================
        if (vladar.Dinastija != null &&
            !string.IsNullOrWhiteSpace(vladar.Dinastija.Naziv) &&
            vladar.Dinastija.Naziv != "string")
        {
            var din = (await _client.Cypher
                .Match("(d:Dinastija)")
                .Where("toLower(d.Naziv) = toLower($naziv)")
                .WithParam("naziv", vladar.Dinastija.Naziv)
                .Return(d => d.As<DinastijaNeo>())
                .ResultsAsync)
                .FirstOrDefault();

            if (din != null && (vl.Dinastija == null || vl.Dinastija.Naziv != din.Naziv))
            {
                query = query.With("v")
                    .OptionalMatch("(v)-[r:PRIPADA_DINASTIJI]->(:Dinastija)")
                    .Delete("r")
                    .With("v")
                    .Match("(d:Dinastija)")
                    .Where("toLower(d.Naziv) = toLower($naziv)")
                    .WithParam("naziv", vladar.Dinastija.Naziv)
                    .Create("(v)-[:PRIPADA_DINASTIJI]->(d)");
            }
        }

        // =========================
        // 10. Mongo update
        // =========================
        if (!string.IsNullOrWhiteSpace(vladar.Tekst) || 
            !string.IsNullOrWhiteSpace(vladar.Slika) || 
            !string.IsNullOrWhiteSpace(vladar.Teritorija))
        {
            var filter = Builders<VladarMongo>.Filter.Eq(d => d.ID, id);
            var update = Builders<VladarMongo>.Update.Combine(
                Builders<VladarMongo>.Update.Set(d => d.Tekst, vladar.Tekst),
                Builders<VladarMongo>.Update.Set(d => d.Slika, vladar.Slika),
                Builders<VladarMongo>.Update.Set(d => d.Teritorija, vladar.Teritorija)
            );
            await _vladarCollection.UpdateOneAsync(filter, update);
        }

        // =========================
        // 11. Execute query
        // =========================
        await query.ExecuteWithoutResultsAsync();

        return Ok($"Vladar sa ID {id} uspešno je ažuriran!");
    }
    catch (Exception ex)
    {
        return StatusCode(500, $"Došlo je do greške pri radu sa Neo4j bazom: {ex.Message}");
    }
}



    [HttpPut("UpdateVladarBezDinastije/{id}")]
    public async Task<IActionResult> UpdateVladarBezDinastije([FromBody] VladarDto vladar, Guid id)
    {
        //racunamo da mora da ima titulu, ime, prezime i od veza samo pocetak i kraj vladavine
        try {
            var vl = (await _client.Cypher.Match("(v:Licnost:Vladar)")
                                          .Where((VladarNeo v) => v.ID == id)
                                          //   .OptionalMatch("(l)-[r:RODJEN]->(gr:Godina)")
                                          //   .OptionalMatch("(l)-[r2:UMRO]->(gs:Godina)")
                                          //   .OptionalMatch("(l)-[r3:RODJEN_U]->(m:Lokacija)-[:PRIPADA_ZEMLJI]->(z:Zemlja)")
                                          //   .OptionalMatch("(l)-[r4:VLADAO_OD]->(gpv:Godina)")
                                          //   .OptionalMatch("(l)-[r5:VLADAO_DO]->(gkv:Godina)")
                                          //.OptionalMatch("(v)-[r6:PRIPADA_DINASTIJI]->(d:Dinastija)")
                                          // .Return((l, gr, gs, m, z, gpv, gkv, d) => new {
                                          //     Vladar = l.As<Vladar>(),
                                          //     Rodjen = gr.As<Godina>(),
                                          //     Umro = gs.As<Godina>(),
                                          //     Mesto =  m.As<Lokacija>(),
                                          //     Zemlja = z.As<Zemlja>(),
                                          //     Pocetak = gpv.As<Godina>(),
                                          //     Kraj = gkv.As<Godina>(),
                                          //     Dinastija = d.As<Dinastija>()                                        
                                          // }) 
                                          .Return(v => v.As<VladarNeo>())
                                          .ResultsAsync)
                                          .FirstOrDefault();
        
            //prvo update obicne atribute pa provera za sve ostale

            if(vl == null)
            {
                return BadRequest($"Vladar sa ID: {id} nije pronadjena u bazi!");
            }

            var postojeciVladar = (await _client.Cypher.Match("(l:Licnost:Vladar)")
                                                       .Where("toLower(l.Titula) = toLower($titula) AND toLower(l.Ime) = toLower($ime) AND toLower(l.Prezime) = toLower($prezime) AND l.ID <> $id")
                                                       .WithParam("titula", vladar.Titula)
                                                       .WithParam("ime", vladar.Ime)
                                                       .WithParam("prezime", vladar.Prezime)
                                                       .WithParam("id", id)
                                                       .Return(l => l.As<VladarNeo>())
                                                       .ResultsAsync)
                                                       .FirstOrDefault();


            if (postojeciVladar != null)
                return BadRequest($"Vladar {vladar.Titula} {vladar.Ime} {vladar.Prezime} vec postoji u bazi sa ID: {postojeciVladar.ID}!");


            var query = _client.Cypher.Match("(v:Licnost:Vladar)")
                                      .Where((VladarNeo v) => v.ID == id)
                                      .OptionalMatch("(v)-[r6:PRIPADA_DINASTIJI]->(d:Dinastija)")
                                      .Set("v.Titula = $titula, v.Ime = $ime, v.Prezime = $prezime, v.Pol = $pol, v.MestoRodjenja = $mestoRodjenja")
                                      .WithParam("titula", vladar.Titula)
                                      .WithParam("ime", vladar.Ime)
                                      .WithParam("prezime", vladar.Prezime)
                                      .WithParam("pol", vladar.Pol)
                                      //.WithParam("slika", vladar.Slika)
                                      //.WithParam("tekst", vladar.Tekst)
                                      .WithParam("mestoRodjenja", vladar.MestoRodjenja);
                                     

            if (vladar.GodinaRodjenja != 0)
            {
                if (vl.GodinaRodjenja != 0)
                {
                    if (vl.GodinaRodjenja != vladar.GodinaRodjenja || vl.GodinaRodjenjaPNE != vladar.GodinaRodjenjaPNE)
                    {
                        await _godinaService.DodajGodinu(vladar.GodinaRodjenja, vladar.GodinaRodjenjaPNE);
                        query = query.With("v")
                                     .Match("(v)-[r:RODJEN]->(sg:Godina)")
                                     .Match("(g:Godina {God: $god, IsPNE: $pne})")
                                     .WithParam("god", vladar.GodinaRodjenja)
                                     .WithParam("pne", vladar.GodinaRodjenjaPNE)
                                     .Delete("r")
                                     .Create("(v)-[:RODJEN]->(g)")
                                     .Set("v.GodinaRodjenja = $god, v.GodinaRodjenjaPNE = $pne");
                    }
                }
                else
                {
                    //ne postoji godina samo unosimo novu 
                    await _godinaService.DodajGodinu(vladar.GodinaRodjenja, vladar.GodinaRodjenjaPNE);
                    query = query.With("v")
                                 .Match("(g:Godina {God: $god, IsPNE: $pne})")
                                 .WithParam("god", vladar.GodinaRodjenja)
                                 .WithParam("pne", vladar.GodinaRodjenjaPNE)
                                 .Create("(v)-[:RODJEN]->(g)")
                                 .Set("v.GodinaRodjenja = $god, v.GodinaRodjenjaPNE = $pne");
                }
            }
            else //nije uneta godina brisemo staru
            {
                query = query.With("v")
                               .OptionalMatch("(v)-[r1:RODJEN]->()")
                               .Delete("r1");
            }

            //isto samo za smrt
            if (vladar.GodinaSmrti != 0)//uneta godina
            {
                if (vl.GodinaSmrti != 0)//postoji vec neka godina
                {
                    if (vl.GodinaSmrti != vladar.GodinaSmrti || vl.GodinaSmrtiPNE != vladar.GodinaSmrtiPNE)
                    {//promenjena je 
                        await _godinaService.DodajGodinu(vladar.GodinaSmrti, vladar.GodinaSmrtiPNE);
                        query = query.With("v")
                                     .Match("(v)-[r1:UMRO]->(sgs:Godina)")
                                     .Match("(g2:Godina {God: $gods, IsPNE: $pnes})")
                                     .WithParam("gods", vladar.GodinaSmrti)
                                     .WithParam("pnes", vladar.GodinaSmrtiPNE)
                                     .Delete("r1")
                                     .Create("(v)-[:UMRO]->(g2)")
                                     .Set("v.GodinaSmrti = $gods, v.GodinaSmrtiPNE = $pnes");

                    }
                    //else ista je godina ne radi se nista 
                }
                else
                {
                    //ne postoji godina samo unosimo novu 
                    await _godinaService.DodajGodinu(vladar.GodinaSmrti, vladar.GodinaSmrtiPNE);
                    query = query.With("v")
                                 .Match("(g2:Godina {God: $gods, IsPNE: $pnes})")
                                 .WithParam("gods", vladar.GodinaSmrti)
                                 .WithParam("pnes", vladar.GodinaSmrtiPNE)
                                 .Create("(v)-[:UMRO]->(g2)")
                                 .Set("v.GodinaSmrti = $gods, v.GodinaSmrtiPNE = $pnes");
                }

            }
            else
            {
                query = query.With("v")
                               .OptionalMatch("(v)-[r2:UMRO]->()")
                               .Delete("r2");
            }

            //mesto
            if (!string.IsNullOrWhiteSpace(vladar.MestoRodjenja) && vladar.MestoRodjenja != "string")//uneto mesto 
            {
                var z = (await _client.Cypher.Match("(z:Zemlja)")
                                             .Where("toLower(z.Naziv) = toLower($naziv)")
                                             .WithParam("naziv", vladar.MestoRodjenja)
                                             .Return(z => z.As<ZemljaNeo>())
                                             .ResultsAsync)
                                             .FirstOrDefault();

                if (z != null)//postoji takvo mesto ima smisla da se bilo sta proverava
                {
                    if (!string.IsNullOrWhiteSpace(vl.MestoRodjenja) && vl.MestoRodjenja != "string")//vec postoji nesto u bazi  
                    {
                        //provera je l su ista mesta 
                        if (vl.MestoRodjenja != vladar.MestoRodjenja)//izmenjeno je 
                        {
                            query = query.With("v")
                                         .Match("(z:Zemlja)")
                                         .Where("toLower(z.Naziv) = toLower($naziv)")
                                         .Match("(v)-[r2:RODJEN_U]->(sz:Zemlja)")
                                         .WithParam("naziv", vladar.MestoRodjenja)
                                         .Delete("r2")
                                         .Create("(v)-[:RODJEN_U]->(z)")
                                         .Set("v.MestoRodjenja = $naziv");
                        }
                        //else isto je 
                    }
                    else //nije postojalo mesto u bazi ali je uneto novo 
                        query = query.With("v")
                                     .Match("(z:Zemlja)")
                                     .Where("toLower(z.Naziv) = toLower($naziv)")
                                     .WithParam("naziv", vladar.MestoRodjenja)
                                     .Create("(v)-[:RODJEN_U]->(z)")
                                     .Set("v.MestoRodjenja = $naziv");
                }
                //else to mesto ne postoji kao da nista nije ni uneto                
            }

            if (vladar.PocetakVladavineGod != 0)
            {
                if (vl.PocetakVladavineGod != 0)
                {
                    if (vl.PocetakVladavineGod != vladar.PocetakVladavineGod || vl.PocetakVladavinePNE != vladar.PocetakVladavinePNE)
                    {
                        await _godinaService.DodajGodinu(vladar.PocetakVladavineGod, vladar.PocetakVladavinePNE);
                        query = query.With("v")
                                     .Match("(gp:Godina {God: $poc, IsPNE: $pnep})")
                                     .Match("(v)-[r7:VLADAO_OD]->(gpv:Godina)")
                                     .WithParam("poc", vladar.PocetakVladavineGod)
                                     .WithParam("pnep", vladar.PocetakVladavinePNE)
                                     .Delete("r7")
                                     .Set("v.PocetakVladavineGod = $poc, v.PocetakVladavinePNE = $pnep")
                                     .Create("(v)-[:VLADAO_OD]->(gp)");
                    }
                }
                else
                {
                    await _godinaService.DodajGodinu(vladar.PocetakVladavineGod, vladar.PocetakVladavinePNE);
                    query = query.With("v")
                                 .Match("(gp:Godina {God: $poc, IsPNE: $pnep})")
                                 .WithParam("poc", vladar.PocetakVladavineGod)
                                 .WithParam("pnep", vladar.PocetakVladavinePNE)
                                 .Set("v.PocetakVladavineGod = $poc, v.PocetakVladavinePNE = $pnep")
                                 .Create("(v)-[:VLADAO_OD]->(gp)");
                }
            }
            else
            {
                query = query.With("v")
                               .OptionalMatch("(v)-[r3:VLADAO_OD]->()")
                               .Delete("r3");
            }

            if (vladar.KrajVladavineGod != 0)
            {
                if (vl.KrajVladavineGod != 0)
                {
                    if (vl.KrajVladavineGod != vladar.KrajVladavineGod || vl.KrajVladavinePNE != vladar.KrajVladavinePNE)
                    {
                        await _godinaService.DodajGodinu(vladar.KrajVladavineGod, vladar.KrajVladavinePNE);
                        query = query.With("v")
                                     .Match("(gk:Godina {God: $kraj, IsPNE: $kvpne})")
                                     .Match("(v)-[r8:VLADAO_DO]->(gkv:Godina)")
                                     .WithParam("kraj", vladar.KrajVladavineGod)
                                     .WithParam("kvpne", vladar.KrajVladavinePNE)
                                     .Delete("r8")
                                     .Set("v.KrajVladavineGod = $kraj, v.KrajVladavinePNE = $kvpne")
                                     .Create("(v)-[:VLADAO_DO]->(gk)");
                    }
                }
                else
                {
                    await _godinaService.DodajGodinu(vladar.KrajVladavineGod, vladar.KrajVladavinePNE);
                    query = query.With("v")
                                 .Match("(gk:Godina {God: $kraj, IsPNE: $kvpne})")
                                 .WithParam("kraj", vladar.KrajVladavineGod)
                                 .WithParam("kvpne", vladar.KrajVladavinePNE)
                                 .Set("v.KrajVladavineGod = $kraj, v.KrajVladavinePNE = $kvpne")
                                 .Create("(v)-[:VLADAO_DO]->(gk)");
                }
            }
            else
            {
                query = query.With("v")
                               .OptionalMatch("(v)-[r4:VLADAO_DO]->()")
                               .Delete("r4");
            }

            await query.ExecuteWithoutResultsAsync();

            return Ok($"Licnost sa id: {id} je uspesno promenjena!");
        }
        catch (Exception ex)  
        {
            return StatusCode(500, $"Došlo je do greške pri radu sa Neo4j bazom: {ex.Message}");
        }        
    }

    [HttpDelete("DeleteVladar/{id}")]
    public async Task<IActionResult> DeleteVladar(Guid id)
    {
        try
        {
            var v = (await _client.Cypher.Match("(v:Licnost:Vladar)")
                                         .Where((VladarNeo v) => v.ID == id)
                                         .Return(v => v.As<VladarNeo>())
                                         .ResultsAsync)
                                         .FirstOrDefault();

            if (v == null)
            {
                return BadRequest($"Vladar sa ID: {id} nije pronadjen u bazi!");
            }

            await _client.Cypher
            .Match("(v:Licnost:Vladar)")
            .Where((VladarNeo v) => v.ID == id)
            .DetachDelete("v")  // briše čvor i sve njegove relacije
            .ExecuteWithoutResultsAsync();
            await _vladarCollection.DeleteOneAsync(d => d.ID == id);
            return Ok($"Vladar sa id:{id} uspesno obrisan iz baze!");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Došlo je do greške pri radu sa Neo4j bazom: {ex.Message}");
        }
    }
    
    [HttpGet("GetAllVladare")]
    public async Task<IActionResult> GetAllVladare()
    {
        try
        {
            var vladari = (await _client.Cypher.Match("(v:Licnost:Vladar)")
                                          //   .OptionalMatch("(l)-[r:RODJEN]->(gr:Godina)")
                                          //   .OptionalMatch("(l)-[r2:UMRO]->(gs:Godina)")
                                          //   .OptionalMatch("(l)-[r3:RODJEN_U]->(m:Lokacija)-[:PRIPADA_ZEMLJI]->(z:Zemlja)")
                                          //   .OptionalMatch("(l)-[r4:VLADAO_OD]->(gpv:Godina)")
                                          //   .OptionalMatch("(l)-[r5:VLADAO_DO]->(gkv:Godina)")
                                          .OptionalMatch("(v)-[r6:PRIPADA_DINASTIJI]->(d:Dinastija)")
                                          // .Return((l, gr, gs, m, z, gpv, gkv, d) => new {
                                          //     Vladar = l.As<Vladar>(),
                                          //     Rodjen = gr.As<Godina>(),
                                          //     Umro = gs.As<Godina>(),
                                          //     Mesto =  m.As<Lokacija>(),
                                          //     Zemlja = z.As<Zemlja>(),
                                          //     Pocetak = gpv.As<Godina>(),
                                          //     Kraj = gkv.As<Godina>(),
                                          //     Dinastija = d.As<Dinastija>()                                        
                                          // }) 
                                          .Return((v, d) => new
                                          {
                                              Vladar = v.As<VladarNeo>(),
                                              Dinastija = d.As<DinastijaNeo>()
                                          })
                                          //.Return(vl => vl.As<Vladar>())
                                          .ResultsAsync)
                                          .ToList();


            if (!vladari.Any())
            {
                return BadRequest($"Nijedan vladar nije pronađen u bazi!");
            }

            var ids = vladari.Select(v => v.Vladar.ID).ToList();
            var mongoList = await _vladarCollection.Find(m => ids.Contains(m.ID)).ToListAsync();

            var result = vladari.Select(vl =>
            {
                var mongo = mongoList.FirstOrDefault(m => m.ID == vl.Vladar.ID);
                return new VladarDto
                {
                    ID = vl.Vladar.ID,
                    Titula = vl.Vladar.Titula,
                    Ime = vl.Vladar.Ime,
                    Prezime = vl.Vladar.Prezime,
                    GodinaRodjenja = vl.Vladar.GodinaRodjenja,
                    GodinaRodjenjaPNE = vl.Vladar.GodinaRodjenjaPNE,
                    GodinaSmrti = vl.Vladar.GodinaSmrti,
                    GodinaSmrtiPNE = vl.Vladar.GodinaSmrtiPNE,
                    Pol = vl.Vladar.Pol,
                    //Slika = vl.Vladar.Slika,
                    MestoRodjenja = vl.Vladar.MestoRodjenja,
                    //Tekst = vl.Vladar.Tekst,
                    Dinastija = vl.Dinastija, // ?? new Dinastija() ne moze jer mora da ima naziv da bi kreirao novu zato sad ostavljam ovako 
                    PocetakVladavineGod = vl.Vladar.PocetakVladavineGod,
                    PocetakVladavinePNE = vl.Vladar.PocetakVladavinePNE,
                    KrajVladavineGod = vl.Vladar.KrajVladavineGod,
                    KrajVladavinePNE = vl.Vladar.KrajVladavinePNE,
                    //Clanovi = item.Clanovi?.ToList() ?? new List<Licnost>()  // If no Licnost found, return empty list
                    Tekst = mongo?.Tekst,
                    Slika = mongo?.Slika,
                    Teritorija = mongo?.Teritorija
                };
            });
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Došlo je do greške pri radu sa Neo4j bazom: {ex.Message}");
        }
    }

    // private static LicnostTreeDto ToTreeDto(
    //     LicnostNeo neo,
    //     string? tekst,
    //     string? slika,
    //     List<Guid> roditeljiID)
    //     {
    //         return new LicnostTreeDto
    //         {
    //             ID = neo.ID,
    //             Titula = neo.Titula,
    //             Ime = neo.Ime,
    //             Prezime = neo.Prezime,
    //             GodinaRodjenja = neo.GodinaRodjenja,
    //             GodinaRodjenjaPNE = neo.GodinaRodjenjaPNE,
    //             GodinaSmrti = neo.GodinaSmrti,
    //             GodinaSmrtiPNE = neo.GodinaSmrtiPNE,
    //             Pol = neo.Pol,
    //             MestoRodjenja = neo.MestoRodjenja,
    //             RoditeljiID = roditeljiID,
    //             Tekst = tekst,
    //             Slika = slika,
    //             Deca = new List<LicnostTreeDto>()
    //         };
    //     }


    // [HttpGet("GetVladareByDinastija")]
    // public async Task<IActionResult> GetVladareByDinastija(Guid id)
    // {
    //     try
    //     {
    //         //za svakog vracam njegovu decu i ID-jeve njegovih roditelja
    //         var vladari = (await _client.Cypher.Match("(v:Licnost:Vladar)-[:PRIPADA_DINASTIJI]->(d:Dinastija { ID: $id })")
    //                                       .WithParam("id", id)
    //                                       .OptionalMatch("(v)-[r3:JE_RODITELJ]->(dete:Licnost)")
    //                                       .OptionalMatch("(v)<-[r4:JE_RODITELJ]-(rod:Licnost)")
    //                                       //.OptionalMatch("(v)-[r6:PRIPADA_DINASTIJI]->(d:Dinastija)")
    //                                       .With("v, collect(DISTINCT rod.ID) as roditeljiID, collect(DISTINCT dete.ID) as decaID")
    //                                       .Return((v, decaID, roditeljiID) => new
    //                                       {
    //                                           Vladar = v.As<VladarNeo>(),
    //                                           //Dinastija = d.As<DinastijaNeo>(),
    //                                           DecaID = decaID.As<List<Guid>>(),
    //                                           RoditeljiID = roditeljiID.As<List<Guid>>()
    //                                       })
    //                                       .ResultsAsync)
    //                                       .ToList();


    //         if (!vladari.Any())
    //         {
    //             return BadRequest($"Nijedan vladar nije pronađen u bazi!");
    //         }

    //         var ids = vladari.Select(v => v.Vladar.ID).ToList();
    //         var mongoList = await _vladarCollection.Find(m => ids.Contains(m.ID)).ToListAsync();

    //         var result = vladari.Select(vl =>
    //         {
    //             var mongo = mongoList.FirstOrDefault(m => m.ID == vl.Vladar.ID);
    //             return new LicnostTreeDto
    //             {
    //                 ID = vl.Vladar.ID,
    //                 Titula = vl.Vladar.Titula,
    //                 Ime = vl.Vladar.Ime,
    //                 Prezime = vl.Vladar.Prezime,
    //                 GodinaRodjenja = vl.Vladar.GodinaRodjenja,
    //                 GodinaRodjenjaPNE = vl.Vladar.GodinaRodjenjaPNE,
    //                 GodinaSmrti = vl.Vladar.GodinaSmrti,
    //                 GodinaSmrtiPNE = vl.Vladar.GodinaSmrtiPNE,
    //                 Pol = vl.Vladar.Pol,
    //                 MestoRodjenja = vl.Vladar.MestoRodjenja,
    //                 //Dinastija = vl.Dinastija, // ?? new Dinastija() ne moze jer mora da ima naziv da bi kreirao novu zato sad ostavljam ovako 
    //                 //ali takodje i stoji u modelu da dinastija moze da bude null tkd???
                    
    //                 // PocetakVladavineGod = vl.Vladar.PocetakVladavineGod,
    //                 // PocetakVladavinePNE = vl.Vladar.PocetakVladavinePNE,
    //                 // KrajVladavineGod = vl.Vladar.KrajVladavineGod,
    //                 // KrajVladavinePNE = vl.Vladar.KrajVladavinePNE,

    //                 Tekst = mongo?.Tekst,
    //                 Slika = mongo?.Slika,
    //                 //Teritorija = mongo.Teritorija,
    //                 DecaID = vl.DecaID,
    //                 RoditeljiID = vl.RoditeljiID
    //             };
    //         }).ToList();

    //         //var roots = _treeBuilder.BuildTree(flatList);
    //         return Ok(result);
    //     }
    //     catch (Exception ex)
    //     {
    //         return StatusCode(500, $"Došlo je do greške pri radu sa Neo4j bazom: {ex.Message}");
    //     }
    // }

    // [HttpGet("GetVladareByDinastijaTree")]
    // public async Task<IActionResult> GetVladareByDinastijaTree(Guid id)
    // {
    //     // Step 1: Get all dynasty members (flat list)
    //     var flatList = await GetVladareByDinastija(id); // Reuse your existing query
    //     var flatListDtos = flatList.Select(v => new LicnostTreeDto
    //     {
    //         ID = v.ID,
    //         Titula = v.Titula,
    //         Ime = v.Ime,
    //         Prezime = v.Prezime,
    //         GodinaRodjenja = v.GodinaRodjenja,
    //         GodinaRodjenjaPNE = v.GodinaRodjenjaPNE,
    //         GodinaSmrti = v.GodinaSmrti,
    //         GodinaSmrtiPNE = v.GodinaSmrtiPNE,
    //         Pol = v.Pol,
    //         MestoRodjenja = v.MestoRodjenja,
    //         Slika = v.Slika,
    //         Tekst = v.Tekst,
    //         DecaID = v.DecaID,
    //         RoditeljiID = v.RoditeljiID
    //     }).ToList();

    //     // Step 2: Create the tree builder
    //     var builder = new TreeBuilder(async ids =>
    //     {
    //         // Fetch additional people from DB/Neo4j by ID
    //         // Exclude already fetched ones
    //         var missing = await _client.Cypher
    //             .Match("(p:Licnost)")
    //             .Where("p.ID IN $ids")
    //             .WithParam("ids", ids)
    //             .Return(p => p.As<LicnostTreeDto>())
    //             .ResultsAsync;

    //         return missing.ToList();
    //     });

    //     // Step 3: Build the tree
    //     var roots = builder.BuildTree(flatListDtos);

    //     return Ok(roots);
    // }


}