using Microsoft.AspNetCore.Mvc;
using Neo4jClient;
using System;
using System.Threading.Tasks;
using System.Reflection.Metadata;
using KrvNijeVoda.Back;
using Microsoft.AspNetCore.Authorization;
using MongoDB.Driver;

[Route("api")]
[ApiController]
public class GodinaController : ControllerBase
{
    private readonly IGraphClient _client;
    private readonly IMongoCollection<LicnostMongo> _licnostCollection;
    private readonly IMongoCollection<VladarMongo> _vladarCollection;
    private readonly IMongoCollection<DinastijaMongo> _dinastijaCollection;

    public GodinaController(Neo4jService neo4jService,  MongoService mongoService)
    {
        _client = neo4jService.GetClient();
        _licnostCollection = mongoService.GetCollection<LicnostMongo>("Licnosti");
        _vladarCollection = mongoService.GetCollection<VladarMongo>("Vladari");
        _dinastijaCollection = mongoService.GetCollection<DinastijaMongo>("Dinastije");
    }

    
    [HttpPost("ExportDatabaseAsCypherString")]
    public async Task<IActionResult> ExportDatabaseAsCypherString()
    {
        var result = await _client.Cypher
            .Call("apoc.export.cypher.all(null, {stream:true})")
            .Yield("cypherStatements")
            .Return<string>("cypherStatements")
            .ResultsAsync;

        string cypherScript = string.Join(Environment.NewLine, result);
        return Ok(new { Cypher = cypherScript });
    }

    [HttpPost("CreateGodina")]
    public async Task<IActionResult> CreateGodina([FromBody] GodinaDto godina)
    {
        try
        {
            var god = (await _client.Cypher.Match("(g:Godina)")
                                           .Where((GodinaNeo g) => g.God == godina.God && g.IsPNE == godina.IsPNE)
                                           .Return(g => g.As<GodinaNeo>())
                                           .ResultsAsync)
                                           .FirstOrDefault();

            if (god != null)
            {
                return BadRequest($"Godina {god.God}. vec postoji u bazi!");
            }

            await _client.Cypher.Create("(g:Godina {ID: $id, God: $god, IsPNE: $ispne})")
                                .WithParam("god", godina.God)
                                .WithParam("id", Guid.NewGuid())
                                .WithParam("ispne", godina.IsPNE)
                                .ExecuteWithoutResultsAsync();
            return Ok($"Godina {godina.God}. je uspesno dodata u bazu!");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Došlo je do greške pri radu sa Neo4j bazom: {ex.Message}");
        }
    }
    [Authorize(Roles = "admin")]
    [HttpGet("GetGodina/{id}")]
    public async Task<IActionResult> GetGodina(Guid id)
    {
        try
        {
            var godina = (await _client.Cypher.Match("(g:Godina)")
                                            .Where((GodinaNeo g) => g.ID == id)
                                            .Return(g => g.As<GodinaNeo>())
                                            .ResultsAsync)
                                            .FirstOrDefault();
            if (godina == null)
            {
                return NotFound($"Godina sa id: {id} ne postoji u bazi!");
            }
            return Ok(godina);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Došlo je do greške pri radu sa Neo4j bazom: {ex.Message}");
        }
    }

