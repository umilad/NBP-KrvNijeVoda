using Microsoft.AspNetCore.Mvc;
using Neo4jClient;
using System;
using System.Threading.Tasks;
using KrvNijeVoda.Back.Models;
using System.Reflection.Metadata;
using KrvNijeVoda.Back;
[Route("api")]
[ApiController]
public class MestoController : ControllerBase
{
    private readonly IGraphClient _client;

    public MestoController(Neo4jService neo4jService)
    {
        _client = neo4jService.GetClient();  
    }


    // [HttpPost("CreateMesto")]
    // public async Task<IActionResult> CreateMesto([FromBody] Dinastija dinastija)
    // {    
    //     await _client.Cypher.Create("(d:Dinastija $din)")
    //                             .WithParam("din", dinastija)
    //                             .ExecuteWithoutResultsAsync();
    //     return Ok();
    // }

    // [HttpGet("GetMesto/{id}")]
    // public async Task<IActionResult> GetMesto(Guid id)
    // {
    //     var din = await _client.Cypher.Match("(d:Dinastija)")
    //                                 .Where((Dinastija d) => d.ID == id)
    //                                 .Return(d => d.As<Dinastija>())
    //                                 .ResultsAsync;
    //     return Ok(din.LastOrDefault());
    // }

    // [HttpPut("UpdateMesto/{id}")]
    // public async Task<IActionResult> UpdateMesto([FromBody] Dinastija dinastija, Guid id)
    // {
    //     await _client.Cypher.Match("(d:Dinastija)")
    //                                 .Where((Dinastija d) => d.ID == id)
    //                                 .Set("d = $dinastija")
    //                                 .WithParam("dinastija", dinastija)
    //                                 .ExecuteWithoutResultsAsync();
    //     return Ok();
    // }

    // [HttpDelete("DeleteMesto/{id}")]
    // public async Task<IActionResult> DeleteMesto(Guid id)
    // {
    //     await _client.Cypher.Match("(d:Dinastija)")
    //                         .Where((Dinastija d) => d.ID == id)
    //                         .Delete("d")
    //                         .ExecuteWithoutResultsAsync();
    //     return Ok();
    // }

}


// using Microsoft.AspNetCore.Mvc;
// using Neo4j.Driver;
// using System;
// using System.Threading.Tasks;
// using KrvNijeVoda.Models;
// using System.Reflection.Metadata;
// using KrvNijeVoda.Back.Models;
// [Route("api")]
// [ApiController]
// public class MestoController : ControllerBase
// {
//     private readonly Neo4jService _neo4jService;

//     public MestoController(Neo4jService neo4jService)
//     {
//         _neo4jService = neo4jService;
//     }

// [HttpPost("DodajMesto")]
// public async Task<IActionResult> CreateMesto([FromBody] Mesto mesto)
// {
//     try
//     {
//         using (var session = _neo4jService.GetSession())
//         {
//             var query = @"
//                 // Merge Lokacija node, using the shared ID
//                 MERGE (l:Lokacija {ID: $lokacijaId})
//                 ON CREATE SET l.Naziv = $lokacijaNaziv
//                 ON MATCH SET l.Naziv = coalesce(l.Naziv, $lokacijaNaziv)

//                 // Merge the Mesto node as an extension of Lokacija
//                 MERGE (m:Mesto {ID: $mestoId})
//                 ON CREATE SET m.Naziv = $mestoNaziv
//                 MERGE (m)-[:IS_LOCATED_IN]->(l)

//                 // Ensure Zemlja node exists and relate Mesto to Zemlja
//                 MERGE (z:Zemlja {ID: $zemljaId})
//                 ON CREATE SET z.Naziv = $zemljaNaziv, z.Trajanje = $zemljaTrajanje, z.Grb = $zemljaGrb
//                 MERGE (m)-[:BELONGS_TO]->(z)
//             ";

//             var parameters = new
//             {
//                 lokacijaId = mesto.ID.ToString(), // Shared ID for both Lokacija and Mesto
//                 lokacijaNaziv = mesto.Naziv,
//                 mestoId = mesto.ID.ToString(), // Same ID for Mesto as for Lokacija
//                 mestoNaziv = mesto.Naziv,
//                 zemljaNaziv = mesto.PripadaZemlji.Naziv,
//                 zemljaId = mesto.PripadaZemlji.ID.ToString(),
//                 zemljaTrajanje = mesto.PripadaZemlji.Trajanje,
//                 zemljaGrb = mesto.PripadaZemlji.Grb
//             };

//             await session.RunAsync(query, parameters);
//         }
//         return Ok(mesto);
//     }
//     catch (Exception ex)
//     {
//         return StatusCode(500, $"Internal Server Error: {ex.Message}");
//     }
// }

