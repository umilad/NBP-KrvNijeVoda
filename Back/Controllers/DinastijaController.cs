using Microsoft.AspNetCore.Mvc;
//using Neo4j.Driver;
using Neo4jClient;
using System;
using System.Threading.Tasks;
using KrvNijeVoda.Back.Models;
using System.Reflection.Metadata;
using KrvNijeVoda.Back;
using System.Formats.Tar;
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
        try
        {
            //PROVERA JE L VEC POSTOJI????????????
            if(dinastija.PocetakVladavine == null || dinastija.PocetakVladavine.God == 0)
            {
                return BadRequest("Morate uneti godinu pocetka vladavine dinastije!");
            }
            if(dinastija.KrajVladavine == null || dinastija.KrajVladavine.God == 0)
            {
                return BadRequest("Morate uneti godinu kraja vladavine dinastije!");
            }
            if(dinastija.PocetakVladavine.God > dinastija.KrajVladavine.God)//moze jednaka mozda vladali samo te godine 
            {
                //sredi ako dodas p.n.e.
                return BadRequest("Godina pocetka vladavine mora biti manja ili jednaka godini kraja vladavine dinastije!");
            }
            // da li da se dodaju provere sta vraca za svaki slucaj ako ovde puca
            await _godinaService.DodajGodinu(dinastija.PocetakVladavine.God);
            await _godinaService.DodajGodinu(dinastija.KrajVladavine.God);

            await _client.Cypher.Match("(pg:Godina {God: $pocetak})", "(kg:Godina {God: $kraj})")//mora da se doda i veza (d) -[CLANOVI]->(l) i da se obrise takodje
                                .Create("(d:Dinastija {ID: $id, Naziv: $naziv, Slika: $slika}) -[:POCETAK_VLADAVINE]-> (pg), (d) -[:KRAJ_VLADAVINE]-> (kg)")
                                .WithParam("id", Guid.NewGuid())
                                .WithParam("naziv", dinastija.Naziv)
                                .WithParam("slika", dinastija.Slika)
                                .WithParam("pocetak", dinastija.PocetakVladavine.God)
                                .WithParam("kraj", dinastija.KrajVladavine.God)
                                .ExecuteWithoutResultsAsync();
            //mozda ovo znaci da se vezuje samo s tim propertijem i mozda ce morati za ostale da se dodaje al to mi nema smisla
            //kaze chatGPT da se vezuje sa celim cvorom we good
            return Ok($"Dinastija {dinastija.Naziv} je uspesno kreirana!");
        }
        catch (Exception ex)  
        {
            return StatusCode(500, $"Došlo je do greške pri radu sa Neo4j bazom: {ex.Message}");
        }
        
    }

    [HttpGet("GetDinastija/{id}")]
    public async Task<IActionResult> GetDinastija(Guid id)
    {
        try
        {
            var din = (await _client.Cypher.Match("(d:Dinastija)")
                                           .Where((Dinastija d) => d.ID == id)
                                           .OptionalMatch("(d)-[:POCETAK_VLADAVINE]->(pg:Godina)") 
                                           .OptionalMatch("(d)-[:KRAJ_VLADAVINE]->(kg:Godina)")
                                           .Return((d, pg, kg) => new {
                                                Dinastija = d.As<Dinastija>(),
                                                PocetakVladavine = pg.As<Godina>(),
                                                KrajVladavine = kg.As<Godina>()
                                            }) 
                                           .ResultsAsync)
                                           .FirstOrDefault();
            if (din == null)
            {
                return BadRequest($"Nije pronadjena nijedna dinastija sa id: {id}");
            }
            var result = new Dinastija {
                        ID = din.Dinastija.ID,
                        Naziv = din.Dinastija.Naziv,
                        Slika = din.Dinastija.Slika,
                        PocetakVladavine = din.PocetakVladavine ?? new Godina(), //godine ce uvek da nadje ako nisu nullable
                        KrajVladavine = din.KrajVladavine ?? new Godina() 
                        //Clanovi = item.Clanovi?.ToList() ?? new List<Licnost>()  // If no Licnost found, return empty list
                        };

            return Ok(result);
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
                                                 .OptionalMatch("(d)-[:POCETAK_VLADAVINE]->(pg:Godina)") 
                                                 .OptionalMatch("(d)-[:KRAJ_VLADAVINE]->(kg:Godina)")  
                                                 .Return((d, pg, kg) => new {
                                                     Dinastija = d.As<Dinastija>(),
                                                     PocetakVladavine = pg.As<Godina>(),
                                                     KrajVladavine = kg.As<Godina>()
                                                 }) 
                                                 .ResultsAsync)
                                                 .ToList(); 

            if (dinastije == null || !dinastije.Any())
            {
                return BadRequest("Nije pronadjena nijedna dinastija!");
            }

            var result = dinastije.Select(item => new Dinastija {
                                            ID = item.Dinastija.ID,
                                            Naziv = item.Dinastija.Naziv,
                                            Slika = item.Dinastija.Slika,
                                            PocetakVladavine = item.PocetakVladavine ?? new Godina(), //godine ce uvek da nadje ako nisu nullable
                                            KrajVladavine = item.KrajVladavine ?? new Godina() 
                                            //Clanovi = item.Clanovi?.ToList() ?? new List<Licnost>()  // If no Licnost found, return empty list
                                        }).ToList();

            return Ok(result);
        }
        catch (Exception ex)  
        {
            return StatusCode(500, $"Došlo je do greške pri radu sa Neo4j bazom: {ex.Message}");
        }
    }

    [HttpPut("UpdateDinastija/{id}")]
    public async Task<IActionResult> UpdateDinastija([FromBody] Dinastija dinastija, Guid id)
    {
        try
        {
            //ZA LICNOSTI DA BUDE DA SAMO MOZES DA DODAS POSTOJECU LICNOST ILI DA OBRISES NEKU IZ DINASTIJE 
            //A NE DA MOZES DA UBACUJES KOMPLET NOVU 
            
            //zasto mi ne ucita postojece podatke u swagger NE MOZE GOVNO

            if(dinastija.PocetakVladavine == null || dinastija.PocetakVladavine.God == 0)
            {
                return BadRequest("Morate uneti godinu pocetka vladavine dinastije!");
            }
            if(dinastija.KrajVladavine == null || dinastija.KrajVladavine.God == 0)
            {
                return BadRequest("Morate uneti godinu kraja vladavine dinastije!");
            }
            if(dinastija.PocetakVladavine.God > dinastija.KrajVladavine.God)//moze jednaka mozda vladali samo te godine 
            {
                //sredi ako dodas p.n.e.
                return BadRequest("Godina pocetka vladavine mora biti manja ili jednaka godini kraja vladavine dinastije!");
            }

            var din = (await _client.Cypher.Match("(d:Dinastija)")
                                           .Where((Dinastija d) => d.ID == id)
                                           .OptionalMatch("(d)-[r:POCETAK_VLADAVINE]->(pg:Godina)")
                                           .OptionalMatch("(d)-[r2:KRAJ_VLADAVINE]->(kg:Godina)")
                                           .Return((d, pg, kg) => new 
                                           {
                                                Dinastija = d.As<Dinastija>(),//ne mora da je vraca jer se ne koristi nigde a ovo svakako ne sme da bued null
                                                Pocetak = pg.As<Godina>(),
                                                Kraj = kg.As<Godina>()
                                           })
                                           .ResultsAsync)
                                           .FirstOrDefault();
            
            if(din == null || din.Dinastija == null)
            {
                return BadRequest($"Dinastija sa id: {id} ne postoji u bazi!");
            }

            bool promenjenPocetak = false;
            bool promenjenKraj = false;

            if(dinastija.PocetakVladavine != null)
            {
                if(din.Pocetak != null)
                {
                    if(din.Pocetak.God != dinastija.PocetakVladavine.God)
                    {
                        await _godinaService.DodajGodinu(dinastija.PocetakVladavine.God);
                        promenjenPocetak = true;
                    }
                }
                else 
                promenjenPocetak = true;
            }

            if(dinastija.KrajVladavine != null)
            {
                if(din.Kraj != null)
                {
                    if(din.Kraj.God != dinastija.KrajVladavine.God)
                    {
                        await _godinaService.DodajGodinu(dinastija.KrajVladavine.God);
                        promenjenKraj = true;
                    }
                }
                else
                    promenjenKraj = true;
            }

            //da li nam treba da nam fja vrati novu dinastiju
            //var novainastija = ().FirstOrDefault();
            
            //dinastija postoji => update sve proste atribute
            await _client.Cypher.Match("(d:Dinastija)")
                                .Where("d.ID = $id")
                                .Set("d.Naziv = $naziv, d.Slika = $slika")
                                .WithParam("id", id)
                                .WithParam("naziv", dinastija.Naziv)
                                .WithParam("slika", dinastija.Slika)
                                .ExecuteWithoutResultsAsync();
            
            
            if(promenjenPocetak)
            {
                await _client.Cypher.Match("(d:Dinastija)", "(npg:Godina)") //mora da se mecuje i godina da bi bila referenca na cvor inace je samo na objekat
                                    .Where("d.ID = $id AND npg.God = $pocetak")
                                    .OptionalMatch("(d)-[r:POCETAK_VLADAVINE]->(:Godina)")
                                    .Delete("r")
                                    .Create("(d)-[:POCETAK_VLADAVINE]->(npg)")
                                    .WithParam("id", id)
                                    .WithParam("pocetak", dinastija.PocetakVladavine!.God)
                                    .ExecuteWithoutResultsAsync();
            }
            
            if(promenjenKraj)
            {
                await _client.Cypher.Match("(d:Dinastija)", "(nkg:Godina)") //mora da se mecuje i godina da bi bila referenca na cvor inace je samo na objekat
                                    .Where("d.ID = $id AND nkg.God = $kraj")
                                    .OptionalMatch("(d)-[r:KRAJ_VLADAVINE]->(:Godina)")
                                    .Delete("r")
                                    .Create("(d)-[:KRAJ_VLADAVINE]->(nkg)")
                                    .WithParam("id", id)
                                    .WithParam("kraj", dinastija.KrajVladavine!.God)
                                    .ExecuteWithoutResultsAsync();
            }


            return Ok($"Dinastija {dinastija.Naziv} sa id: {id} je uspesno promenjena!");
        }
        catch (Exception ex)  
        {
            return StatusCode(500, $"Došlo je do greške pri radu sa Neo4j bazom: {ex.Message}");
        }
    }
    
    [HttpPut("UpdateDinastijaPocetakVladavine/{id}/{god}")]
    public async Task<IActionResult> UpdateDinastijaPocetakVladavine(Guid id, int god)
    {
        try
        {
            var din = (await _client.Cypher.Match("(d:Dinastija)")
                                           .Where((Dinastija d) => d.ID == id)
                                           .OptionalMatch("(d)-[r:POCETAK_VLADAVINE]->(pg:Godina)")
                                           .Return((d, pg, kg) => new 
                                           {
                                                Dinastija = d.As<Dinastija>(),//ne mora da je vraca jer se ne koristi nigde a ovo svakako ne sme da bued null
                                                Pocetak = pg.As<Godina>(),
                                                Kraj = kg.As<Godina>()
                                           })
                                           .ResultsAsync)
                                           .FirstOrDefault();
            
            if(din == null || din.Dinastija == null)
            {
                return BadRequest($"Dinastija sa id: {id} ne postoji u bazi!");
            }

            if(god == 0)
            {
                return BadRequest("Morate uneti godinu pocetka vladavine dinastije!");
            }
            if(god > din.Kraj.God)//moze jednaka mozda vladali samo te godine 
            {
                //sredi ako dodas p.n.e.
                return BadRequest("Godina pocetka vladavine mora biti manja ili jednaka godini kraja vladavine dinastije!");
            }

            if(din.Pocetak != null && din.Pocetak.God == god)
            {
                return BadRequest($"Pocetak vladavine dinastije {din.Dinastija.Naziv} je vec postavljen na {god}. godinu!");
            }
            await _godinaService.DodajGodinu(god);

            await _client.Cypher.Match("(d:Dinastija)", "(npg:Godina)") //mora da se mecuje i godina da bi bila referenca na cvor inace je samo na objekat
                                .Where("d.ID = $id AND npg.God = $pocetak")
                                .OptionalMatch("(d)-[r:POCETAK_VLADAVINE]->(:Godina)")
                                .Delete("r")
                                .Create("(d)-[:POCETAK_VLADAVINE]->(npg)")
                                .WithParam("id", id)
                                .WithParam("pocetak", god)
                                .ExecuteWithoutResultsAsync();

            return Ok($"Godina pocetka vladavine dinastije {din.Dinastija.Naziv} sa id: {id} je uspesno promenjena!");
        }
        catch (Exception ex)  
        {
            return StatusCode(500, $"Došlo je do greške pri radu sa Neo4j bazom: {ex.Message}");
        }
    }

    [HttpPut("UpdateDinastijaKrajVladavine/{id}/{god}")]
    public async Task<IActionResult> UpdateDinastijaKrajVladavine(Guid id, int god)
    {
        try
        {
            var din = (await _client.Cypher.Match("(d:Dinastija)")
                                           .Where((Dinastija d) => d.ID == id)
                                           .OptionalMatch("(d)-[r:KRAJ_VLADAVINE]->(kg:Godina)")
                                           .Return((d, pg, kg) => new 
                                           {
                                                Dinastija = d.As<Dinastija>(),//ne mora da je vraca jer se ne koristi nigde a ovo svakako ne sme da bued null
                                                Pocetak = pg.As<Godina>(),
                                                Kraj = kg.As<Godina>()
                                           })
                                           .ResultsAsync)
                                           .FirstOrDefault();
            
            if(din == null || din.Dinastija == null)
            {
                return BadRequest($"Dinastija sa id: {id} ne postoji u bazi!");
            }

            if(god == 0)
            {
                return BadRequest("Morate uneti godinu kraja vladavine dinastije!");
            }
            if(god < din.Pocetak.God)//moze jednaka mozda vladali samo te godine 
            {
                //sredi ako dodas p.n.e.
                return BadRequest("Godina kraja vladavine mora biti veca ili jednaka godini pocetka vladavine dinastije!");
            }

            if(din.Kraj != null && din.Kraj.God == god)
            {
                return BadRequest($"Kraj vladavine dinastije {din.Dinastija.Naziv} je vec postavljen na {god}. godinu!");
            }
            await _godinaService.DodajGodinu(god);

            await _client.Cypher.Match("(d:Dinastija)", "(nkg:Godina)") //mora da se mecuje i godina da bi bila referenca na cvor inace je samo na objekat
                                .Where("d.ID = $id AND nkg.God = $kraj")
                                .OptionalMatch("(d)-[r:KRAJ_VLADAVINE]->(:Godina)")
                                .Delete("r")
                                .Create("(d)-[:KRAJ_VLADAVINE]->(nkg)")
                                .WithParam("id", id)
                                .WithParam("kraj", god)
                                .ExecuteWithoutResultsAsync();

            return Ok($"Godina kraja vladavine dinastije {din.Dinastija.Naziv} sa id: {id} je uspesno promenjena!");
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
                                //.OptionalMatch("(d)-[r3:CLANOVI]->(l:Licnost)")//dodaj nadnadno
                                .Delete("r, r2, d")//r3
                                .ExecuteWithoutResultsAsync();

            return Ok($"Dinastija sa id: {id} je uspesno obrisana iz baze!");
        }
        
        catch (Exception ex)  
        {
            return StatusCode(500, $"Došlo je do greške pri radu sa Neo4j bazom: {ex.Message}");
        }
    }

}