    [HttpDelete("DeleteGodina/{id}")]
    public async Task<IActionResult> DeleteGodina(Guid id)
    {
        try
        {
            var god = (await _client.Cypher.Match("(g:Godina)")
                                            .Where((GodinaNeo g) => g.ID == id)
                                            .Return(g => g.As<GodinaNeo>())
                                            .ResultsAsync)
                                            .FirstOrDefault();
            if (god == null)
            {
                return NotFound($"Godina sa ID-em {id} ne postoji u bazi!");
            }

            await _client.Cypher.Match("(g:Godina)")
                                .Where((GodinaNeo g) => g.ID == id)
                                .DetachDelete("g") 
                                .ExecuteWithoutResultsAsync();

            return Ok($"Godina sa ID: {id} je uspešno obrisana iz baze!");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Došlo je do greške pri radu sa Neo4j bazom: {ex.Message}");
        }
    }
    [HttpPut("UpdateGodina/{id}")]
    public async Task<IActionResult> UpdateGodina(Guid id, [FromBody] GodinaDto updatedGodina)
    {
        try
        {

            var god = (await _client.Cypher.Match("(g:Godina)")
                                    .Where((GodinaNeo g) => g.ID == id)
                                    .Return(g => g.As<GodinaNeo>())
                                    .ResultsAsync)
                                    .FirstOrDefault();


            if (god == null)
                return NotFound($"Godina sa ID: {id} nije pronađena.");

            var god1 = (await _client.Cypher.Match("(g:Godina)")
                                           .Where((GodinaNeo g) => g.God == updatedGodina.God && g.IsPNE == updatedGodina.IsPNE)
                                           .Return(g => g.As<GodinaNeo>())
                                           .ResultsAsync)
                                           .FirstOrDefault();

            if (god1 != null)
            {
                return BadRequest($"Godina {updatedGodina.God}. vec postoji u bazi!");
            }
            await _client.Cypher.Match("(g:Godina)")
                                .Where((GodinaNeo g) => g.ID == id)
                                .Set("g.God = $godina, g.IsPNE = $ispne")
                                .WithParam("godina", updatedGodina.God)
                                .WithParam("ispne", updatedGodina.IsPNE)
                                .ExecuteWithoutResultsAsync();

            return Ok($"Godina sa ID: {id} uspešno ažurirana.");
        }

        catch (Exception ex)
        {
            return StatusCode(500, $"Došlo je do greške pri radu sa Neo4j bazom: {ex.Message}");
        }
    }

