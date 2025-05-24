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
    // private readonly LokacijaService _lokacijaService;
    // private readonly ZemljaService _zemljaService;

    public LicnostController(Neo4jService neo4jService, GodinaService godinaService/*, LokacijaService lokacijaService, ZemljaService zemljaService*/)
    {
        _client = neo4jService.GetClient();
        _godinaService = godinaService;
        // _lokacijaService = lokacijaService;
        // _zemljaService = zemljaService;
    }

    [HttpPost("CreateLicnost")]
    public async Task<IActionResult> CreateLicnost([FromBody] Licnost licnost)
    {
        try
        {
            var postojecaLicnost = (await _client.Cypher.Match("(l:Licnost)")
                                                        .Where("l.Titula = $titula AND l.Ime = $ime AND l.Prezime = $prezime")
                                                        .WithParam("titula", licnost.Titula)
                                                        .WithParam("ime", licnost.Ime)
                                                        .WithParam("prezime", licnost.Prezime)
                                                        .Return(l => l.As<Licnost>())
                                                        .ResultsAsync)
                                                        .FirstOrDefault();

            if (postojecaLicnost != null)
                return BadRequest($"Licnost {licnost.Titula} {licnost.Ime} {licnost.Prezime} vec postoji u bazi sa ID: {postojecaLicnost.ID}!");


            var licnostID = Guid.NewGuid();
            var query = _client.Cypher.Create("(l:Licnost {ID: $id, Titula: $titula, Ime: $ime, Prezime: $prezime, Pol: $pol, Slika: $slika, MestoRodjenja: $mestoRodjenja, Tekst: $tekst})")
                                .WithParam("id", licnostID)
                                .WithParam("titula", licnost.Titula)
                                .WithParam("ime", licnost.Ime)
                                .WithParam("prezime", licnost.Prezime)
                                .WithParam("pol", licnost.Pol)
                                .WithParam("slika", licnost.Slika)
                                .WithParam("mestoRodjenja", licnost.MestoRodjenja)
                                .WithParam("tekst", licnost.Tekst); 

            if (licnost.GodinaRodjenja != 0)
            {
                await _godinaService.DodajGodinu(licnost.GodinaRodjenja, licnost.GodinaRodjenjaPNE);
                query = query.With("l")
                             .Match("(gr:Godina {God: $rodj, IsPNE: $pne})")
                             .WithParam("rodj", licnost.GodinaRodjenja)
                             .WithParam("pne", licnost.GodinaRodjenjaPNE)
                             .Set("l.GodinaRodjenja = $rodj, l.GodinaRodjenjaPNE = $pne")
                             .Create("(l)-[:RODJEN]->(gr)");
            }
            if (licnost.GodinaSmrti != 0)
            {
                await _godinaService.DodajGodinu(licnost.GodinaSmrti, licnost.GodinaSmrtiPNE);
                query = query.With("l")
                             .Match("(gs:Godina {God: $smrt, IsPNE: $pnes})")
                             .WithParam("smrt", licnost.GodinaSmrti)
                             .WithParam("pnes", licnost.GodinaSmrtiPNE)
                             .Set("l.GodinaSmrti = $smrt, l.GodinaSmrtiPNE = $pnes")
                             .Create("(l)-[:UMRO]->(gs)");
            }

            if (!string.IsNullOrWhiteSpace(licnost.MestoRodjenja) && licnost.MestoRodjenja != "string")
            {
                var z = (await _client.Cypher.Match("(z:Zemlja {Naziv: $naziv})")
                          .WithParam("naziv", licnost.MestoRodjenja)
                          .Return(z => z.As<Zemlja>())
                          .ResultsAsync)
                          .FirstOrDefault();

                if (z != null)
                    query = query.With("l")
                                 .Match("(z:Zemlja {Naziv: $naziv})")
                                 .WithParam("naziv", licnost.MestoRodjenja)
                                 .Create("(l)-[:RODJEN_U]->(z)")
                                 .Set("l.MestoRodjenja = $naziv");
                else
                    query = query.With("l")
                                 .Set("l.MestoRodjenja = $mr")
                                 .WithParam("mr", "/");
            }

            await query.ExecuteWithoutResultsAsync();

            return Ok($"Uspesno dodata licnost sa ID:{licnostID} u bazu!");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Došlo je do greške pri radu sa Neo4j bazom: {ex.Message}");
        }
    }

    [HttpGet("GetLicnost/{id}")]
    public async Task<IActionResult> GetLicnost(Guid id)
    {
        try
        {
            var lic = (await _client.Cypher.Match("(l:Licnost)")
                                           .Where((Licnost l) => l.ID == id)
                                           //.OptionalMatch("(l)-[r:RODJEN]->(gr:Godina)")
                                           //.OptionalMatch("(l)-[r2:UMRO]->(gs:Godina)")
                                           //.OptionalMatch("(l)-[r3:RODJEN_U]->(m:Lokacija)-[:PRIPADA_ZEMLJI]->(z:Zemlja)")
                                           //    .Return((l, gr, gs, m, z) => new {
                                           //        Licnost = l.As<Licnost>(),
                                           //        Rodjen = gr.As<Godina>(),
                                           //        Umro = gs.As<Godina>(),
                                           //        Mesto = m.As<Lokacija>(),
                                           //        Zemlja = z.As<Zemlja>()
                                           //    })
                                           .Return(l => l.As<Licnost>())
                                           .ResultsAsync)
                                           .FirstOrDefault();

            if (lic == null)
            {
                return BadRequest($"Licnost sa id {id} nije pronadjena u bazi!");
            }

            // if (lic.Mesto != null)
            //     lic.Mesto.PripadaZemlji = lic.Zemlja ?? new Zemlja();
            var result = new Licnost
            {
                ID = lic.ID,
                Titula = lic.Titula,
                Ime = lic.Ime,
                Prezime = lic.Prezime,
                GodinaRodjenja = lic.GodinaRodjenja,
                GodinaRodjenjaPNE = lic.GodinaRodjenjaPNE,
                GodinaSmrti = lic.GodinaSmrti,
                GodinaSmrtiPNE = lic.GodinaSmrtiPNE,
                Pol = lic.Pol,
                Slika = lic.Slika,
                MestoRodjenja = lic.MestoRodjenja ?? "",
                Tekst = lic.Tekst
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Došlo je do greške pri radu sa Neo4j bazom: {ex.Message}");
        }
    }
    
    [HttpPut("UpdateLicnost/{id}")]
    public async Task<IActionResult> UpdateLicnost([FromBody] Licnost licnost, Guid id)
    {
        try {
            var lic = (await _client.Cypher.Match("(l:Licnost)")
                                           .Where((Licnost l) => l.ID == id)
                                           .Return(l => l.As<Licnost>())
                                           .ResultsAsync)
                                           .FirstOrDefault();
        
            if(lic == null)
            {
                return BadRequest($"Licnost sa id {id} nije pronadjena u bazi!");
            }

            var query = _client.Cypher.Match("(l:Licnost)")
                                      .Where((Licnost l) => l.ID == id)
                                      .Set("l.Titula = $titula, l.Ime = $ime, l.Prezime = $prezime, l.Pol = $pol, l.Slika = $slika, l.Tekst = $tekst")
                                      .WithParam("titula", licnost.Titula)
                                      .WithParam("ime", licnost.Ime)
                                      .WithParam("prezime", licnost.Prezime)
                                      .WithParam("pol", licnost.Pol)
                                      .WithParam("slika", licnost.Slika)
                                      .WithParam("tekst", licnost.Tekst);

            if (licnost.GodinaRodjenja != 0)//uneta godina
            {
                if (lic.GodinaRodjenja != 0)//postoji vec neka godina
                {
                    if (lic.GodinaRodjenja != licnost.GodinaRodjenja || lic.GodinaRodjenjaPNE != licnost.GodinaRodjenjaPNE)
                    {//promenjena je 
                        await _godinaService.DodajGodinu(licnost.GodinaRodjenja, licnost.GodinaRodjenjaPNE);
                        query = query.With("l")
                                     .Match("(l)-[r:RODJEN]->(sg:Godina)")
                                     .Match("(g:Godina {God: $god, IsPNE: $pne})")
                                     .WithParam("god", licnost.GodinaRodjenja)
                                     .WithParam("pne", licnost.GodinaRodjenjaPNE)                                     
                                     .Delete("r")     
                                     .Create("(l)-[:RODJEN]->(g)")
                                     .Set("l.GodinaRodjenja = $god, l.GodinaRodjenjaPNE = $pne");

                    }
                    //else ista je godina ne radi se nista 
                }
                else
                {
                    //ne postoji godina samo unosimo novu 
                    await _godinaService.DodajGodinu(licnost.GodinaRodjenja, licnost.GodinaRodjenjaPNE);
                    query = query.With("l")
                                 .Match("(g:Godina {God: $god, IsPNE: $pne})")
                                 .WithParam("god", licnost.GodinaRodjenja)
                                 .WithParam("pne", licnost.GodinaRodjenjaPNE)
                                 .Create("(l)-[:RODJEN]->(g)")
                                 .Set("l.GodinaRodjenja = $god, l.GodinaRodjenjaPNE = $pne");
                }
                
            }

            //isto samo za smrt
            if (licnost.GodinaSmrti != 0)//uneta godina
            {
                if (lic.GodinaSmrti != 0)//postoji vec neka godina
                {
                    if (lic.GodinaSmrti != licnost.GodinaSmrti || lic.GodinaSmrtiPNE != licnost.GodinaSmrtiPNE)
                    {//promenjena je 
                        await _godinaService.DodajGodinu(licnost.GodinaSmrti, licnost.GodinaSmrtiPNE);
                        query = query.With("l")
                                     .Match("(l)-[r1:UMRO]->(sgs:Godina)")
                                     .Match("(g2:Godina {God: $gods, IsPNE: $pnes})")
                                     .WithParam("gods", licnost.GodinaSmrti)
                                     .WithParam("pnes", licnost.GodinaSmrtiPNE)
                                     .Delete("r1")
                                     .Create("(l)-[:UMRO]->(g2)")
                                     .Set("l.GodinaSmrti = $gods, l.GodinaSmrtiPNE = $pnes");

                    }
                    //else ista je godina ne radi se nista 
                }
                else
                {
                    //ne postoji godina samo unosimo novu 
                    await _godinaService.DodajGodinu(licnost.GodinaSmrti, licnost.GodinaSmrtiPNE);
                    query = query.With("l")
                                 .Match("(g2:Godina {God: $gods, IsPNE: $pnes})")
                                 .WithParam("gods", licnost.GodinaSmrti)
                                 .WithParam("pnes", licnost.GodinaSmrtiPNE)
                                 .Create("(l)-[:UMRO]->(g2)")
                                 .Set("l.GodinaSmrti = $gods, l.GodinaSmrtiPNE = $pnes");
                }
                
            }

            //mesto
            if (!string.IsNullOrWhiteSpace(licnost.MestoRodjenja) && licnost.MestoRodjenja != "string")//uneto mesto 
            {
                var z = (await _client.Cypher.Match("(z:Zemlja {Naziv: $naziv})")
                                             .WithParam("naziv", licnost.MestoRodjenja)
                                             .Return(z => z.As<Zemlja>())
                                             .ResultsAsync)
                                             .FirstOrDefault();

                if (z != null)//postoji takvo mesto ima smisla da se bilo sta proverava
                {
                    if (!string.IsNullOrWhiteSpace(lic.MestoRodjenja) && lic.MestoRodjenja != "string")//vec postoji nesto u bazi  
                    {
                        //provera je l su ista mesta 
                        if (lic.MestoRodjenja != licnost.MestoRodjenja)//izmenjeno je 
                        {
                            query = query.With("l")
                                         .Match("(z:Zemlja {Naziv: $naziv})")
                                         .Match("(l)-[r2:RODJEN_U]->(sz:Zemlja)")
                                         .WithParam("naziv", licnost.MestoRodjenja)
                                         .Delete("r2")
                                         .Create("(l)-[:RODJEN_U]->(z)")
                                         .Set("l.MestoRodjenja = $naziv");
                        }
                        //else isto je 
                    }
                    else //nije postojalo mesto u bazi ali je uneto novo 
                        query = query.With("l")
                                     .Match("(z:Zemlja {Naziv: $naziv})")
                                     .WithParam("naziv", licnost.MestoRodjenja)
                                     .Create("(l)-[:RODJEN_U]->(z)")
                                     .Set("l.MestoRodjenja = $naziv");
                }
                //else to mesto ne postoji kao da nista nije ni uneto                
            }

            await query.ExecuteWithoutResultsAsync();
        
            return Ok($"Licnost sa id: {id} je uspesno promenjena!");
        }
        catch (Exception ex)  
        {
            return StatusCode(500, $"Došlo je do greške pri radu sa Neo4j bazom: {ex.Message}");
        }        
    }

    [HttpDelete("DeleteLicnost/{id}")]
    public async Task<IActionResult> DeleteLicnost(Guid id)
    {
        try {
            await _client.Cypher.Match("(l:Licnost)")
                                .Where((Licnost l) => l.ID == id)
                                .OptionalMatch("(l)-[r:RODJEN]->(gr:Godina)")
                                .OptionalMatch("(l)-[r2:UMRO]->(gs:Godina)")
                                .OptionalMatch("(l)-[r3:RODJEN_U]->(m:Zemlja)")
                                .Delete("r, r2, r3, l")
                                .ExecuteWithoutResultsAsync();

            return Ok($"Licnost sa id:{id} uspesno obrisana iz baze!");
        }        
        catch (Exception ex)  
        {
            return StatusCode(500, $"Došlo je do greške pri radu sa Neo4j bazom: {ex.Message}");
        }
    }

}







