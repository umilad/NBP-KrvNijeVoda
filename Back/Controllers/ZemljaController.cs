using Microsoft.AspNetCore.Mvc;
using Neo4jClient;
using MongoDB.Driver;
using System;
using System.Linq;
using System.Threading.Tasks;

[Route("api")]
[ApiController]
public class ZemljaController : ControllerBase
{
    private readonly IGraphClient _neo4jClient;
    private readonly IMongoCollection<ZemljaMongo> _mongo;

    public ZemljaController(Neo4jService neo4jService, MongoService mongoService)
    {
        _neo4jClient = neo4jService.GetClient();
        _mongo = mongoService.GetCollection<ZemljaMongo>("Zemlje");
    }

    [HttpPost("CreateZemlja")]
    public async Task<IActionResult> CreateZemlja([FromBody] ZemljaDto dto)
    {
        try
        {
            
            var exists = (await _neo4jClient.Cypher
                .Match("(z:Zemlja)")
                .Where("toLower(z.Naziv) = toLower($naziv)")
                .WithParam("naziv", dto.Naziv)
                .Return(z => z.As<ZemljaNeo>())
                .ResultsAsync)
                .FirstOrDefault();

            if (exists != null)
                return BadRequest($"Zemlja '{dto.Naziv}' već postoji u bazi!");

            
            var id = Guid.NewGuid();

            await _neo4jClient.Cypher
                .Create("(z:Zemlja {ID: $id, Naziv: $naziv, Trajanje: $trajanje})")
                .WithParam("id", id)
                .WithParam("naziv", dto.Naziv)
                .WithParam("trajanje", dto.Trajanje)
                .ExecuteWithoutResultsAsync();

            var mongo = new ZemljaMongo
            {
                ID = id,
                Grb = dto.Grb,
                BrojStanovnika = dto.BrojStanovnika
            };
            await _mongo.InsertOneAsync(mongo);

            return Ok($"Zemlja '{dto.Naziv}' uspešno dodata!");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Greška: {ex.Message}");
        }
    }

    [HttpGet("GetZemlja/{id}")]
    public async Task<IActionResult> GetZemlja(Guid id)
    {
        try
        {
            var neo = (await _neo4jClient.Cypher
                .Match("(z:Zemlja)")
                .Where((ZemljaNeo z) => z.ID == id)
                .Return(z => z.As<ZemljaNeo>())
                .ResultsAsync)
                .FirstOrDefault();

            if (neo == null)
                return NotFound($"Zemlja sa ID {id} ne postoji u Neo4j bazi!");

            var mongo = await _mongo.Find(m => m.ID == id).FirstOrDefaultAsync();

            var dto = new ZemljaDto
            {
                ID = neo.ID,
                Naziv = neo.Naziv,
                Trajanje = neo.Trajanje,
                Grb = mongo?.Grb,
                BrojStanovnika = mongo?.BrojStanovnika
            };

            return Ok(dto);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Greška: {ex.Message}");
        }
    }

    [HttpGet("GetAllZemlje")]
    public async Task<IActionResult> GetAllZemlje()
    {
        try
        {
            var zemlje = (await _neo4jClient.Cypher
                .Match("(z:Zemlja)")
                .Return(z => z.As<ZemljaNeo>())
                .ResultsAsync)
                .ToList();

            if (!zemlje.Any())
                return NotFound("Nije pronađena nijedna zemlja u bazi!");

            var ids = zemlje.Select(z => z.ID).ToList();
            var mongoList = await _mongo.Find(m => ids.Contains(m.ID)).ToListAsync();

            var result = zemlje.Select(z =>
            {
                var mongo = mongoList.FirstOrDefault(m => m.ID == z.ID);
                return new ZemljaDto
                {
                    ID = z.ID,
                    Naziv = z.Naziv,
                    Trajanje = z.Trajanje,
                    Grb = mongo?.Grb,
                    BrojStanovnika = mongo?.BrojStanovnika
                };
            }).ToList();

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Došlo je do greške: {ex.Message}");
        }
    }



    [HttpPut("UpdateZemlja/{id}")]
    public async Task<IActionResult> UpdateZemlja(Guid id, [FromBody] ZemljaDto dto)
    {
        try
        {
        
            var exists = (await _neo4jClient.Cypher
                .Match("(z:Zemlja)")
                .Where((ZemljaNeo z) => z.ID == id)
                .Return(z => z.As<ZemljaNeo>())
                .ResultsAsync)
                .FirstOrDefault();

            if (exists == null)
                return NotFound($"Zemlja sa ID: {id} ne postoji!");

            
            await _neo4jClient.Cypher
                .Match("(z:Zemlja)")
                .Where((ZemljaNeo z) => z.ID == id)
                .Set("z.Naziv = $naziv, z.Trajanje = $trajanje")
                .WithParam("naziv", dto.Naziv)
                .WithParam("trajanje", dto.Trajanje)
                .ExecuteWithoutResultsAsync();

            
            var update = Builders<ZemljaMongo>.Update
                .Set(m => m.Grb, dto.Grb)
                .Set(m => m.BrojStanovnika, dto.BrojStanovnika);
            await _mongo.UpdateOneAsync(m => m.ID == id, update);

            return Ok($"Zemlja sa ID: {id} je uspešno ažurirana!");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Greška: {ex.Message}");
        }
    }

    [HttpDelete("DeleteZemlja/{id}")]
    public async Task<IActionResult> DeleteZemlja(Guid id)
    {
        try
        {            
            await _neo4jClient.Cypher
                .Match("(z:Zemlja)")
                .Where((ZemljaNeo z) => z.ID == id)
                .DetachDelete("z")
                .ExecuteWithoutResultsAsync();

           
            await _mongo.DeleteOneAsync(m => m.ID == id);

            return Ok($"Zemlja sa ID: {id} je obrisana iz obe baze!");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Greška: {ex.Message}");
        }
    }

    [HttpGet("GetZemljaPoNazivu/{naziv}")]
    public async Task<IActionResult> GetZemljaPoNazivu(string naziv)
    {
        try
        {            
            var neo = (await _neo4jClient.Cypher
                .Match("(z:Zemlja)")
                .Where("toLower(z.Naziv) = toLower($naziv)")
                .WithParam("naziv", naziv)
                .Return(z => z.As<ZemljaNeo>())
                .ResultsAsync)
                .FirstOrDefault();

            if (neo == null)
                return NotFound($"Zemlja sa nazivom {naziv} ne postoji!");

            
            var mongo = await _mongo.Find(m => m.ID == neo.ID).FirstOrDefaultAsync();

            var dto = new ZemljaDto
            {
                ID= neo.ID,
                Naziv = neo.Naziv,
                Trajanje = neo.Trajanje,
                Grb = mongo?.Grb,
                BrojStanovnika = mongo?.BrojStanovnika
            };

            return Ok(dto);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Greška: {ex.Message}");
        }
    }
}
