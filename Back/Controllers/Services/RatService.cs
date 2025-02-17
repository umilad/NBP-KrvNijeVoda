using Neo4jClient;
using KrvNijeVoda.Back.Models;
using System;
using System.Threading.Tasks;

public class RatService
{
    private readonly IGraphClient _client;
    private readonly GodinaService _godinaService;
    private readonly LokacijaService _lokacijaService;
    private readonly ZemljaService _zemljaService;

    public RatService(Neo4jService neo4jService, GodinaService godinaService, LokacijaService lokacijaService, ZemljaService zemljaService)
    {
        _client = neo4jService.GetClient();
        _godinaService = godinaService;
        _lokacijaService = lokacijaService;
        _zemljaService= zemljaService;
    }

    public async Task<Rat> DodajRat(Rat rat, Lokacija lokacija1)
    {
        // Proveri da li rat već postoji po imenu
        var postojiRat = (await _client.Cypher
        .Match("(r:Rat)")
        .Where((Rat r) => r.Ime == rat.Ime)
        .Return(r => r.As<Rat>())
        .ResultsAsync)
        .FirstOrDefault();
    if (postojiRat==null)
    {
        await _godinaService.DodajGodinu(rat.Godina.God);
        await _godinaService.DodajGodinu(rat.GodinaDo.God);
        await _zemljaService.DodajZemlju(rat.Lokacija.PripadaZemlji);
        var lokacija = await _lokacijaService.DodajLokaciju(rat.Lokacija.Naziv, rat.Lokacija.PripadaZemlji);
        
        var ratID = Guid.NewGuid();
        postojiRat = (await _client.Cypher   
            .Match("(g:Godina {God: $godina})", "(gd:Godina {God: $godinaDo})", "(l:Lokacija {ID: $idLokacije})")
            .Create("(r:Dogadjaj:Rat {ID: $id, Ime: $ime, Tip: 'Rat', Tekst: $tekst}) -[:DESIO_SE]-> (g), " +
                    "(r) -[:RAT_TRAJAO_DO]-> (gd), (r) -[:DESIO_SE_U]-> (l)")
            .WithParam("id", ratID)
            .WithParam("ime", rat.Ime)
            .WithParam("tekst", rat.Tekst)
            .WithParam("godina", rat.Godina.God)
            .WithParam("godinaDo", rat.GodinaDo.God)
            .WithParam("idLokacije", lokacija.ID)
            .Return(r => r.As<Rat>())
                            .ResultsAsync)
                            .FirstOrDefault();
    }

        return postojiRat;
    }
}
