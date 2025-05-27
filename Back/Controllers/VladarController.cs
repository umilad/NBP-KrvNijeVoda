using Microsoft.AspNetCore.Mvc;
using Neo4jClient;
using System;
using System.Threading.Tasks;
using KrvNijeVoda.Back.Models;
using System.Reflection.Metadata;
using KrvNijeVoda.Back;
[Route("api")]
[ApiController]
public class VladarController : ControllerBase
{
    private readonly IGraphClient _client;
    private readonly GodinaService _godinaService;
    // private readonly LokacijaService _lokacijaService;
    // private readonly ZemljaService _zemljaService;
    // private readonly DinastijaService _dinastijaService;

    public VladarController(Neo4jService neo4jService, GodinaService godinaService/*, LokacijaService lokacijaService, ZemljaService zemljaService, DinastijaService dinastijaService*/)
    {
        _client = neo4jService.GetClient();
        _godinaService = godinaService;
        // _lokacijaService = lokacijaService;
        // _zemljaService = zemljaService;
        // _dinastijaService = dinastijaService;
    }

    [HttpPost("CreateVladar")]
    public async Task<IActionResult> CreateVladar([FromBody] Vladar vladar)
    {
        try
        {
            //obavezni atributi    titula ime prezime na frontu    
            var postojeciVladar = (await _client.Cypher.Match("(l:Licnost:Vladar)")
                                                       .Where("toLower(l.Titula) = toLower($titula) AND toLower(l.Ime) = toLower($ime) AND toLower(l.Prezime) = toLower($prezime)") 
                                                       .WithParam("titula", vladar.Titula)
                                                       .WithParam("ime", vladar.Ime)
                                                       .WithParam("prezime", vladar.Prezime)
                                                       .Return(l => l.As<Vladar>())
                                                       .ResultsAsync)
                                                       .FirstOrDefault();


            if (postojeciVladar != null)
                return BadRequest($"Vladar {vladar.Titula} {vladar.Ime} {vladar.Prezime} vec postoji u bazi sa ID: {postojeciVladar.ID}!");

            var vladarID = Guid.NewGuid();
            var query = _client.Cypher.Create("(v:Licnost:Vladar {ID: $id, Titula: $titula, Ime: $ime, Prezime: $prezime, Pol: $pol, Slika: $slika, MestoRodjenja: $mestoRodjenja, Tekst: $tekst, Teritorija: $teritorija})")
                                      .WithParam("id", vladarID)
                                      .WithParam("titula", vladar.Titula)
                                      .WithParam("ime", vladar.Ime)
                                      .WithParam("prezime", vladar.Prezime)
                                      .WithParam("pol", vladar.Pol)
                                      .WithParam("slika", vladar.Slika)
                                      .WithParam("tekst", vladar.Tekst)
                                      .WithParam("mestoRodjenja", vladar.MestoRodjenja)
                                      .WithParam("teritorija", vladar.Teritorija);

            if (vladar.GodinaRodjenja != 0)
            {
                await _godinaService.DodajGodinu(vladar.GodinaRodjenja, vladar.GodinaRodjenjaPNE);
                query = query.With("v")
                             .Match("(gr:Godina {God: $rodj, IsPNE: $pner})")
                             .WithParam("rodj", vladar.GodinaRodjenja)
                             .WithParam("pner", vladar.GodinaRodjenjaPNE)
                             .Set("v.GodinaRodjenja = $rodj, v.GodinaRodjenjaPNE = $pner")
                             .Create("(v)-[:RODJEN]->(gr)");
            }
            if (vladar.GodinaSmrti != 0)
            {
                await _godinaService.DodajGodinu(vladar.GodinaSmrti, vladar.GodinaSmrtiPNE);
                query = query.With("v")
                             .Match("(gs:Godina {God: $smrt, IsPNE: $pnes})")
                             .WithParam("smrt", vladar.GodinaSmrti)
                             .WithParam("pnes", vladar.GodinaSmrtiPNE)
                             .Set("v.GodinaSmrti = $smrt, v.GodinaSmrtiPNE = $pnes")
                             .Create("(v)-[:UMRO]->(gs)");
            }

            if (vladar.PocetakVladavineGod != 0)
            {
                await _godinaService.DodajGodinu(vladar.PocetakVladavineGod, vladar.PocetakVladavinePNE);
                query = query.With("v")
                             .Match("(gp:Godina {God: $poc, IsPNE: $pne})")
                             .WithParam("poc", vladar.PocetakVladavineGod)
                             .WithParam("pne", vladar.PocetakVladavinePNE)
                             .Set("v.PocetakVladavineGod = $poc, v.PocetakVladavinePNE = $pne")
                             .Create("(v)-[:VLADAO_OD]->(gp)");
            }

            if (vladar.KrajVladavineGod != 0)
            {
                await _godinaService.DodajGodinu(vladar.KrajVladavineGod, vladar.KrajVladavinePNE);
                query = query.With("v")
                             .Match("(gk:Godina {God: $gkv, IsPNE: $kvpne})")
                             .WithParam("gkv", vladar.KrajVladavineGod)
                             .WithParam("kvpne", vladar.KrajVladavinePNE)
                             .Set("v.KrajVladavineGod = $gkv, v.KrajVladavinePNE = $kvpne")
                             .Create("(v)-[:VLADAO_DO]->(gk)");
            }

            if (!string.IsNullOrWhiteSpace(vladar.MestoRodjenja) && vladar.MestoRodjenja != "string")
            {
                var z = (await _client.Cypher.Match("(z:Zemlja)")
                                             .Where("toLower(z.Naziv) = toLower($naziv)")
                                             .WithParam("naziv", vladar.MestoRodjenja)
                                             .Return(z => z.As<Zemlja>())
                                             .ResultsAsync)
                                             .FirstOrDefault();

                if (z != null)
                    query = query.With("v")
                                 .Match("(z:Zemlja)")
                                 .Where("toLower(z.Naziv) = toLower($naziv)")
                                 .WithParam("naziv", vladar.MestoRodjenja)
                                 .Create("(v)-[:RODJEN_U]->(z)")
                                 .Set("v.MestoRodjenja = $naziv");
                else
                    query = query.With("v")
                                 .Set("v.MestoRodjenja = $mr")
                                 .WithParam("mr", "string");
            }

            if (vladar.Dinastija != null)
            //napravi DINASTIJASERVICE
            {
                if (!string.IsNullOrWhiteSpace(vladar.Dinastija.Naziv) || vladar.Dinastija.Naziv != "string")
                {
                    var din = (await _client.Cypher.Match("(d:Dinastija)")
                                                   .Where("toLower(d.Naziv) = toLower($naziv)")
                                                   .WithParam("naziv", vladar.Dinastija.Naziv)
                                                   .Return(d => d.As<Dinastija>())
                                                   .ResultsAsync)
                                                   .FirstOrDefault();

                    if (din != null)
                    {
                        //vladar.Dinastija = din;
                        query = query.With("v")
                                     .Match("(d:Dinastija)")
                                     .Where("toLower(d.Naziv) = toLower($naziv)")
                                     .WithParam("naziv", vladar.Dinastija.Naziv)
                                     .Create("(v)-[:PRIPADA_DINASTIJI]->(d)");
                        //za setovanje parametara svih.Set("d.Dinastija")
                        //nema potrebe jer ih ne ocitavam tu a kad povuce vezu videce sve 
                    }
                }
            }

            await query.ExecuteWithoutResultsAsync();
            return Ok($"Uspesno dodata vladar sa id:{vladarID} u bazu!");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Došlo je do greške pri radu sa Neo4j bazom: {ex.Message}");
        }
    }

