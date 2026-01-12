using Microsoft.AspNetCore.Mvc;
//using Neo4j.Driver;
using Neo4jClient;
using System;
using System.Threading.Tasks;
//using KrvNijeVoda.Back.Models;
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
    public async Task<IActionResult> CreateDinastija([FromBody] DinastijaDto dinastija)
    {
        //provere za prazna polja FRONT
        //provere godPocetka < godKraja i to FRONT 
        //godina da ne sme da bude 0 FRONT 
        //pne na false FRONT??
        try
        {
            var din = (await _client.Cypher.Match("(d:Dinastija)")
                                           .Where("toLower(d.Naziv) = toLower($naziv)")
                                           .WithParam("naziv", dinastija.Naziv)
                                           .Return(d => d.As<DinastijaNeo>())
                                           .ResultsAsync)
                                           .FirstOrDefault();

            if (din != null)
                return BadRequest($"Dinastija sa imenom {dinastija.Naziv} vec postoji u bazi!");

            var dinID = Guid.NewGuid();
            var query = _client.Cypher.Create("(d:Dinastija {ID: $id, Naziv: $naziv})")
                                      .WithParam("id", dinID)
                                      .WithParam("naziv", dinastija.Naziv);

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

            if (!string.IsNullOrWhiteSpace(dinastija.Slika))
            {
                var dinastijaMongo = new DinastijaMongo
                {
                    ID =  dinID,
                    Slika = dinastija.Slika
                };
                await _dinastijaCollection.InsertOneAsync(dinastijaMongo);
            }

            return Ok($"Dinastija {dinastija.Naziv} je uspesno kreirana!");
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
    public async Task<IActionResult> UpdateDinastija([FromBody] DinastijaDto dinastija, Guid id)
    {
        //samo osnovne propertije da moze da menja, na frontu ce da se sredi logistika za godine
        //napravi da na frontu postoje godine koj emozes da izaberes za kraj nakon sto izaberes pocetak da bi bile vece od pocetka
        try
        {
            var din = (await _client.Cypher.Match("(d:Dinastija)")
                                           .Where((DinastijaNeo d) => d.ID == id)
                                           .Return(d => d.As<DinastijaNeo>())
                                           .ResultsAsync)
                                           .FirstOrDefault();

            if (din == null)
                return BadRequest($"Dinastija sa ID: {id} ne postoji u bazi!");

             //ako naziv ostaje isti to je ta ista 
            var duplikat = (await _client.Cypher
                .Match("(d:Dinastija)")
                .Where("toLower(d.Naziv) = toLower($naziv) AND d.ID <> $id")
                .WithParam("naziv", dinastija.Naziv)
                .WithParam("id", id)
                .Return(d => d.As<DinastijaNeo>())
                .ResultsAsync)
                .Any();

            if (duplikat)
            {
                return BadRequest($"Dinastija sa nazivom '{dinastija.Naziv}' već postoji u bazi!");
            }

            //dinastija postoji => update sve proste atribute                         
            var query = _client.Cypher.Match("(d:Dinastija)")
                                      .Where((DinastijaNeo d) => d.ID == id)
                                      .Set("d.Naziv = $naziv")
                                      .WithParam("naziv", dinastija.Naziv);
                                      

            bool promenjenPocetak = false;
            bool promenjenKraj = false;

            if (dinastija.PocetakVladavineGod != 0)//uneta je izmena
            {
                if (din.PocetakVladavineGod != 0)//postoji vec neka godina proveri da nisu iste
                {
                    if (din.PocetakVladavineGod != dinastija.PocetakVladavineGod || din.PocetakVladavinePNE != din.PocetakVladavinePNE)//ako ga preskoci iste su godine
                    {//uso je godina je promenjena
                        query = query.With("d")//brisemo godinu s kojom je bila vezana
                                     .Match("(d)-[r:POCETAK_VLADAVINE]->(pg:Godina)")
                                     .Delete("r");
                        promenjenPocetak = true;//treba da se doda veza
                    }
                    //esle iste su nema promene
                }
                else //nije bila postavljena godina ali sad je unosimo 
                    promenjenPocetak = true; //treba da se doda veza
            }
            else //ostavljeno je prazno polje brisemo godinu 
            {
                query = query.With("d")
                             .OptionalMatch("(d)-[r:POCETAK_VLADAVINE]->()")
                             .Delete("r");
            }
            //isto za kraj
            if (dinastija.KrajVladavineGod != 0)//uneta je izmena
            {
                if (din.KrajVladavineGod != 0)//postoji vec neka godina proveri da nisu iste
                {
                    if (din.KrajVladavineGod != dinastija.KrajVladavineGod || din.KrajVladavinePNE != dinastija.KrajVladavinePNE)//ako ga preskoci iste su godine
                    {//uso je godina je promenjena                        
                        query = query.With("d")//brisemo godinu s kojom je bila vezana
                                     .Match("(d)-[r1:KRAJ_VLADAVINE]->(pg:Godina)")
                                     .Delete("r1");
                        promenjenKraj = true;
                    }
                }
                else //nije bila postavljena godina ali sad je unosimo 
                    promenjenKraj = true;
            }
            else //ostavljeno je prazno polje brisemo godinu 
            {
                query = query.With("d")
                             .OptionalMatch("(d)-[r1:KRAJ_VLADAVINE]->()")
                             .Delete("r1");
            }

            //da li nam treba da nam fja vrati novu dinastiju
            //var novainastija = ().FirstOrDefault();           

            if (promenjenPocetak)
            {
                await _godinaService.DodajGodinu(dinastija.PocetakVladavineGod, dinastija.PocetakVladavinePNE);
                query = query.With("d")//brisemo godinu s kojom je bila vezana
                             .Match("(pg:Godina {God: $pocetak, IsPNE: $pocetakPNE})")
                             .WithParam("pocetak", dinastija.PocetakVladavineGod)
                             .WithParam("pocetakPNE", dinastija.PocetakVladavinePNE)
                             .Set("d.PocetakVladavineGod = $pocetak, d.PocetakVladavinePNE = $pocetakPNE")
                             .Create("(d)-[:POCETAK_VLADAVINE]->(pg)");
            }

            if (promenjenKraj)
            {
                await _godinaService.DodajGodinu(dinastija.KrajVladavineGod, dinastija.KrajVladavinePNE);
                query = query.With("d")//brisemo godinu s kojom je bila vezana
                             .Match("(kg:Godina {God: $kraj, IsPNE: $krajPNE})") //mora da se mecuje i godina da bi bila referenca na cvor inace je samo na objekat
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
                var result = await _dinastijaCollection.UpdateOneAsync(filter, update);

            }

            return Ok($"Dinastija {dinastija.Naziv} je uspesno azurirana!");
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

        //groupby i select za svaki slucaj ako ima duplikata
        var flatList = vladari
            .Concat(licnosti)
            .GroupBy(p => p.ID)
            .Select(g => g.First())
            .ToList();

        var trees = _treeBuilder.BuildTrees(flatList);
        return Ok(trees);
    }

}