// [HttpGet("GetMesto/{id}")]
// public async Task<IActionResult> GetMesto(Guid id)
// {
//     try
//     {
//         using (var session = _neo4jService.GetSession())
//         {
//             var query = @"
//                 MATCH (m:Mesto {ID: $id})
//                 OPTIONAL MATCH (m)-[:IS_LOCATED_IN]->(l:Lokacija)
//                 OPTIONAL MATCH (m)-[:BELONGS_TO]->(z:Zemlja)
//                 RETURN m, l, z
//             ";

//             var parameters = new { id = id.ToString() };
//             var result = await session.RunAsync(query, parameters);
//             var record = await result.SingleAsync();

//             if (record != null)
//             {
//                 var mesto = record["m"].As<INode>().Properties;
                
//                 // Check if 'l' (Lokacija) exists
//                 var lokacija = record.Keys.Contains("l") ? record["l"].As<INode>().Properties : null;

//                 // Check if 'z' (Zemlja) exists
//                 var zemlja = record.Keys.Contains("z") ? record["z"].As<INode>().Properties : null;

//                 return Ok(new { Mesto = mesto, Lokacija = lokacija, Zemlja = zemlja });
//             }
//             else
//             {
//                 return NotFound("Mesto not found.");
//             }
//         }
//     }
//     catch (Exception ex)
//     {
//         return StatusCode(500, $"Internal Server Error: {ex.Message}");
//     }
// }



// [HttpDelete("DeleteMesto/{id}")]
// public async Task<IActionResult> DeleteMesto(Guid id)
// {
//     try
//     {
//         using (var session = _neo4jService.GetSession())
//         {
//             // First, delete all relationships connected to the Mesto node
//             var deleteRelationshipsQuery = @"
//                 MATCH (m:Mesto {ID: $id})
//                 OPTIONAL MATCH (m)-[r]-()
//                 DELETE r
//             ";

//             var parameters = new { id = id.ToString() };
//             await session.RunAsync(deleteRelationshipsQuery, parameters);

//             // Now, delete the Mesto node itself
//             var deleteMestoQuery = @"
//                 MATCH (m:Mesto {ID: $id})
//                 DELETE m
//             ";

//             await session.RunAsync(deleteMestoQuery, parameters);

//             return Ok("Mesto deleted successfully.");
//         }
//     }
//     catch (Exception ex)
//     {
//         return StatusCode(500, $"Internal Server Error: {ex.Message}");
//     }
// }


// [HttpPut("UpdateMesto/{id}")]
// public async Task<IActionResult> UpdateMesto(Guid id, [FromBody] Mesto updatedMesto)
// {
//     try
//     {
//         using (var session = _neo4jService.GetSession())
//         {
//             var query = @"
//                 MATCH (m:Mesto {ID: $id})
//                 SET m.Naziv = COALESCE($naziv, m.Naziv)

//                 // Pass 'm' forward to the next part of the query
//                 WITH m

//                 // Match or merge the related Lokacija node
//                 OPTIONAL MATCH (m)-[:IS_LOCATED_IN]->(l:Lokacija)
//                 MERGE (newLokacija:Lokacija {ID: $lokacijaId})
//                 ON CREATE SET newLokacija.Naziv = $lokacijaNaziv
//                 ON MATCH SET newLokacija.Naziv = COALESCE($lokacijaNaziv, newLokacija.Naziv)

//                 // Merge the relationship between Mesto and Lokacija
//                 MERGE (m)-[:IS_LOCATED_IN]->(newLokacija)

//                 // Match or merge the related Zemlja node
//                 OPTIONAL MATCH (m)-[:BELONGS_TO]->(z:Zemlja)
//                 MERGE (newZemlja:Zemlja {ID: $zemljaId})
//                 ON CREATE SET newZemlja.Naziv = $zemljaNaziv, newZemlja.Trajanje = $zemljaTrajanje, newZemlja.Grb = $zemljaGrb
//                 ON MATCH SET newZemlja.Naziv = COALESCE($zemljaNaziv, newZemlja.Naziv),
//                              newZemlja.Trajanje = COALESCE($zemljaTrajanje, newZemlja.Trajanje),
//                              newZemlja.Grb = COALESCE($zemljaGrb, newZemlja.Grb)

//                 // Merge the relationship between Mesto and Zemlja
//                 MERGE (m)-[:BELONGS_TO]->(newZemlja)
//             ";

//             var parameters = new
//             {
//                 id = id.ToString(),
//                 naziv = updatedMesto.Naziv,
//                 lokacijaId = updatedMesto.ID.ToString(),
//                 lokacijaNaziv = updatedMesto.Naziv,
//                 zemljaId = updatedMesto.PripadaZemlji.ID.ToString(),
//                 zemljaNaziv = updatedMesto.PripadaZemlji.Naziv,
//                 zemljaTrajanje = updatedMesto.PripadaZemlji.Trajanje,
//                 zemljaGrb = updatedMesto.PripadaZemlji.Grb
//             };

//             var result = await session.RunAsync(query, parameters);

//             return Ok("Mesto updated successfully.");
//         }
//     }
//     catch (Exception ex)
//     {
//         return StatusCode(500, $"Internal Server Error: {ex.Message}");
//     }
// }



// }