    [HttpGet("GetVladar/{id}")]
    public async Task<IActionResult> GetVladar(Guid id)
    {
        try
        {
            var vl = (await _client.Cypher.Match("(v:Licnost:Vladar)")
                                          .Where((Vladar v) => v.ID == id)
                                          //   .OptionalMatch("(l)-[r:RODJEN]->(gr:Godina)")
                                          //   .OptionalMatch("(l)-[r2:UMRO]->(gs:Godina)")
                                          //   .OptionalMatch("(l)-[r3:RODJEN_U]->(m:Lokacija)-[:PRIPADA_ZEMLJI]->(z:Zemlja)")
                                          //   .OptionalMatch("(l)-[r4:VLADAO_OD]->(gpv:Godina)")
                                          //   .OptionalMatch("(l)-[r5:VLADAO_DO]->(gkv:Godina)")
                                          .OptionalMatch("(v)-[r6:PRIPADA_DINASTIJI]->(d:Dinastija)")
                                          // .Return((l, gr, gs, m, z, gpv, gkv, d) => new {
                                          //     Vladar = l.As<Vladar>(),
                                          //     Rodjen = gr.As<Godina>(),
                                          //     Umro = gs.As<Godina>(),
                                          //     Mesto =  m.As<Lokacija>(),
                                          //     Zemlja = z.As<Zemlja>(),
                                          //     Pocetak = gpv.As<Godina>(),
                                          //     Kraj = gkv.As<Godina>(),
                                          //     Dinastija = d.As<Dinastija>()                                        
                                          // }) 
                                          .Return((v, d) => new
                                          {
                                              Vladar = v.As<Vladar>(),
                                              Dinastija = d.As<Dinastija>()
                                          })
                                          //.Return(vl => vl.As<Vladar>())
                                          .ResultsAsync)
                                          .FirstOrDefault();


            if (vl.Vladar == null)
            {
                return BadRequest($"Vladar sa ID: {id} nije pronadjen u bazi!");
            }

            var result = new Vladar
            {
                ID = vl.Vladar.ID,
                Titula = vl.Vladar.Titula,
                Ime = vl.Vladar.Ime,
                Prezime = vl.Vladar.Prezime,
                GodinaRodjenja = vl.Vladar.GodinaRodjenja,
                GodinaRodjenjaPNE = vl.Vladar.GodinaRodjenjaPNE,
                GodinaSmrti = vl.Vladar.GodinaSmrti,
                GodinaSmrtiPNE = vl.Vladar.GodinaSmrtiPNE,
                Pol = vl.Vladar.Pol,
                Slika = vl.Vladar.Slika,
                MestoRodjenja = vl.Vladar.MestoRodjenja,
                Tekst = vl.Vladar.Tekst,
                Dinastija = vl.Dinastija ?? new Dinastija(),
                Teritorija = vl.Vladar.Teritorija,
                PocetakVladavineGod = vl.Vladar.PocetakVladavineGod,
                PocetakVladavinePNE = vl.Vladar.PocetakVladavinePNE,
                KrajVladavineGod = vl.Vladar.KrajVladavineGod,
                KrajVladavinePNE = vl.Vladar.KrajVladavinePNE
                //Clanovi = item.Clanovi?.ToList() ?? new List<Licnost>()  // If no Licnost found, return empty list
            };
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Došlo je do greške pri radu sa Neo4j bazom: {ex.Message}");
        }
    }

