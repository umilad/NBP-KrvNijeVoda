using Microsoft.AspNetCore.Mvc;
using Neo4jClient;
using System;
using System.Threading.Tasks;
using KrvNijeVoda.Back.Models;
using System.Reflection.Metadata;
using KrvNijeVoda.Back;
using KrvNijeVoda.Models;
[Route("api")]
[ApiController]
public class LicnostController : ControllerBase
{
    private readonly IGraphClient _client;
    private readonly GodinaService _godinaService;
    private readonly LokacijaService _lokacijaService;
    private readonly ZemljaService _zemljaService;

    public LicnostController(Neo4jService neo4jService, GodinaService godinaService, LokacijaService lokacijaService, ZemljaService zemljaService)
    {
        _client = neo4jService.GetClient();
        _godinaService = godinaService;
        _lokacijaService = lokacijaService;
        _zemljaService = zemljaService;
    }


    [HttpPost("CreateLicnost")]
    public async Task<IActionResult> CreateLicnost([FromBody] Licnost licnost)
    {   
        ///TESTIRAJ LEPO!!!!!!!!!
        var licnostID = Guid.NewGuid();
        await _client.Cypher.Create("(l:Licnost {ID: $id, Titula: $titula, Ime: $ime, Prezime: $prezime, Pol: $pol, Slika: $slika})")
                            .WithParam("id", licnostID)
                            .WithParam("titula", licnost.Titula)
                            .WithParam("ime", licnost.Ime)
                            .WithParam("prezime", licnost.Prezime)
                            .WithParam("pol", licnost.Pol)
                            .WithParam("slika", licnost.Slika)
                            .ExecuteWithoutResultsAsync();

        if(licnost.GodinaRodjenja!=null)
        {
            await _godinaService.DodajGodinu(licnost.GodinaRodjenja!.God);
            await _client.Cypher.Match("(l:Licnost {ID: $id})", "(gr:Godina {God: $rodj})")
                                .Create("(l)-[:RODJEN]->(gr)")
                                .WithParam("id", licnostID)
                                .WithParam("rodj", licnost.GodinaRodjenja.God)
                                .ExecuteWithoutResultsAsync();
        }
        if(licnost.GodinaSmrti!=null)
        {
            await _godinaService.DodajGodinu(licnost.GodinaSmrti!.God);
            await _client.Cypher.Match("(l:Licnost {ID: $id})", "(gs:Godina {God: $smrt})")
                                .Create("(l)-[:UMRO]->(gs)")
                                .WithParam("id", licnostID)
                                .WithParam("smrt", licnost.GodinaSmrti.God)
                                .ExecuteWithoutResultsAsync();
        }
        //MENJAJJJJJJJJJJJJJ
        if(licnost.MestoRodjenja != null)
        {
            if(licnost.MestoRodjenja.PripadaZemlji != null)//mora da ima zemlju 
            {
                var nz = await _zemljaService.DodajZemljuParametri(licnost.MestoRodjenja.PripadaZemlji.Naziv, licnost.MestoRodjenja.PripadaZemlji.Grb, licnost.MestoRodjenja.PripadaZemlji.Trajanje);
                var nl = await _lokacijaService.DodajLokaciju(licnost.MestoRodjenja.Naziv, nz);
                await _client.Cypher.Match("(l:Licnost {ID: $id})", "(lm:Lokacija {ID: $mid})")
                                    .Create("(l)-[:RODJEN_U]->(lm)")
                                    .WithParam("id", licnostID)
                                    .WithParam("mid", nl.ID)
                                    .ExecuteWithoutResultsAsync();
            }
            else {
                return Ok($"Uspesno dodata licnost sa id:{licnostID} u bazu, ALI bez mesta jer nije stavljena zemlja kojoj pripada!");
            }
        }

        return Ok($"Uspesno dodata licnost sa id:{licnostID} u bazu!");
    }

    [HttpGet("GetLicnost/{id}")]
    public async Task<IActionResult> GetLicnost(Guid id)
    {
        //NAPRAVI RESULT KAKO DA VRACA 
        var lic = (await _client.Cypher.Match("(l:Licnost)")
                                    .Where((Licnost l) => l.ID == id)
                                    .OptionalMatch("(l)-[r:RODJEN]->(gr:Godina)")
                                    .OptionalMatch("(l)-[r2:UMRO]->(gs:Godina)")
                                    .OptionalMatch("(l)-[r3:RODJEN_U]->(m:Lokacija)-[:PRIPADA_ZEMLJI]->(z:Zemlja)")
                                    .Return((gr, gs, m) => new {
                                        Rodjen = gr.As<Godina>(),
                                        Umro = gs.As<Godina>(),
                                        Mesto =  m.As<Lokacija>().PripadaZemlji

                                        //Zemlja = z.As<Zemlja>()                                        
                                    }) 
                                    .ResultsAsync)
                                    .FirstOrDefault();
        
        if(lic != null)
        {
            //lic.Mesto.PripadaZemlji = lic.Zemlja;
            return Ok(lic);
        }
        else 
            return BadRequest($"Licnost sa id {id} nije pronadjena u bazi!");

        
    }

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

    [HttpDelete("DeleteLicnost/{id}")]
    public async Task<IActionResult> DeleteLicnost(Guid id)
    {
        await _client.Cypher.Match("(l:Licnost)")
                            .Where((Licnost l) => l.ID == id)
                            .OptionalMatch("(l)-[r:RODJEN]->(gr:Godina)")
                            .OptionalMatch("(l)-[r2:UMRO]->(gs:Godina)")
                            .OptionalMatch("(l)-[r3:RODJEN_U]->(m:Lokacija)")
                            .Delete("r, r2, r3, l")
                            .ExecuteWithoutResultsAsync();

        return Ok($"Licnost sa id:{id} uspesno obrisana iz baze!");
    }

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