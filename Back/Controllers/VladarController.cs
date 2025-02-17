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
    private readonly LokacijaService _lokacijaService;
    private readonly ZemljaService _zemljaService;
    private readonly DinastijaService _dinastijaService;

    public VladarController(Neo4jService neo4jService, GodinaService godinaService, LokacijaService lokacijaService, ZemljaService zemljaService, DinastijaService dinastijaService)
    {
        _client = neo4jService.GetClient();  
        _godinaService = godinaService;
        _lokacijaService = lokacijaService;
        _zemljaService = zemljaService;
        _dinastijaService = dinastijaService;
    }

    
    [HttpPost("CreateVladar")]
    public async Task<IActionResult> CreateVladar([FromBody] Vladar vladar)
    {   
        try
        {
             //obavezni atributi                                        
            if (string.IsNullOrWhiteSpace(vladar.Titula) || vladar.Titula == "string")
            {
                return BadRequest("Morate uneti titulu!");
            }
            if (string.IsNullOrWhiteSpace(vladar.Ime) || vladar.Ime == "string")
            {
                return BadRequest("Morate uneti ime!");
            }
            if (string.IsNullOrWhiteSpace(vladar.Prezime) || vladar.Prezime == "string")
            {
                return BadRequest("Morate uneti prezime!");
            }

            var postojeciVladar = (await _client.Cypher.Match("(l:Licnost:Vladar)")
                                                       .Where("l.Titula = $titula AND l.Ime = $ime AND l.Prezime = $prezime")
                                                       .WithParam("titula", vladar.Titula)
                                                       .WithParam("ime", vladar.Ime)
                                                       .WithParam("prezime", vladar.Prezime)
                                                       .Return(l => l.As<Vladar>())
                                                       .ResultsAsync)
                                                       .FirstOrDefault();
           

            if(postojeciVladar != null)
                return BadRequest($"Vladar {vladar.Titula} {vladar.Ime} {vladar.Prezime} vec postoji u bazi sa ID: {postojeciVladar.ID}!");

            if(vladar.PocetakVladavine == null || vladar.PocetakVladavine.God == 0)
            {
                return BadRequest("Morate uneti godinu pocetka vladavine!");
            }

            if(vladar.KrajVladavine == null || vladar.KrajVladavine.God == 0)
            {
                return BadRequest("Morate uneti godinu kraja vladavine!");
            }
            if(vladar.PocetakVladavine.God > vladar.KrajVladavine.God)
            {
                return BadRequest("Godina pocetka mora biti manja od godine kraja vladavine!");
            }

            await _godinaService.DodajGodinu(vladar.PocetakVladavine.God);
            await _godinaService.DodajGodinu(vladar.KrajVladavine.God);

            if(vladar.Dinastija != null)
            //napravi DINASTIJASERVICE
            {
                if(!string.IsNullOrWhiteSpace(vladar.Dinastija.Naziv) || vladar.Dinastija.Naziv != "string")
                    await _dinastijaService.DodajDinastiju(vladar.Dinastija);
                else 
                    Console.WriteLine("Dinastija nije dodata jer nije unet naziv dinastije!");        
            }

            var vladarID = Guid.NewGuid();
            var cypher = _client.Cypher.Match("(gp: Godina {God: $godP})", "(gk: Godina {God: $godK})")
                                       .WithParam("id", vladarID)
                                       .WithParam("titula", vladar.Titula)
                                       .WithParam("ime", vladar.Ime)
                                       .WithParam("prezime", vladar.Prezime)                                       
                                       .WithParam("pol", vladar.Pol)
                                       .WithParam("slika", vladar.Slika)
                                       .WithParam("teritorija", vladar.Teritorija)
                                       .WithParam("godP", vladar.PocetakVladavine.God)
                                       .WithParam("godK", vladar.KrajVladavine.God)
                                       .Create("(v:Licnost:Vladar {ID: $id, Titula: $titula, Ime: $ime, Prezime: $prezime, Pol: $pol, Slika: $slika, Teritorija: $teritorija})")
                                       .Create("(v)-[:VLADAO_OD]->(gp)")
                                       .Create("(v)-[:VLADAO_DO]->(gk)");

            if(vladar.GodinaRodjenja != null)
            {
                if(vladar.GodinaRodjenja.God < vladar.PocetakVladavine.God)
                {                    
                    await _godinaService.DodajGodinu(vladar.GodinaRodjenja!.God);
                    cypher = cypher.With("v")
                                   .Match("(gr:Godina {God: $rodj})")
                                   .WithParam("rodj", vladar.GodinaRodjenja.God)
                                   .Create("(v)-[:RODJEN]->(gr)");
                }
                else 
                    Console.WriteLine("Godina rodjenja i godine vladavine nisu konzistente! Godina rodjenja nije uneta u bazu!");
            }
            if(vladar.GodinaSmrti != null)
            {
                if(vladar.GodinaSmrti.God > vladar.KrajVladavine.God)
                {
                    await _godinaService.DodajGodinu(vladar.GodinaSmrti!.God);
                    cypher = cypher.With("v")
                                   .Match("(gs:Godina {God: $smrt})")
                                   .WithParam("smrt", vladar.GodinaSmrti.God)
                                   .Create("(v)-[:UMRO]->(gs)");
                }   
                else                     
                    Console.WriteLine("Godina smrti i godine vladavine nisu konzistente! Godina smrti nije uneta u bazu!");
                
            }
            if(vladar.MestoRodjenja != null && !string.IsNullOrWhiteSpace(vladar.MestoRodjenja.Naziv) && vladar.MestoRodjenja.Naziv != "string")
            {
                if(vladar.MestoRodjenja.PripadaZemlji != null)//mora da ima zemlju 
                {
                    var nz = await _zemljaService.DodajZemljuParametri(vladar.MestoRodjenja.PripadaZemlji.Naziv, vladar.MestoRodjenja.PripadaZemlji.Grb, vladar.MestoRodjenja.PripadaZemlji.Trajanje);
                    var nl = await _lokacijaService.DodajLokaciju(vladar.MestoRodjenja.Naziv, nz);
                    cypher = cypher.With("v")
                                   .Match("(lm:Lokacija {ID: $mid})")
                                   .WithParam("mid", nl.ID)
                                   .Create("(v)-[:RODJEN_U]->(lm)");
                }
                else {
                    await cypher.ExecuteWithoutResultsAsync();
                    return Ok($"Uspesno dodat vladar sa id:{vladarID} u bazu, ALI bez mesta jer nije stavljena zemlja kojoj pripada!");
                }
            }
            await cypher.ExecuteWithoutResultsAsync();
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
            var vl = (await _client.Cypher.Match("(l:Licnost:Vladar)")
                                           .Where((Vladar l) => l.ID == id)
                                           .OptionalMatch("(l)-[r:RODJEN]->(gr:Godina)")
                                           .OptionalMatch("(l)-[r2:UMRO]->(gs:Godina)")
                                           .OptionalMatch("(l)-[r3:RODJEN_U]->(m:Lokacija)-[:PRIPADA_ZEMLJI]->(z:Zemlja)")
                                           .OptionalMatch("(l)-[r4:VLADAO_OD]->(gpv:Godina)")
                                           .OptionalMatch("(l)-[r5:VLADAO_DO]->(gkv:Godina)")
                                           .OptionalMatch("(l)-[r6:PRIPADA_DINASTIJI]->(d:Dinastija)")
                                           .Return((l, gr, gs, m, z, gpv, gkv, d) => new {
                                               Vladar = l.As<Vladar>(),
                                               Rodjen = gr.As<Godina>(),
                                               Umro = gs.As<Godina>(),
                                               Mesto =  m.As<Lokacija>(),
                                               Zemlja = z.As<Zemlja>(),
                                               Pocetak = gpv.As<Godina>(),
                                               Kraj = gkv.As<Godina>(),
                                               Dinastija = d.As<Dinastija>()                                        
                                           }) 
                                           .ResultsAsync)
                                           .FirstOrDefault();
            

            if(vl == null)
            {
                return BadRequest($"Vladar sa id {id} nije pronadjen u bazi!");
            }
            if(vl.Mesto != null)
                vl.Mesto.PripadaZemlji = vl.Zemlja ?? new Zemlja();
            var result = new Vladar {
                        ID = vl.Vladar.ID,
                        Titula = vl.Vladar.Titula,
                        Ime = vl.Vladar.Ime,
                        Prezime = vl.Vladar.Prezime,
                        GodinaRodjenja = vl.Rodjen ?? new Godina(),
                        GodinaSmrti = vl.Umro ?? new Godina(),
                        Pol = vl.Vladar.Pol,
                        Slika = vl.Vladar.Slika,
                        MestoRodjenja = vl.Mesto ?? new Lokacija(),
                        Dinastija = vl.Dinastija ?? new Dinastija(),
                        Teritorija = vl.Vladar.Teritorija,
                        PocetakVladavine = vl.Pocetak ?? new Godina(),
                        KrajVladavine = vl.Kraj ?? new Godina()
                        //Clanovi = item.Clanovi?.ToList() ?? new List<Licnost>()  // If no Licnost found, return empty list
                        };
            return Ok(result);
        }
        catch (Exception ex)  
        {
            return StatusCode(500, $"Došlo je do greške pri radu sa Neo4j bazom: {ex.Message}");
        }                
    }

    // [HttpPut("UpdateLicnost/{id}")]
    // public async Task<IActionResult> UpdateLicnost([FromBody] Licnost licnost, Guid id)
    // {
    //     try {
    //         var lic = (await _client.Cypher.Match("(l:Licnost)")
    //                                        .Where((Licnost l) => l.ID == id)
    //                                        .OptionalMatch("(l)-[r:RODJEN]->(gr:Godina)")
    //                                        .OptionalMatch("(l)-[r2:UMRO]->(gs:Godina)")
    //                                        .OptionalMatch("(l)-[r3:RODJEN_U]->(m:Lokacija)-[:PRIPADA_ZEMLJI]->(z:Zemlja)")
    //                                        .Return((l, gr, gs, m, z) => new {
    //                                             Licnost = l.As<Licnost>(),
    //                                             Rodjen = gr.As<Godina>(),
    //                                             Umro = gs.As<Godina>(),
    //                                             Mesto =  m.As<Lokacija>(),
    //                                             Zemlja = z.As<Zemlja>()                                        
    //                                         }) 
    //                                        .ResultsAsync)
    //                                        .FirstOrDefault();
        

    //         if(lic == null)
    //         {
    //             return BadRequest($"Licnost sa id {id} nije pronadjena u bazi!");
    //         }

    //         bool promenjenaGodRodj = false;
    //         bool promenjenaGodSmrti = false;
    //         bool promenjenoMestoRodj = false;

    //         if(licnost.GodinaRodjenja != null)
    //         {
    //             if(lic.Rodjen != null)
    //             {
    //                 if(lic.Rodjen.God != licnost.GodinaRodjenja.God)
    //                 {
    //                     await _godinaService.DodajGodinu(licnost.GodinaRodjenja.God);
    //                     promenjenaGodRodj = true;
    //                 }
    //             }
    //             else 
    //             promenjenaGodRodj = true;
    //         }

    //         if(licnost.GodinaSmrti != null)
    //         {
    //             if(lic.Umro != null)
    //             {
    //                 if(lic.Umro.God != licnost.GodinaSmrti.God)
    //                 {
    //                     await _godinaService.DodajGodinu(licnost.GodinaSmrti.God);
    //                     promenjenaGodSmrti = true;
    //                 }
    //             }
    //             else
    //                 promenjenaGodSmrti = true;
    //         }
    //         Lokacija nl = new Lokacija();
    //         if(licnost.MestoRodjenja != null && licnost.MestoRodjenja.PripadaZemlji != null)
    //         {
    //             var nz = await _zemljaService.DodajZemlju(licnost.MestoRodjenja.PripadaZemlji);
    //             nl = await _lokacijaService.DodajLokaciju(licnost.MestoRodjenja.Naziv, nz);
    //             if(lic.Mesto != null && lic.Zemlja != null)//jer ne moze da se napravi mesto bez zemlje
    //             {
    //                 if(lic.Mesto.Naziv != licnost.MestoRodjenja.Naziv || lic.Zemlja.Naziv != licnost.MestoRodjenja.PripadaZemlji.Naziv)
    //                 {
    //                     promenjenoMestoRodj = true;
    //                 }
    //             }
    //             else
    //                 promenjenoMestoRodj = true;
    //         }

    //         await _client.Cypher.Match("(l:Licnost)")
    //                             .Where("l.ID = $id")
    //                             .Set("l.Titula = $titula, l.Ime = $ime, l.Prezime = $prezime, l.Pol = $pol, l.Slika = $slika")
    //                             .WithParam("id", id)
    //                             .WithParam("titula", licnost.Titula)
    //                             .WithParam("ime", licnost.Ime)
    //                             .WithParam("prezime", licnost.Prezime)

    //                             .WithParam("pol", licnost.Pol)
    //                             .WithParam("slika", licnost.Slika)
    //                             .ExecuteWithoutResultsAsync();

    //         if(promenjenaGodRodj)
    //         {
    //             await _client.Cypher.Match("(l:Licnost)", "(gr:Godina)")
    //                                 .Where("l.ID = $id AND gr.God = $rodjen")
    //                                 .OptionalMatch("(l)-[r:RODJEN]->(:Godina)")
    //                                 .Delete("r")
    //                                 .Create("(l)-[:RODJEN]->(gr)")
    //                                 .WithParam("id", id)
    //                                 .WithParam("rodjen", licnost.GodinaRodjenja!.God)//provera za null je bila da je null ne bi bool bio true 
    //                                 .ExecuteWithoutResultsAsync();
    //         }
    //         if(promenjenaGodSmrti)
    //         {
    //             await _client.Cypher.Match("(l:Licnost)", "(gs:Godina)")
    //                                 .Where("l.ID = $id AND gs.God = $umro")
    //                                 .OptionalMatch("(l)-[r:UMRO]->(:Godina)")
    //                                 .Delete("r")
    //                                 .Create("(l)-[:UMRO]->(gs)")
    //                                 .WithParam("id", id)
    //                                 .WithParam("umro", licnost.GodinaSmrti!.God)//provera za null je bila da je null ne bi bool bio true 
    //                                 .ExecuteWithoutResultsAsync();
    //         }
    //         if(promenjenoMestoRodj)
    //         {
    //             await _client.Cypher.Match("(l:Licnost)", "(mr:Lokacija)")
    //                                 .Where("l.ID = $id AND mr.ID = $mrid")
    //                                 .OptionalMatch("(l)-[r:RODJEN_U]->(:Lokacija)")
    //                                 .Delete("r")
    //                                 .Create("(l)-[:RODJEN_U]->(mr)")
    //                                 .WithParam("id", id)
    //                                 .WithParam("mrid", nl.ID)
    //                                 .ExecuteWithoutResultsAsync();
    //         }
        
    //         return Ok($"Licnost sa id: {id} je uspesno promenjena!");
    //     }
    //     catch (Exception ex)  
    //     {
    //         return StatusCode(500, $"Došlo je do greške pri radu sa Neo4j bazom: {ex.Message}");
    //     }        
    // }

    [HttpDelete("DeleteLicnost/{id}")]
    public async Task<IActionResult> DeleteLicnost(Guid id)
    {
        try {
            await _client.Cypher.Match("(v:Licnost:Vladar)")
                                .Where((Vladar v) => v.ID == id)
                                .OptionalMatch("(v)-[r:RODJEN]->(gr:Godina)")
                                .OptionalMatch("(v)-[r2:UMRO]->(gs:Godina)")
                                .OptionalMatch("(v)-[r3:RODJEN_U]->(m:Lokacija)")
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