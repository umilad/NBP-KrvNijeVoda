using Microsoft.AspNetCore.Mvc;
using Neo4jClient;
using System;
using System.Threading.Tasks;
using KrvNijeVoda.Back.Models;
using System.Reflection.Metadata;
using KrvNijeVoda.Back;
[Route("api")]
[ApiController]
public class LicnostController : ControllerBase
{
    private readonly IGraphClient _client;

    public LicnostController(Neo4jService neo4jService)
    {
        _client = neo4jService.GetClient();  
    }


    // [HttpPost("CreateLicnost")]
    // public async Task<IActionResult> CreateLicnost([FromBody] Dinastija dinastija)
    // {    
    //     await _client.Cypher.Create("(d:Dinastija $din)")
    //                             .WithParam("din", dinastija)
    //                             .ExecuteWithoutResultsAsync();
    //     return Ok();
    // }

    // [HttpGet("GetLicnost/{id}")]
    // public async Task<IActionResult> GetLicnost(Guid id)
    // {
    //     var din = await _client.Cypher.Match("(d:Dinastija)")
    //                                 .Where((Dinastija d) => d.ID == id)
    //                                 .Return(d => d.As<Dinastija>())
    //                                 .ResultsAsync;
    //     return Ok(din.LastOrDefault());
    // }

    // [HttpPut("UpdateLicnost/{id}")]
    // public async Task<IActionResult> UpdateLicnost([FromBody] Dinastija dinastija, Guid id)
    // {
    //     await _client.Cypher.Match("(d:Dinastija)")
    //                                 .Where((Dinastija d) => d.ID == id)
    //                                 .Set("d = $dinastija")
    //                                 .WithParam("dinastija", dinastija)
    //                                 .ExecuteWithoutResultsAsync();
    //     return Ok();
    // }

    // [HttpDelete("DeleteLicnost/{id}")]
    // public async Task<IActionResult> DeleteLicnost(Guid id)
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
// [Route("api")]
// [ApiController]
// public class LicnostController : ControllerBase
// {
//     private readonly Neo4jService _neo4jService;

//     public LicnostController(Neo4jService neo4jService)
//     {
//         _neo4jService = neo4jService;
//     }


// [HttpPost("DodajLicnost")]
// public async Task<IActionResult> CreateLicnost([FromBody] Licnost licnost)
// {
//     try
//     {
//         using (var session = _neo4jService.GetSession())
//         {
//             // Generate unique GUIDs for each entity
//             var licnostId = Guid.NewGuid().ToString();
//             var godinaRodjenjaId = Guid.NewGuid().ToString();
//             var godinaSmrtiId = Guid.NewGuid().ToString();
//             var mestoRodjenjaId = Guid.NewGuid().ToString();
//             var zemljaId = Guid.NewGuid().ToString();  // For the country (Zemlja)

//             var query = @"
//                 MERGE (gr:Godina {ID: $godinaRodjenjaId, God: $godinaRodjenja})
//                 MERGE (gs:Godina {ID: $godinaSmrtiId, God: $godinaSmrti})
//                 MERGE (m: Mesto {ID: $mestoRodjenjaId})
//                 ON CREATE SET m.Naziv = $mestoRodjenjaNaziv
//                 MERGE (z: Zemlja {ID: $zemljaId})
//                 ON CREATE SET z.Naziv = $zemljaNaziv, z.Trajanje = $zemljaTrajanje, z.Grb = $zemljaGrb
//                 CREATE (l:Licnost {
//                     ID: $licnostId, Titula: $titula, Ime: $ime, Prezime: $prezime, 
//                     Pol: $pol, Slika: $slika
//                 })
//                 MERGE (l)-[:RODJEN_U]->(m)
//                 MERGE (l)-[:RODJEN_GODINE]->(gr)
//                 MERGE (l)-[:UMRO_GODINE]->(gs)
//                 MERGE (m)-[:IS_LOCATED_IN]->(z)
//             ";

//             var parameters = new
//             {
//                 licnostId = licnostId,
//                 titula = licnost.Titula,
//                 ime = licnost.Ime,
//                 prezime = licnost.Prezime,
//                 pol = licnost.Pol,
//                 slika = licnost.Slika,
//                 godinaRodjenjaId = licnost.GodinaRodjenja?.ID.ToString() ?? godinaRodjenjaId,
//                 godinaRodjenja = licnost.GodinaRodjenja?.God,
//                 godinaSmrtiId = licnost.GodinaSmrti?.ID.ToString() ?? godinaSmrtiId,
//                 godinaSmrti = licnost.GodinaSmrti?.God,
//                 mestoRodjenjaId = licnost.MestoRodjenja != null && licnost.MestoRodjenja.ID != Guid.Empty
//                     ? licnost.MestoRodjenja.ID.ToString()
//                     : mestoRodjenjaId,
//                 mestoRodjenjaNaziv = licnost.MestoRodjenja?.Naziv,
//                 zemljaId = zemljaId,
//                 zemljaNaziv = licnost.MestoRodjenja?.PripadaZemlji?.Naziv,
//                 zemljaTrajanje = licnost.MestoRodjenja?.PripadaZemlji?.Trajanje,
//                 zemljaGrb = licnost.MestoRodjenja?.PripadaZemlji?.Grb
//             };

//             await session.RunAsync(query, parameters);
//         }
//         return Ok(licnost);
//     }
//     catch (Exception ex)
//     {
//         return StatusCode(500, $"Internal Server Error: {ex.Message}");
//     }
// }





//    /*[HttpPut("{userId}")]
// public async Task<IActionResult> UpdateUser(Guid userId, [FromBody] User user)
// {
//     using (var session = _neo4jService.GetSession())
//     {
//         var query = $@"
//             MATCH (u:User {{UserId: '{userId}'}})
//             SET u.UserName = $userName, 
//                 u.Email = $email,
//                 u.Ime = $ime,
//                 u.Prezime = $prezime,
//                 u.DatumRodjenja = $datumRodjenja,
//                 u.Pol = $pol,
//                 u.MestoRodjenja = $mestoRodjenja,
//                 u.Slika = $slika
//             RETURN u";

//         var parameters = new
//         {
//             userName = user.UserName,
//             email = user.Email,
//             ime = user.Ime,
//             prezime = user.Prezime,
//             datumRodjenja = user.DatumRodjenja,
//             pol = user.Pol,
//             mestoRodjenja = user.MestoRodjenja,
//             slika = user.Slika
//             // Add other properties as needed
//         };

//         try
//         {
//             var result = await session.RunAsync(query, parameters);

//             // You can handle the result if needed

//             return NoContent();
//         }
//         catch (Exception ex)
//         {
//             // Handle exceptions appropriately
//             return StatusCode(500, $"Internal Server Error: {ex.Message}");
//         }
//     }
// }


//     [HttpDelete("{userId}")]
// public async Task<IActionResult> DeleteUser(Guid userId)
// {
//     try
//     {
//         using (var session = _neo4jService.GetSession())
//         {
//             // Example query: "MATCH (u:User {UserId: $userId}) DETACH DELETE u"
//             var query = "MATCH (u:User {UserId: $userId}) DETACH DELETE u";
            
//             var parameters = new
//             {
//                 userId = userId.ToString()
//             };

//             await session.RunAsync(query, parameters);
//         }

//         return NoContent();
//     }
//     catch (Exception ex)
//     {
//         // Handle exceptions (e.g., log the error)
//         return StatusCode(500, "Internal Server Error");
//     }
// }*/

// }