    [HttpPut("UpdateVladar/{id}")]
    public async Task<IActionResult> UpdateVladar([FromBody] Vladar vladar, Guid id)
    {
        //racunamo da mora da ima titulu, ime, prezime i od veza samo pocetak i kraj vladavine
        try {
            var vl = (await _client.Cypher.Match("(v:Licnost:Vladar)")
                                          .Where((Vladar v) => v.ID == id)
                                          .OptionalMatch("(v)-[:PRIPADA_DINASTIJI]->(d)")
                                          .Return((v, d) => new
                                          {
                                            Dinastija = d.As<Dinastija>(),
                                            Vladar = v.As<Vladar>()         
                                          })
                                          .ResultsAsync)
                                          .FirstOrDefault();
        
            //prvo update obicne atribute pa provera za sve ostale

            if(vl.Vladar == null)
            {
                return BadRequest($"Vladar sa ID: {id} nije pronadjena u bazi!");
            }

            var query = _client.Cypher.Match("(v:Licnost:Vladar)")
                                      .Where((Vladar v) => v.ID == id)
                                      .OptionalMatch("(v)-[r6:PRIPADA_DINASTIJI]->(d:Dinastija)")
                                      .Set("v.Titula = $titula, v.Ime = $ime, v.Prezime = $prezime, v.Pol = $pol, v.Slika = $slika, v.MestoRodjenja = $mestoRodjenja, v.Tekst = $tekst, v.Teritorija = $teritorija")
                                      .WithParam("titula", vladar.Titula)
                                      .WithParam("ime", vladar.Ime)
                                      .WithParam("prezime", vladar.Prezime)
                                      .WithParam("pol", vladar.Pol)
                                      .WithParam("slika", vladar.Slika)
                                      .WithParam("tekst", vladar.Tekst)
                                      .WithParam("mestoRodjenja", vl.Vladar.MestoRodjenja)
                                      .WithParam("teritorija", vladar.Teritorija);

            if (vladar.GodinaRodjenja != 0)
            {
                if (vl.Vladar.GodinaRodjenja != 0)
                {
                    if (vl.Vladar.GodinaRodjenja != vladar.GodinaRodjenja || vl.Vladar.GodinaRodjenjaPNE != vladar.GodinaRodjenjaPNE)
                    {
                        await _godinaService.DodajGodinu(vladar.GodinaRodjenja, vladar.GodinaRodjenjaPNE);
                        query = query.With("v")
                                     .Match("(v)-[r:RODJEN]->(sg:Godina)")
                                     .Match("(g:Godina {God: $god, IsPNE: $pne})")
                                     .WithParam("god", vladar.GodinaRodjenja)
                                     .WithParam("pne", vladar.GodinaRodjenjaPNE)                                     
                                     .Delete("r")     
                                     .Create("(v)-[:RODJEN]->(g)")
                                     .Set("v.GodinaRodjenja = $god, v.GodinaRodjenjaPNE = $pne");
                    }
                }
                else
                {
                    //ne postoji godina samo unosimo novu 
                    await _godinaService.DodajGodinu(vladar.GodinaRodjenja, vladar.GodinaRodjenjaPNE);
                    query = query.With("v")
                                 .Match("(g:Godina {God: $god, IsPNE: $pne})")
                                 .WithParam("god", vladar.GodinaRodjenja)
                                 .WithParam("pne", vladar.GodinaRodjenjaPNE)
                                 .Create("(v)-[:RODJEN]->(g)")
                                 .Set("v.GodinaRodjenja = $god, v.GodinaRodjenjaPNE = $pne");
                }
            }

            //isto samo za smrt
            if (vladar.GodinaSmrti != 0)//uneta godina
            {
                if (vl.Vladar.GodinaSmrti != 0)//postoji vec neka godina
                {
                    if (vl.Vladar.GodinaSmrti != vladar.GodinaSmrti || vl.Vladar.GodinaSmrtiPNE != vladar.GodinaSmrtiPNE)
                    {//promenjena je 
                        await _godinaService.DodajGodinu(vladar.GodinaSmrti, vladar.GodinaSmrtiPNE);
                        query = query.With("v")
                                     .Match("(v)-[r1:UMRO]->(sgs:Godina)")
                                     .Match("(g2:Godina {God: $gods, IsPNE: $pnes})")
                                     .WithParam("gods", vladar.GodinaSmrti)
                                     .WithParam("pnes", vladar.GodinaSmrtiPNE)
                                     .Delete("r1")
                                     .Create("(v)-[:UMRO]->(g2)")
                                     .Set("v.GodinaSmrti = $gods, v.GodinaSmrtiPNE = $pnes");

                    }
                    //else ista je godina ne radi se nista 
                }
                else
                {
                    //ne postoji godina samo unosimo novu 
                    await _godinaService.DodajGodinu(vladar.GodinaSmrti, vladar.GodinaSmrtiPNE);
                    query = query.With("v")
                                 .Match("(g2:Godina {God: $gods, IsPNE: $pnes})")
                                 .WithParam("gods", vladar.GodinaSmrti)
                                 .WithParam("pnes", vladar.GodinaSmrtiPNE)
                                 .Create("(v)-[:UMRO]->(g2)")
                                 .Set("v.GodinaSmrti = $gods, v.GodinaSmrtiPNE = $pnes");
                }
                
            }

            //mesto
            if (!string.IsNullOrWhiteSpace(vladar.MestoRodjenja) && vladar.MestoRodjenja != "string")//uneto mesto 
            {
                var z = (await _client.Cypher.Match("(z:Zemlja)")
                                             .Where("toLower(z.Naziv) = toLower($n)")
                                             .WithParam("n", vladar.MestoRodjenja)
                                             .Return(z => z.As<Zemlja>())
                                             .ResultsAsync)
                                             .FirstOrDefault();

                if (z != null)//postoji takvo mesto ima smisla da se bilo sta proverava
                {
                    if (!string.IsNullOrWhiteSpace(vl.Vladar.MestoRodjenja) && vl.Vladar.MestoRodjenja != "string")//vec postoji nesto u bazi  
                    {
                        //provera je l su ista mesta 
                        if (vl.Vladar.MestoRodjenja != vladar.MestoRodjenja)//izmenjeno je 
                        {
                            query = query.With("v")
                                         .Match("(z:Zemlja)")
                                         .Where("toLower(z.Naziv) = toLower($n)")
                                         .Match("(v)-[r2:RODJEN_U]->(sz:Zemlja)")
                                         .WithParam("n", vladar.MestoRodjenja)
                                         .Delete("r2")
                                         .Create("(v)-[:RODJEN_U]->(z)")
                                         .Set("v.MestoRodjenja = $n");
                        }
                        //else isto je 
                    }
                    else //nije postojalo mesto u bazi ali je uneto novo 
                        query = query.With("v")
                                     .Match("(z:Zemlja)")
                                     .Where("toLower(z.Naziv) = toLower($n)")
                                     .WithParam("n", vladar.MestoRodjenja)
                                     .Create("(v)-[:RODJEN_U]->(z)")
                                     .Set("v.MestoRodjenja = $n");
                }
                //else to mesto ne postoji kao da nista nije ni uneto                
            }

            if (vladar.PocetakVladavineGod != 0)
            {
                if (vl.Vladar.PocetakVladavineGod != 0)
                {
                    if (vl.Vladar.PocetakVladavineGod != vladar.PocetakVladavineGod || vl.Vladar.PocetakVladavinePNE != vladar.PocetakVladavinePNE)
                    {
                        await _godinaService.DodajGodinu(vladar.PocetakVladavineGod, vladar.PocetakVladavinePNE);
                        query = query.With("v")
                                     .Match("(gp:Godina {God: $poc, IsPNE: $pnep})")
                                     .Match("(v)-[r7:VLADAO_OD]->(gpv:Godina)")
                                     .WithParam("poc", vladar.PocetakVladavineGod)
                                     .WithParam("pnep", vladar.PocetakVladavinePNE)
                                     .Delete("r7")
                                     .Set("v.PocetakVladavineGod = $poc, v.PocetakVladavinePNE = $pnep")
                                     .Create("(v)-[:VLADAO_OD]->(gp)");
                    }
                }
                else
                {
                    await _godinaService.DodajGodinu(vladar.PocetakVladavineGod, vladar.PocetakVladavinePNE);
                    query = query.With("v")
                                 .Match("(gp:Godina {God: $poc, IsPNE: $pnep})")
                                 .WithParam("poc", vladar.PocetakVladavineGod)
                                 .WithParam("pnep", vladar.PocetakVladavinePNE)
                                 .Set("v.PocetakVladavineGod = $poc, v.PocetakVladavinePNE = $pnep")
                                 .Create("(v)-[:VLADAO_OD]->(gp)");
                }
            }

            if (vladar.KrajVladavineGod != 0)
            {
                if (vl.Vladar.KrajVladavineGod != 0)
                {
                    if (vl.Vladar.KrajVladavineGod != vladar.KrajVladavineGod || vl.Vladar.KrajVladavinePNE != vladar.KrajVladavinePNE)
                    {
                        await _godinaService.DodajGodinu(vladar.KrajVladavineGod, vladar.KrajVladavinePNE);
                        query = query.With("v")
                                     .Match("(gk:Godina {God: $kraj, IsPNE: $kvpne})")
                                     .Match("(v)-[r8:VLADAO_DO]->(gkv:Godina)")
                                     .WithParam("kraj", vladar.KrajVladavineGod)
                                     .WithParam("kvpne", vladar.KrajVladavinePNE)
                                     .Delete("r8")
                                     .Set("v.KrajVladavineGod = $kraj, v.KrajVladavinePNE = $kvpne")
                                     .Create("(v)-[:VLADAO_DO]->(gk)");
                    }
                }
                else
                {
                    await _godinaService.DodajGodinu(vladar.KrajVladavineGod, vladar.KrajVladavinePNE);
                    query = query.With("v")
                                 .Match("(gk:Godina {God: $kraj, IsPNE: $kvpne})")
                                 .WithParam("kraj", vladar.KrajVladavineGod)
                                 .WithParam("kvpne", vladar.KrajVladavinePNE)
                                 .Set("v.KrajVladavineGod = $kraj, v.KrajVladavinePNE = $kvpne")
                                 .Create("(v)-[:VLADAO_DO]->(gk)");
                }
            }
///////////////////////DO OVDE JOS DINASTIJU NAPRAVI I PREKRSTI SE 
            if (vladar.Dinastija != null && !string.IsNullOrWhiteSpace(vladar.Dinastija.Naziv) || vladar.Dinastija.Naziv != "string")
            //napravi DINASTIJASERVICE
            {
                var din = (await _client.Cypher.Match("(d:Dinastija)")
                                               .Where("toLower(d.Naziv) = toLower($naziv)")
                                               .WithParam("naziv", vladar.Dinastija.Naziv)
                                               .Return(d => d.As<Dinastija>())
                                               .ResultsAsync)
                                               .FirstOrDefault();
                                                       
                if (din != null)//postoji takva dinastija pa ima smisla da se proverava 
                {
                    if (vl.Dinastija != null)//postoji vec neka u bazi 
                    {
                        if (vladar.Dinastija.Naziv != vl.Dinastija.Naziv)
                        {//promenjena
                            vladar.Dinastija = din;
                            query = query.With("v")
                                         .Match("(d:Dinastija)")
                                         .Where("toLower(d.Naziv) = toLower($naziv)")
                                         .Match("(v)-[r9:PRIPADA_DINASTIJI]->(d)")
                                         .WithParam("naziv", vladar.Dinastija.Naziv)
                                         .Delete("r9")
                                         .Create("(v)-[:PRIPADA_DINASTIJI]->(d)");
                            //za setovanje parametara svih.Set("d.Dinastija")

                        }
                        //else ista je 
                    }
                    else //ne postoji nista u bazi samo dodaj novu 
                    {
                        vladar.Dinastija = din;
                        query = query.With("v")
                                     .Match("(d:Dinastija)")
                                     .Where("toLower(d.Naziv) = toLower($naziv)")
                                     .WithParam("naziv", vladar.Dinastija.Naziv)
                                     .Create("(v)-[:PRIPADA_DINASTIJI]->(d)");
                        //za setovanje parametara svih.Set("d.Dinastija")
                    }
                }
                
            }

            await query.ExecuteWithoutResultsAsync();
        
            return Ok($"Licnost sa id: {id} je uspesno promenjena!");
        }
        catch (Exception ex)  
        {
            return StatusCode(500, $"Došlo je do greške pri radu sa Neo4j bazom: {ex.Message}");
        }        
    }

