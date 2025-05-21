using Microsoft.AspNetCore.Mvc;
//using Neo4j.Driver;
using Neo4jClient;
using System;
using System.Threading.Tasks;
using KrvNijeVoda.Back.Models;
using System.Reflection.Metadata;
using KrvNijeVoda.Back;
using System.Formats.Tar;
using KrvNijeVoda.Back.Helpers;

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

    [HttpPost("CreateDinastija")]
    public async Task<IActionResult> CreateDinastija([FromBody] Dinastija dinastija)
    {
        //provere za prazna polja FRONT
        //provere godPocetka < godKraja i to FRONT 
        //godina da ne sme da bude 0 FRONT 
        //pne na false FRONT??
        try
        {
            var query = _client.Cypher.Create("(d:Dinastija {ID: $id, Naziv: $naziv, Slika: $slika})")
                                      .WithParam("id", Guid.NewGuid())
                                      .WithParam("naziv", dinastija.Naziv)
                                      .WithParam("slika", dinastija.Slika);

            if (dinastija.PocetakVladavineGod != 0)
            {
                await _godinaService.DodajGodinu(dinastija.PocetakVladavineGod, dinastija.PocetakVladavinePNE);
                query = query.With("d")
                             .Match("(pg:Godina {God: $pocetak, IsPNE: $pocetakPNE})")
                             .WithParam("pocetak", dinastija.PocetakVladavineGod)
                             .WithParam("pocetakPNE", dinastija.PocetakVladavinePNE)
                             .Create("(d)-[:POCETAK_VLADAVINE]->(pg)");
            }

            if (dinastija.KrajVladavineGod != 0)
            {
                await _godinaService.DodajGodinu(dinastija.KrajVladavineGod, dinastija.KrajVladavinePNE);
                query = query.With("d")
                             .Match("(kg:Godina {God: $kraj, IsPNE: $krajPNE})")
                             .WithParam("kraj", dinastija.KrajVladavineGod)
                             .WithParam("krajPNE", dinastija.KrajVladavinePNE)
                             .Create("(d)-[:KRAJ_VLADAVINE]->(kg)");
            }

            await query.ExecuteWithoutResultsAsync();

            return Ok($"Dinastija {dinastija.Naziv} je uspesno kreirana!");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Došlo je do greške pri radu sa Neo4j bazom: {ex.Message}");
        }

    }

    //SREDI
    [HttpGet("GetDinastija/{id}")]//NE ODVEZUJE KAKO TREBA DELETE POPRAVI VEZUJE DOBRO
    public async Task<IActionResult> GetDinastija(Guid id)
    {
        try
        {
            var din = (await _client.Cypher.Match("(d:Dinastija)")
                                           .Where((Dinastija d) => d.ID == id)
                                           .Return(d => d.As<Dinastija>())
                                           .ResultsAsync)
                                           .FirstOrDefault();
            if (din == null)
                return NotFound($"Dinastija sa ID: {id} ne postoji u bazi!");

            // var result = new Dinastija
            // {
            //     ID = din.ID,
            //     Naziv = din.Naziv,
            //     Slika = din.Slika,
            //     PocetakVladavine = $"{din.PocetakVladavineGod }"
            //     // PocetakVladavine = new GodinaStruct(din.PocetakVladavineGod, din.PocetakVladavinePNE),
            //     // KrajVladavine = new GodinaStruct(din.KrajVladavineGod, din.KrajVladavinePNE)
            // };

            return Ok(din);//uredjen prikaz kasnije za pne i to 
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Došlo je do greške pri radu sa Neo4j bazom: {ex.Message}");
        }
    }

    [HttpPut("UpdateDinastija/{id}")]
    public async Task<IActionResult> UpdateDinastija([FromBody] Dinastija dinastija, Guid id)
    {
        //samo osnovne propertije da moze da menja, na frontu ce da se sredi logistika za godine
        //napravi da na frontu postoje godine koj emozes da izaberes za kraj nakon sto izaberes pocetak da bi bile vece od pocetka
        try
        {
            var din = (await _client.Cypher.Match("(d:Dinastija)")
                                             .Where((Dinastija d) => d.ID == id)
                                             .Return(d => d.As<Dinastija>())
                                             .ResultsAsync)
                                             .FirstOrDefault();

            if (din == null)
                return BadRequest($"Dinastija sa ID: {id} ne postoji u bazi!");

            //dinastija postoji => update sve proste atribute                         
            var query = _client.Cypher.Match("(d:Dinastija)")
                                      .Where((Dinastija d) => d.ID == id)
                                      .Set("d.Naziv = $naziv, d.Slika = $slika")
                                      .WithParam("naziv", dinastija.Naziv)
                                      .WithParam("slika", dinastija.Slika);

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
            //isto za kraj
            if (dinastija.KrajVladavineGod != 0)//uneta je izmena
            {
                if (din.KrajVladavineGod != 0)//postoji vec neka godina proveri da nisu iste
                {
                    if (din.KrajVladavineGod != dinastija.KrajVladavineGod || din.KrajVladavinePNE != dinastija.KrajVladavinePNE)//ako ga preskoci iste su godine
                    {//uso je godina je promenjena                        
                        query = query.With("d")//brisemo godinu s kojom je bila vezana
                                     .Match("(d)-[r:KRAJ_VLADAVINE]->(pg:Godina)")
                                     .Delete("r");
                        promenjenKraj = true;
                    }
                }
                else //nije bila postavljena godina ali sad je unosimo 
                    promenjenKraj = true;
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
                             .Create("(d)-[:POCETAK_VLADAVINE]->(pg)");
            }

            if (promenjenKraj)
            {
                await _godinaService.DodajGodinu(dinastija.KrajVladavineGod, dinastija.KrajVladavinePNE);
                query = query.With("d")//brisemo godinu s kojom je bila vezana
                             .Match("(kg:Godina {God: $kraj, IsPNE: $krajPNE})") //mora da se mecuje i godina da bi bila referenca na cvor inace je samo na objekat
                             .WithParam("kraj", dinastija.KrajVladavineGod)
                             .WithParam("krajPNE", dinastija.KrajVladavinePNE)
                             .Create("(d)-[:KRAJ_VLADAVINE]->(nkg)");
            }

            await query.ExecuteWithoutResultsAsync();

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
            await _client.Cypher.Match("(d:Dinastija)")
                                .Where((Dinastija d) => d.ID == id)
                                .OptionalMatch("(d)-[r:POCETAK_VLADAVINE]->(pg:Godina)")
                                .OptionalMatch("(d)-[r2:KRAJ_VLADAVINE]->(kg:Godina)")
                                .Delete("r, r2, d")
                                .ExecuteWithoutResultsAsync();

            return Ok($"Dinastija sa ID: {id} je uspesno obrisana iz baze!");
        }
        
        catch (Exception ex)  
        {
            return StatusCode(500, $"Došlo je do greške pri radu sa Neo4j bazom: {ex.Message}");
        }
    }

}