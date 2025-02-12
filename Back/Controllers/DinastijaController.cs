using Microsoft.AspNetCore.Mvc;
//using Neo4j.Driver;
using Neo4jClient;
using System;
using System.Threading.Tasks;
using KrvNijeVoda.Back.Models;
using System.Reflection.Metadata;
using KrvNijeVoda.Back;
[Route("api")]
[ApiController]
public class DinastijaController : ControllerBase
{
    private readonly IGraphClient _client;
    private readonly GodinaService _godinaService;

    // Constructor: Injecting Neo4jService and getting the client
    public DinastijaController(Neo4jService neo4jService, GodinaService godinaService)
    {
        _client = neo4jService.GetClient();  // Get the Neo4jClient
        _godinaService = godinaService;
    }


// [HttpPost("CreateDinastija")]
// public async Task<IActionResult> CreateDinastija([FromBody] Dinastija dinastija)
// {
//     // await _client.Cypher.Create("(d:Dinastija $din)")
//     //                     .WithParam("din", dinastija)
//     //                     .ExecuteAsync();
    
//     await _client.Cypher.Create("(d:Dinastija $din)")
//                              .WithParam("din", dinastija)
//                              .ExecuteWithoutResultsAsync();
//     return Ok();
// }

    [HttpPost("CreateDinastija")]
    public async Task<IActionResult> CreateDinastija([FromBody] Dinastija dinastija)
    {
        // da li da se dodaju provere sta vraca za svaki slucaj ako ovde puca
        await _godinaService.DodajGodinu(dinastija.PocetakVladavine.God);
        await _godinaService.DodajGodinu(dinastija.KrajVladavine.God);

        await _client.Cypher
                    .Match("(pg:Godina {God: $pocetak})", "(kg:Godina {God: $kraj})")//mora da se doda i veza (d) -[CLANOVI]->(l) i da se obrise takodje
                    .Create("(d:Dinastija {ID: $id, Naziv: $naziv, Slika: $slika}) -[:POCETAK_VLADAVINE]-> (pg), (d) -[:KRAJ_VLADAVINE]-> (kg)")
                    .WithParam("id", Guid.NewGuid())
                    .WithParam("naziv", dinastija.Naziv)
                    .WithParam("slika", dinastija.Slika)
                    .WithParam("pocetak", dinastija.PocetakVladavine.God)
                    .WithParam("kraj", dinastija.KrajVladavine.God)
                    .ExecuteWithoutResultsAsync();
        //mozda ovo znaci da se vezuje samo s tim propertijem i mozda ce morati za ostale da se dodaje al to mi nema smisla
        //kaze chatGPT da se vezuje sa celim cvorom we good
        return Ok();
    }


// [HttpPost("CreateDinastija")]
// public async Task<IActionResult> CreateDinastija([FromBody] Dinastija dinastija)
// {
//     var pocetakGodina = (await _client.Cypher
//                                       .Match("(g:Godina)")
//                                       .Where((Godina g) => g.God == dinastija.PocetakVladavine.God) 
//                                       .Return(g => g.As<Godina>())
//                                       .ResultsAsync)
//                                       .FirstOrDefault();

//     pocetakGodina ??= await _godinaService.DodajGodinu(dinastija.PocetakVladavine.God);
   
//     var krajGodina = (await _client.Cypher
//                                   .Match("(g:Godina)")
//                                   .Where((Godina g) => g.God == dinastija.KrajVladavine.God) 
//                                   .Return(g => g.As<Godina>())
//                                   .ResultsAsync)
//                                   .FirstOrDefault();


//     if (krajGodina == null)
//         krajGodina = await _godinaService.DodajGodinu(dinastija.KrajVladavine.God);

//     await _client.Cypher
//                  .Match("(pg:Godina {God: $pocetak})", "(kg:Godina {God: $kraj})")
//                  .Create("(d:Dinastija {ID: $id, Naziv: $naziv, Slika: $slika}) -[:HAS_POCETAK_VLADAVINE]-> (pg), (d) -[:HAS_KRAJ_VLADAVINE]-> (kg)")
//                  .WithParam("id", Guid.NewGuid())
//                  .WithParam("naziv", dinastija.Naziv)
//                  .WithParam("slika", dinastija.Slika)
//                  .WithParam("pocetak", dinastija.PocetakVladavine.God)
//                  .WithParam("kraj", dinastija.KrajVladavine.God)
//                  .ExecuteWithoutResultsAsync();

//     return Ok();
// }
 //Godina pocetakGodinaToUse;
    // if (pocetakGodina != null)
    // {
    //     pocetakGodinaToUse = pocetakGodina;
    // }
    // else
    // {
    //     pocetakGodinaToUse = await _godinaService.DodajGodinu(dinastija.PocetakVladavine.God);  
    // }
    //Godina krajGodinaToUse;

    // if (krajGodina.Any())
    // {
    //     // If the end year 'Godina' exists, use the existing one
    //     krajGodinaToUse = krajGodina.First();
    // }
    // else
    // {
    //     // If the end year 'Godina' does not exist, create it
    //     krajGodinaToUse = await _godinaService.DodajGodinu(dinastija.KrajVladavine.God); 
    // }

[HttpGet("GetDinastija/{id}")]
public async Task<IActionResult> GetDinastija(Guid id)
{
    var din = await _client.Cypher.Match("(d:Dinastija)")
                                  .Where((Dinastija d) => d.ID == id)
                                  .Return(d => d.As<Dinastija>())
                                  .ResultsAsync;
    return Ok(din.LastOrDefault());
}

[HttpPut("UpdateDinastija/{id}")]
public async Task<IActionResult> UpdateDinastija([FromBody] Dinastija dinastija, Guid id)
{
    //provere za godine ako si oste nista ide ovako
    //ako nisu mora da  odveze godinu i da veze sa drugom
    await _godinaService.DodajGodinu(dinastija.PocetakVladavine.God);
    await _godinaService.DodajGodinu(dinastija.KrajVladavine.God);

    await _client.Cypher.Match("(d:Dinastija)")
                                  .Where((Dinastija d) => d.ID == id)
                                  .Set("d = $dinastija")
                                  .WithParam("dinastija", dinastija)
                                  .ExecuteWithoutResultsAsync();
    return Ok();
}

    [HttpDelete("DeleteDinastija/{id}")]
    public async Task<IActionResult> DeleteDinastija(Guid id)
    {
        await _client.Cypher.Match("(d:Dinastija)")
                            .Where((Dinastija d) => d.ID == id)
                            .OptionalMatch("(d)-[r:POCETAK_VLADAVINE]->(pg:Godina)")
                            .OptionalMatch("(d)-[r2:KRAJ_VLADAVINE]->(kg:Godina)")
                            //.OptionalMatch("(d)-[r3:CLANOVI]->(l:Licnost)")//dodaj nadnadno
                            .Delete("r, r2, d")//r3
                            .ExecuteWithoutResultsAsync();
        return Ok();
    }

}