    [HttpGet("GetAllEventsForGodina/{god}")]
        public async Task<IActionResult> GetAllEventsForGodina(int god)
        {
            try
            {
                var usedIds = new HashSet<Guid>();

                var results = new Dictionary<string, object>();

                var dogadjaji = await _client.Cypher
                    .Match("(d:Dogadjaj)-[:DESIO_SE]->(g:Godina)")
                    .Where("NOT (d:Bitka OR d:Rat)")
                    .AndWhere((GodinaNeo g) => g.God == god)
                    .Return(d => d.As<DogadjajNeo>())
                    .ResultsAsync;
                var dogadjajiFiltered = dogadjaji
                    .Where(d => usedIds.Add(d.ID))
                    .ToList();

                if (dogadjajiFiltered.Count != 0)
                    results["dogadjaji"] = dogadjajiFiltered;
                else 
                    results["dogadjaji"] = Enumerable.Empty<DogadjajNeo>();


                var bitke = await _client.Cypher
                    .Match("(d:Dogadjaj:Bitka)-[:DESIO_SE]->(g:Godina)")
                    .Where((GodinaNeo g) => g.God == god)
                    .Return(d => d.As<BitkaNeo>())
                    .ResultsAsync;

            
                var bitkeFiltered = bitke
                    .Where(b => usedIds.Add(b.ID))
                    .ToList();

                if (bitkeFiltered.Count != 0)
                    results["bitke"] = bitkeFiltered;
                else 
                    results["bitke"] = Enumerable.Empty<BitkaNeo>();


                var ratovi = await _client.Cypher
                    .Match("(r:Dogadjaj:Rat)-[rel]->(g:Godina)")
                    .Where("(type(rel)='DESIO_SE' OR type(rel)='RAT_TRAJAO_DO')")
                    .AndWhere((GodinaNeo g) => g.God == god)
                    .Return(r => r.As<RatNeo>())
                    .ResultsAsync;
                var ratoviFiltered = ratovi
                    .Where(r => usedIds.Add(r.ID))
                    .ToList();

                if (ratoviFiltered.Count != 0)
                    results["ratovi"] = ratoviFiltered;
                else 
                    results["ratovi"] = Enumerable.Empty<RatNeo>();

                var vladari = await _client.Cypher
                    .Match("(v:Vladar)-[r:VLADAO_OD|VLADAO_DO|RODJEN|UMRO]->(g:Godina)")
                    .Where((GodinaNeo g) => g.God == god)
                    .Return(v => v.As<VladarNeo>())
                    .ResultsAsync;
                var vladariFiltered = vladari
                    .Where(v => usedIds.Add(v.ID))
                    .ToList();

                if (vladariFiltered.Count != 0)
                {
                    var vladarIds = vladariFiltered.Select(v => v.ID).ToList();
                    var mongoVladari = await _vladarCollection
                        .Find(m => vladarIds.Contains(m.ID))
                        .ToListAsync();

                    var vladariDtos = vladariFiltered.Select(v =>
                    {
                        var mongo = mongoVladari.FirstOrDefault(m => m.ID == v.ID);
                        return new LicnostTimelineDto
                        {
                            ID = v.ID,
                            Ime = v.Ime,
                            Prezime = v.Prezime,
                            Titula = v.Titula,
                            Slika = mongo?.Slika
                        };
                    }).ToList();
                    results["vladari"] = vladariDtos;
                }
                else 
                    results["vladari"] = Enumerable.Empty<VladarNeo>();

                var licnosti = await _client.Cypher
                    .Match("(l:Licnost)-[r:RODJEN|UMRO]->(g:Godina)")
                    .Where("NOT l:Vladar")
                    .AndWhere((GodinaNeo g) => g.God == god)
                    .Return(l => l.As<LicnostNeo>())
                    .ResultsAsync;
                var licnostiFiltered = licnosti
                    .Where(l => usedIds.Add(l.ID))
                    .ToList();

                if (licnostiFiltered.Count != 0)
                {
                    var licnostIds = licnostiFiltered.Select(l => l.ID).ToList();
                    var mongoLicnosti = await _licnostCollection
                        .Find(m => licnostIds.Contains(m.ID))
                        .ToListAsync();

                    var licnostiDtos = licnostiFiltered.Select(l =>
                    {
                        var mongo = mongoLicnosti.FirstOrDefault(m => m.ID == l.ID);
                        return new LicnostTimelineDto
                        {
                            ID = l.ID,
                            Ime = l.Ime,
                            Prezime = l.Prezime,
                            Titula = l.Titula,
                            Slika = mongo?.Slika
                        };
                    }).ToList();
                    results["licnosti"] = licnostiDtos;
                }
                else 
                    results["licnosti"] = Enumerable.Empty<LicnostNeo>();

                var dinastije = await _client.Cypher
                    .Match("(d:Dinastija)-[r:POCETAK_VLADAVINE|KRAJ_VLADAVINE]->(g:Godina)")
                    .Where((GodinaNeo g) => g.God == god)
                    .Return(d => d.As<DinastijaNeo>())
                    .ResultsAsync;
                var dinastijeFiltered = dinastije
                    .Where(d => usedIds.Add(d.ID))
                    .ToList();

                if (dinastijeFiltered.Count != 0)
                {
                    var dinastijaIds = dinastijeFiltered.Select(d => d.ID).ToList();
                    var mongoDinastije = await _dinastijaCollection
                        .Find(m => dinastijaIds.Contains(m.ID))
                        .ToListAsync();

                    var dinastijeDtos = dinastijeFiltered.Select(d =>
                    {
                        var mongo = mongoDinastije.FirstOrDefault(m => m.ID == d.ID);
                        return new DinastijaTimelineDto
                        {
                            ID = d.ID,
                            Naziv = d.Naziv,
                            Slika = mongo?.Slika
                        };
                    }).ToList();
                    results["dinastije"] = dinastijeDtos;
                }
                else 
                    results["dinastije"] = Enumerable.Empty<DinastijaNeo>();

                return Ok(results);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Došlo je do greške pri radu sa bazom: {ex.Message}");
            }
        }
}