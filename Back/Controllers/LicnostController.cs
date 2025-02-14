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
                                    .Return((l, gr, gs, m, z) => new {
                                        Licnost = l.As<Licnost>(),
                                        Rodjen = gr.As<Godina>(),
                                        Umro = gs.As<Godina>(),
                                        Mesto =  m.As<Lokacija>(),
                                        Zemlja = z.As<Zemlja>()                                        
                                    }) 
                                    .ResultsAsync)
                                    .FirstOrDefault();
        

        if(lic == null)
        {
            return BadRequest($"Licnost sa id {id} nije pronadjena u bazi!");
        }
        if(lic.Mesto != null)
            lic.Mesto.PripadaZemlji = lic.Zemlja ?? new Zemlja();
        var result = new Licnost {
                    ID = lic.Licnost.ID,
                    Titula = lic.Licnost.Titula,
                    Ime = lic.Licnost.Ime,
                    Prezime = lic.Licnost.Prezime,
                    GodinaRodjenja = lic.Rodjen ?? new Godina(),
                    GodinaSmrti = lic.Umro ?? new Godina(),
                    Pol = lic.Licnost.Pol,
                    Slika = lic.Licnost.Slika,
                    MestoRodjenja = lic.Mesto ?? new Lokacija()
                    //Clanovi = item.Clanovi?.ToList() ?? new List<Licnost>()  // If no Licnost found, return empty list
                };
        return Ok(result);
                
    }

    [HttpPut("UpdateLicnost/{id}")]
    public async Task<IActionResult> UpdateLicnost([FromBody] Licnost licnost, Guid id)
    {
        var lic = (await _client.Cypher.Match("(l:Licnost)")
                                    .Where((Licnost l) => l.ID == id)
                                    .OptionalMatch("(l)-[r:RODJEN]->(gr:Godina)")
                                    .OptionalMatch("(l)-[r2:UMRO]->(gs:Godina)")
                                    .OptionalMatch("(l)-[r3:RODJEN_U]->(m:Lokacija)-[:PRIPADA_ZEMLJI]->(z:Zemlja)")
                                    .Return((l, gr, gs, m, z) => new {
                                        Licnost = l.As<Licnost>(),
                                        Rodjen = gr.As<Godina>(),
                                        Umro = gs.As<Godina>(),
                                        Mesto =  m.As<Lokacija>(),
                                        Zemlja = z.As<Zemlja>()                                        
                                    }) 
                                    .ResultsAsync)
                                    .FirstOrDefault();
        

        if(lic == null)
        {
            return BadRequest($"Licnost sa id {id} nije pronadjena u bazi!");
        }

        bool promenjenaGodRodj = false;
        bool promenjenaGodSmrti = false;
        bool promenjenoMestoRodj = false;

        if(licnost.GodinaRodjenja != null)
        {
            if(lic.Rodjen != null)
            {
                if(lic.Rodjen.God != licnost.GodinaRodjenja.God)
                {
                    await _godinaService.DodajGodinu(licnost.GodinaRodjenja.God);
                    promenjenaGodRodj = true;
                }
            }
            else 
            promenjenaGodRodj = true;
        }

        if(licnost.GodinaSmrti != null)
        {
            if(lic.Umro != null)
            {
                if(lic.Umro.God != licnost.GodinaSmrti.God)
                {
                    await _godinaService.DodajGodinu(licnost.GodinaSmrti.God);
                    promenjenaGodSmrti = true;
                }
            }
            else
                promenjenaGodSmrti = true;
        }
        Lokacija nl = new Lokacija();
        if(licnost.MestoRodjenja != null && licnost.MestoRodjenja.PripadaZemlji != null)
        {
            var nz = await _zemljaService.DodajZemlju(licnost.MestoRodjenja.PripadaZemlji);
            nl = await _lokacijaService.DodajLokaciju(licnost.MestoRodjenja.Naziv, nz);
            if(lic.Mesto != null && lic.Zemlja != null)//jer ne moze da se napravi mesto bez zemlje
            {
                if(lic.Mesto.Naziv != licnost.MestoRodjenja.Naziv || lic.Zemlja.Naziv != licnost.MestoRodjenja.PripadaZemlji.Naziv)
                {
                    promenjenoMestoRodj = true;
                }
            }
            else
                promenjenoMestoRodj = true;
        }

        await _client.Cypher.Match("(l:Licnost)")
                            .Where("l.ID = $id")
                            .Set("l.Titula = $titula, l.Ime = $ime, l.Prezime = $prezime, l.Pol = $pol, l.Slika = $slika")
                            .WithParam("id", id)
                            .WithParam("titula", licnost.Titula)
                            .WithParam("ime", licnost.Ime)
                            .WithParam("prezime", licnost.Prezime)
                            .WithParam("pol", licnost.Pol)
                            .WithParam("slika", licnost.Slika)
                            .ExecuteWithoutResultsAsync();

        if(promenjenaGodRodj)
        {
            await _client.Cypher.Match("(l:Licnost)", "(gr:Godina)")
                                .Where("l.ID = $id AND gr.God = $rodjen")
                                .OptionalMatch("(l)-[r:RODJEN]->(:Godina)")
                                .Delete("r")
                                .Create("(l)-[:RODJEN]->(gr)")
                                .WithParam("id", id)
                                .WithParam("rodjen", licnost.GodinaRodjenja!.God)//provera za null je bila da je null ne bi bool bio true 
                                .ExecuteWithoutResultsAsync();
        }
        if(promenjenaGodSmrti)
        {
            await _client.Cypher.Match("(l:Licnost)", "(gs:Godina)")
                                .Where("l.ID = $id AND gs.God = $umro")
                                .OptionalMatch("(l)-[r:UMRO]->(:Godina)")
                                .Delete("r")
                                .Create("(l)-[:UMRO]->(gs)")
                                .WithParam("id", id)
                                .WithParam("umro", licnost.GodinaSmrti!.God)//provera za null je bila da je null ne bi bool bio true 
                                .ExecuteWithoutResultsAsync();
        }
        if(promenjenoMestoRodj)
        {
            await _client.Cypher.Match("(l:Licnost)", "(mr:Lokacija)")
                                .Where("l.ID = $id AND mr.ID = $mrid")
                                .OptionalMatch("(l)-[r:RODJEN_U]->(:Lokacija)")
                                .Delete("r")
                                .Create("(l)-[:RODJEN_U]->(mr)")
                                .WithParam("id", id)
                                .WithParam("mrid", nl.ID)
                                .ExecuteWithoutResultsAsync();
        }
       
        return Ok($"Licnost sa id: {id} je uspesno promenjena!");
    }

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







