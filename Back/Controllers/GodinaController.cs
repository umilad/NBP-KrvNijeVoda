using Microsoft.AspNetCore.Mvc;
using Neo4jClient;
using System;
using System.Threading.Tasks;
//using KrvNijeVoda.Back.Models;
using System.Reflection.Metadata;
using KrvNijeVoda.Back;

[Route("api")]
[ApiController]
public class GodinaController : ControllerBase
{
    private readonly IGraphClient _client;

    public GodinaController(Neo4jService neo4jService)
    {
        _client = neo4jService.GetClient();
    }

    //TEST TEST PROBA PROBA 
    //[Authorize(Roles = "Admin")]
    // [HttpPost("ExportDatabaseCypher")]
    // public async Task<IActionResult> ExportDatabaseCypher()
    // {
    //     await _client.Cypher
    //         .Call("apoc.export.cypher.all($file, $config)")
    //         .WithParams(new {
    //             file = "export.cypher",
    //             config = new {
    //                 format = "cypher"
    //             }
    //         })
    //         .Yield("file")
    //         .Return(file => file.As<string>())
    //         .ResultsAsync;

    //     return Ok("Export completed.");
    // }

    //PROBA 2
    //     [HttpPost("ExportDatabaseCypherString")]
    //     public async Task<string> ExportDatabaseAsCypherString()
    //     {
    //         var result = await _client.Cypher
    //     .Call("apoc.export.cypher.all(null, {stream:true})")
    //     .Yield("cypherStatements")
    //     .Return<string>("cypherStatements")
    //     .ResultsAsync;

    // string cypherScript = string.Join(Environment.NewLine, result);



    //         return cypherScript;
    //     }
    //PROBA 3
    //     [HttpPost("ExportDatabaseCypherString")]
    // public async Task<string> ExportDatabaseAsCypherString()
    // {
    //     var result = await _client.Cypher
    //         .Call("apoc.export.cypher.all(null, {stream:true})")
    //         .Yield("cypherStatements")
    //         .Return<string>("cypherStatements")
    //         .ResultsAsync;

    //     string cypherScript = string.Join(Environment.NewLine, result);
    //     return cypherScript;
    // }

    //PROBA 4 JSON
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


}