    [HttpPut("UpdateVladarBezDinastije/{id}")]
    public async Task<IActionResult> UpdateVladarBezDinastije([FromBody] Vladar vladar, Guid id)
    {
        //racunamo da mora da ima titulu, ime, prezime i od veza samo pocetak i kraj vladavine
        try {
            var vl = (await _client.Cypher.Match("(v:Licnost:Vladar)")
                                          .Where((Vladar v) => v.ID == id)
                                          //   .OptionalMatch("(l)-[r:RODJEN]->(gr:Godina)")
                                          //   .OptionalMatch("(l)-[r2:UMRO]->(gs:Godina)")
                                          //   .OptionalMatch("(l)-[r3:RODJEN_U]->(m:Lokacija)-[:PRIPADA_ZEMLJI]->(z:Zemlja)")
                                          //   .OptionalMatch("(l)-[r4:VLADAO_OD]->(gpv:Godina)")
                                          //   .OptionalMatch("(l)-[r5:VLADAO_DO]->(gkv:Godina)")
                                          //.OptionalMatch("(v)-[r6:PRIPADA_DINASTIJI]->(d:Dinastija)")
                                          // .Return((l, gr, gs, m, z, gpv, gkv, d) => new {
                                          //     Vladar = l.As<Vladar>(),
                                          //     Rodjen = gr.As<Godina>(),
                                          //     Umro = gs.As<Godina>(),
                                          //     Mesto =  m.As<Lokacija>(),
                                          //     Zemlja = z.As<Zemlja>(),
                                          //     Pocetak = gpv.As<Godina>(),
                                          //     Kraj = gkv.As<Godina>(),
                                          //     Dinastija = d.As<Dinastija>()                                        
                                          // }) 
                                          .Return(v => v.As<Vladar>())
                                          .ResultsAsync)
                                          .FirstOrDefault();
        
            //prvo update obicne atribute pa provera za sve ostale

            if(vl == null)
            {
                return BadRequest($"Vladar sa ID: {id} nije pronadjena u bazi!");
            }

            var postojeciVladar = (await _client.Cypher.Match("(l:Licnost:Vladar)")
                                                       .Where("toLower(l.Titula) = toLower($titula) AND toLower(l.Ime) = toLower($ime) AND toLower(l.Prezime) = toLower($prezime) AND l.ID <> $id")
                                                       .WithParam("titula", vladar.Titula)
                                                       .WithParam("ime", vladar.Ime)
                                                       .WithParam("prezime", vladar.Prezime)
                                                       .WithParam("id", id)
                                                       .Return(l => l.As<Vladar>())
                                                       .ResultsAsync)
                                                       .FirstOrDefault();


            if (postojeciVladar != null)
                return BadRequest($"Vladar {vladar.Titula} {vladar.Ime} {vladar.Prezime} vec postoji u bazi sa ID: {postojeciVladar.ID}!");


            var query = _client.Cypher.Match("(v:Licnost:Vladar)")
                                      .Where((Vladar v) => v.ID == id)
                                      .OptionalMatch("(v)-[r6:PRIPADA_DINASTIJI]->(d:Dinastija)")
                                      .Set("v.Titula = $titula, v.Ime = $ime, v.Prezime = $prezime, v.Pol = $pol, v.Slika = $slika, v.MestoRodjenja = $mestoRodjenja, v.Tekst = $tekst, v.Teritorija = $teritorija")
                                      .WithParam("titula", vladar.Titula)
                                      .WithParam("ime", vladar.Ime)
                                      .WithParam("prezime", vladar.Prezime)
                                      .WithParam("pol", vladar.Pol)
                                      .WithParam("slika", vladar.Slika)
                                      .WithParam("tekst", vladar.Tekst)
                                      .WithParam("mestoRodjenja", vladar.MestoRodjenja)
                                      .WithParam("teritorija", vladar.Teritorija);

            if (vladar.GodinaRodjenja != 0)
            {
                if (vl.GodinaRodjenja != 0)
                {
                    if (vl.GodinaRodjenja != vladar.GodinaRodjenja || vl.GodinaRodjenjaPNE != vladar.GodinaRodjenjaPNE)
                    {
                        await _godinaService.DodajGodinu(vladar.GodinaRodjenja, vladar.GodinaRodjenjaPNE);
                        query = query.With("v")
                                     .Match("(v)-[r:RODJEN]->(sg:Godina)")
                                     .Match("(g:Godina {God: $god, IsPNE: $pne})")
                                     .WithParam("god", vladar.GodinaRodjenja)
                                     .WithParam("pne", vladar.GodinaRodjenjaPNE)
                                     .Delete("r")
                                     .Create("(v)-[:RODJEN]->(g)")
                                     .Set("v.GodinaRodjenja = $god, v.GodinaRodjenjaPNE = $pne");
                    }
                }
                else
                {
                    //ne postoji godina samo unosimo novu 
                    await _godinaService.DodajGodinu(vladar.GodinaRodjenja, vladar.GodinaRodjenjaPNE);
                    query = query.With("v")
                                 .Match("(g:Godina {God: $god, IsPNE: $pne})")
                                 .WithParam("god", vladar.GodinaRodjenja)
                                 .WithParam("pne", vladar.GodinaRodjenjaPNE)
                                 .Create("(v)-[:RODJEN]->(g)")
                                 .Set("v.GodinaRodjenja = $god, v.GodinaRodjenjaPNE = $pne");
                }
            }
            else //nije uneta godina brisemo staru
            {
                query = query.With("v")
                               .OptionalMatch("(v)-[r1:RODJEN]->()")
                               .Delete("r1");
            }

            //isto samo za smrt
            if (vladar.GodinaSmrti != 0)//uneta godina
            {
                if (vl.GodinaSmrti != 0)//postoji vec neka godina
                {
                    if (vl.GodinaSmrti != vladar.GodinaSmrti || vl.GodinaSmrtiPNE != vladar.GodinaSmrtiPNE)
                    {//promenjena je 
                        await _godinaService.DodajGodinu(vladar.GodinaSmrti, vladar.GodinaSmrtiPNE);
                        query = query.With("v")
                                     .Match("(v)-[r1:UMRO]->(sgs:Godina)")
                                     .Match("(g2:Godina {God: $gods, IsPNE: $pnes})")
                                     .WithParam("gods", vladar.GodinaSmrti)
                                     .WithParam("pnes", vladar.GodinaSmrtiPNE)
                                     .Delete("r1")
                                     .Create("(v)-[:UMRO]->(g2)")
                                     .Set("v.GodinaSmrti = $gods, v.GodinaSmrtiPNE = $pnes");

                    }
                    //else ista je godina ne radi se nista 
                }
                else
                {
                    //ne postoji godina samo unosimo novu 
                    await _godinaService.DodajGodinu(vladar.GodinaSmrti, vladar.GodinaSmrtiPNE);
                    query = query.With("v")
                                 .Match("(g2:Godina {God: $gods, IsPNE: $pnes})")
                                 .WithParam("gods", vladar.GodinaSmrti)
                                 .WithParam("pnes", vladar.GodinaSmrtiPNE)
                                 .Create("(v)-[:UMRO]->(g2)")
                                 .Set("v.GodinaSmrti = $gods, v.GodinaSmrtiPNE = $pnes");
                }

            }
            else
            {
                query = query.With("v")
                               .OptionalMatch("(v)-[r2:UMRO]->()")
                               .Delete("r2");
            }

            //mesto
            if (!string.IsNullOrWhiteSpace(vladar.MestoRodjenja) && vladar.MestoRodjenja != "string")//uneto mesto 
            {
                var z = (await _client.Cypher.Match("(z:Zemlja)")
                                             .Where("toLower(z.Naziv) = toLower($naziv)")
                                             .WithParam("naziv", vladar.MestoRodjenja)
                                             .Return(z => z.As<Zemlja>())
                                             .ResultsAsync)
                                             .FirstOrDefault();

                if (z != null)//postoji takvo mesto ima smisla da se bilo sta proverava
                {
                    if (!string.IsNullOrWhiteSpace(vl.MestoRodjenja) && vl.MestoRodjenja != "string")//vec postoji nesto u bazi  
                    {
                        //provera je l su ista mesta 
                        if (vl.MestoRodjenja != vladar.MestoRodjenja)//izmenjeno je 
                        {
                            query = query.With("v")
                                         .Match("(z:Zemlja)")
                                         .Where("toLower(z.Naziv) = toLower($naziv)")
                                         .Match("(v)-[r2:RODJEN_U]->(sz:Zemlja)")
                                         .WithParam("naziv", vladar.MestoRodjenja)
                                         .Delete("r2")
                                         .Create("(v)-[:RODJEN_U]->(z)")
                                         .Set("v.MestoRodjenja = $naziv");
                        }
                        //else isto je 
                    }
                    else //nije postojalo mesto u bazi ali je uneto novo 
                        query = query.With("v")
                                     .Match("(z:Zemlja)")
                                     .Where("toLower(z.Naziv) = toLower($naziv)")
                                     .WithParam("naziv", vladar.MestoRodjenja)
                                     .Create("(v)-[:RODJEN_U]->(z)")
                                     .Set("v.MestoRodjenja = $naziv");
                }
                //else to mesto ne postoji kao da nista nije ni uneto                
            }

            if (vladar.PocetakVladavineGod != 0)
            {
                if (vl.PocetakVladavineGod != 0)
                {
                    if (vl.PocetakVladavineGod != vladar.PocetakVladavineGod || vl.PocetakVladavinePNE != vladar.PocetakVladavinePNE)
                    {
                        await _godinaService.DodajGodinu(vladar.PocetakVladavineGod, vladar.PocetakVladavinePNE);
                        query = query.With("v")
                                     .Match("(gp:Godina {God: $poc, IsPNE: $pnep})")
                                     .Match("(v)-[r7:VLADAO_OD]->(gpv:Godina)")
                                     .WithParam("poc", vladar.PocetakVladavineGod)
                                     .WithParam("pnep", vladar.PocetakVladavinePNE)
                                     .Delete("r7")
                                     .Set("v.PocetakVladavineGod = $poc, v.PocetakVladavinePNE = $pnep")
                                     .Create("(v)-[:VLADAO_OD]->(gp)");
                    }
                }
                else
                {
                    await _godinaService.DodajGodinu(vladar.PocetakVladavineGod, vladar.PocetakVladavinePNE);
                    query = query.With("v")
                                 .Match("(gp:Godina {God: $poc, IsPNE: $pnep})")
                                 .WithParam("poc", vladar.PocetakVladavineGod)
                                 .WithParam("pnep", vladar.PocetakVladavinePNE)
                                 .Set("v.PocetakVladavineGod = $poc, v.PocetakVladavinePNE = $pnep")
                                 .Create("(v)-[:VLADAO_OD]->(gp)");
                }
            }
            else
            {
                query = query.With("v")
                               .OptionalMatch("(v)-[r3:VLADAO_OD]->()")
                               .Delete("r3");
            }

            if (vladar.KrajVladavineGod != 0)
            {
                if (vl.KrajVladavineGod != 0)
                {
                    if (vl.KrajVladavineGod != vladar.KrajVladavineGod || vl.KrajVladavinePNE != vladar.KrajVladavinePNE)
                    {
                        await _godinaService.DodajGodinu(vladar.KrajVladavineGod, vladar.KrajVladavinePNE);
                        query = query.With("v")
                                     .Match("(gk:Godina {God: $kraj, IsPNE: $kvpne})")
                                     .Match("(v)-[r8:VLADAO_DO]->(gkv:Godina)")
                                     .WithParam("kraj", vladar.KrajVladavineGod)
                                     .WithParam("kvpne", vladar.KrajVladavinePNE)
                                     .Delete("r8")
                                     .Set("v.KrajVladavineGod = $kraj, v.KrajVladavinePNE = $kvpne")
                                     .Create("(v)-[:VLADAO_DO]->(gk)");
                    }
                }
                else
                {
                    await _godinaService.DodajGodinu(vladar.KrajVladavineGod, vladar.KrajVladavinePNE);
                    query = query.With("v")
                                 .Match("(gk:Godina {God: $kraj, IsPNE: $kvpne})")
                                 .WithParam("kraj", vladar.KrajVladavineGod)
                                 .WithParam("kvpne", vladar.KrajVladavinePNE)
                                 .Set("v.KrajVladavineGod = $kraj, v.KrajVladavinePNE = $kvpne")
                                 .Create("(v)-[:VLADAO_DO]->(gk)");
                }
            }
            else
            {
                query = query.With("v")
                               .OptionalMatch("(v)-[r4:VLADAO_DO]->()")
                               .Delete("r4");
            }

            await query.ExecuteWithoutResultsAsync();

            return Ok($"Licnost sa id: {id} je uspesno promenjena!");
        }
        catch (Exception ex)  
        {
            return StatusCode(500, $"Došlo je do greške pri radu sa Neo4j bazom: {ex.Message}");
        }        
    }

