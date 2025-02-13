using Neo4jClient;
using KrvNijeVoda.Back.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http.HttpResults;
using Neo4jClient.Cypher;
using Neo4j.Driver;
public class MestoService
{
    private readonly IGraphClient _client;
    private readonly ZemljaService _zemljaService;

    public MestoService(Neo4jService neo4jService, ZemljaService zemljaService)
    {
        _client = neo4jService.GetClient();
        _zemljaService = zemljaService;

    }

    public async Task<Mesto> DodajMesto(string nazivMesta, Zemlja zem)
    {     
        var nm = (await _client.Cypher.Match("(lm:Lokacija:Mesto)-[:PRIPADA_ZEMLJI]->(z:Zemlja)") // Match with relationship
                                      .Where("lm.Naziv = $nazivMesta AND z.Naziv = $nazivZemlje") 
                                      .WithParam("nazivMesta", nazivMesta)
                                      .WithParam("nazivZemlje", zem?.Naziv) 
                                      .Return(lm => lm.As<Mesto>())
                                      .ResultsAsync)
                                      .FirstOrDefault();
        if(nm == null)
        {
            //kreiraj ga sa sve zemljom
            nm = (await _client.Cypher.Match("(z:Zemlja {Naziv: $nazivZemlje})")
                            .Create("(lm:Lokacija:Mesto {ID: $id, Naziv: $naziv}) -[:PRIPADA_ZEMLJI]->(z)")
                            .WithParam("naziv", nazivMesta)
                            .WithParam("id", Guid.NewGuid())
                            .WithParam("nazivZemlje", zem?.Naziv)                           
                            .Return(lm => lm.As<Mesto>())
                            .ResultsAsync)
                            .FirstOrDefault();
        }
        return nm;
    }


    
        //POPRAVI
        // if(m.PripadaZemlji != null)
        // {
        //     var nz = await _zemljaService.DodajZemlju(m.PripadaZemlji);
        // }
        //ako je to mesto nadjeno nadjeno je po ID 
        //znaci ili je imalo neku zemlju i to mora da bude ta ista koja je prosledjena 
        //ili nije imao zemlju i dodaje mu se ta prosledjena 
        //znaci moram da saljem mesto samo po imenu i po imenu da ga trazim da bih mogla da mu dodajem zemlju kako sam zamislila
        
        
        // if(m.PripadaZemlji != null)
        // {
        //     var nz = await _zemljaService.DodajZemlju(m.PripadaZemlji);
        //     if(nm != null && nm.Zemlja != null)
        //     {                
        //         if(nz.ID != nm.Zemlja.ID)
        //         {
        //             //napravi novo mesto kojr pripada toj zemlji
        //             var novoM = (await _client.Cypher.Match("(lm:Lokacija:Mesto)")
        //                               .Where((Mesto lm) => lm.ID == m.ID) 
        //                               .OptionalMatch("(lm)-[PRIPADA_ZEMLJI]->(z:Zemlja)")
        //                               .Return(lm => lm.As<Mesto>())
        //                               .Return((lm, z)=> new {
        //                                 Mesto = lm.As<Mesto>(),
        //                                 Zemlja = z.As<Zemlja>()
        //                               })
        //                               .ResultsAsync)
        //                               .FirstOrDefault();
        //         }
        //     }
        // }
        
        



    // public async Task<Mesto> DodajMesto(Mesto m)
    // {    
    //     //POPRAVI
    //     var nm = (await _client.Cypher.Match("(lm:Lokacija:Mesto)")
    //                                   .Where((Mesto lm) => lm.ID == m.ID) 
    //                                   .OptionalMatch("(lm)-[PRIPADA_ZEMLJI]->(z:Zemlja)")
    //                                   .Return(lm => lm.As<Mesto>())
    //                                 //   .Return((lm, z)=> new {
    //                                 //     Mesto = lm.As<Mesto>(),
    //                                 //     Zemlja = z.As<Zemlja>()
    //                                 //   })
    //                                   .ResultsAsync)
    //                                   .FirstOrDefault();
    //     // if(nm.Zemlja != null)
    //     // {
    //     //     if(m.PripadaZemlji != null)
    //     //     {
    //     //         if(m.PripadaZemlji == nm.Zemlja)
    //     //         //STA AKO NE POSTOJI
    //     //     }
    //     //     //ako obe postoje a razlicite su treba da se dodaje novo mesto
    //     //     //vraceno null a ovo nije nal radi query
    //     // }
    //     if(nm == null)
    //     {
    //         //dodaj zemlju
    //         nm = (await _client.Cypher.Create("(lm:Lokacija:Mesto {ID: $id, Naziv: $naziv})")
    //                         .WithParam("naziv", m.Naziv)
    //                         .WithParam("id", Guid.NewGuid())
    //                         .Return(lm => lm.As<Mesto>())//ne koristim ga  koj ce mi??
    //                         .ResultsAsync)
    //                         .FirstOrDefault();
    //     }
    //     return nm;
    // }
    
}