    [HttpDelete("DeleteVladar/{id}")]
    public async Task<IActionResult> DeleteVladar(Guid id)
    {
        try
        {
            var v = (await _client.Cypher.Match("(v:Licnost:Vladar)")
                                         .Where((Vladar v) => v.ID == id)
                                         .Return(v => v.As<Vladar>())
                                         .ResultsAsync)
                                         .FirstOrDefault();

            if (v == null)
            {
                return BadRequest($"Vladar sa ID: {id} nije pronadjen u bazi!");
            }

            await _client.Cypher.Match("(v:Licnost:Vladar)")
                                .Where((Vladar v) => v.ID == id)
                                .OptionalMatch("(v)-[r:RODJEN]->(gr:Godina)")
                                .OptionalMatch("(v)-[r2:UMRO]->(gs:Godina)")
                                .OptionalMatch("(v)-[r3:RODJEN_U]->(m:Zemlja)")
                                .OptionalMatch("(v)-[r4:VLADAO_OD]->(gpv:Godina)")
                                .OptionalMatch("(v)-[r5:VLADAO_DO]->(gkv:Godina)")
                                .OptionalMatch("(v)-[r6:PRIPADA_DINASTIJI]->(d:Dinastija)")
                                .Delete("r, r2, r3, r4, r5, r6, v")
                                .ExecuteWithoutResultsAsync();

            return Ok($"Vladar sa id:{id} uspesno obrisan iz baze!");
        }        
        catch (Exception ex)  
        {
            return StatusCode(500, $"Došlo je do greške pri radu sa Neo4j bazom: {ex.Message}");
        }
    }

}