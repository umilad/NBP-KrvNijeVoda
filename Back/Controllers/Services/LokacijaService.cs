using Neo4jClient;
using KrvNijeVoda.Back.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http.HttpResults;
using Neo4jClient.Cypher;
using Neo4j.Driver;
public class LokacijaService
{
    private readonly IGraphClient _client;
    private readonly ZemljaService _zemljaService;

    public LokacijaService(Neo4jService neo4jService, ZemljaService zemljaService)
    {
        _client = neo4jService.GetClient();
        _zemljaService = zemljaService;

    }
    public async Task<Lokacija> DodajLokaciju(string nazivMesta, Zemlja zem)
    {     
        var nl = (await _client.Cypher.Match("(l:Lokacija)-[:PRIPADA_ZEMLJI]->(z:Zemlja)") // Match with relationship
                                      .Where("l.Naziv = $nazivMesta AND z.Naziv = $nazivZemlje") 
                                      .WithParam("nazivMesta", nazivMesta)
                                      .WithParam("nazivZemlje", zem?.Naziv) 
                                      .Return(l => l.As<Lokacija>())
                                      .ResultsAsync)
                                      .FirstOrDefault();
        if(nl == null)
        {
            //kreiraj ga sa sve zemljom
            nl = (await _client.Cypher.Match("(z:Zemlja {Naziv: $nazivZemlje})")
                            .Create("(l:Lokacija {ID: $id, Naziv: $naziv}) -[:PRIPADA_ZEMLJI]->(z)")
                            .WithParam("naziv", nazivMesta)
                            .WithParam("id", Guid.NewGuid())
                            .WithParam("nazivZemlje", zem?.Naziv)                           
                            .Return(l => l.As<Lokacija>())
                            .ResultsAsync)
                            .FirstOrDefault();
        }
        return nl;
    }
}
//AKO KREIRA DVA MESTA CHATGPT KAZE OVAKO
// public async Task<Lokacija> DodajLokaciju(string nazivMesta, Zemlja zem)
// {
//     // First, try to match an existing Lokacija connected to the specified Zemlja.
//     var nl = (await _client.Cypher
//         .Match("(l:Lokacija)-[:PRIPADA_ZEMLJI]->(z:Zemlja)") // Match the relationship
//         .Where("l.Naziv = $nazivMesta AND z.Naziv = $nazivZemlje") // Filter by location name and country name
//         .WithParam("nazivMesta", nazivMesta)
//         .WithParam("nazivZemlje", zem.Naziv) // Ensure the Zemlja exists and match its name
//         .Return(l => l.As<Lokacija>()) // Return the Lokacija node
//         .ResultsAsync)
//         .FirstOrDefault(); // If it exists, take the first one, or return null if none.

//     // If no existing Lokacija is found, create a new one with the provided Zemlja.
//     if (nl == null)
//     {
//         // Ensure Zemlja exists first, before creating a new Lokacija.
//         await _zemljaService.DodajZemljuParametri(zem.Naziv, zem.Grb, zem.Trajanje);

//         // Now, create the new Lokacija node and link it to the Zemlja.
//         nl = (await _client.Cypher
//             .Match("(z:Zemlja {Naziv: $nazivZemlje})") // Match the Zemlja node
//             .Merge("(l:Lokacija {ID: $id, Naziv: $naziv})") // Merge to avoid duplicates
//             .Merge("(l)-[:PRIPADA_ZEMLJI]->(z)") // Merge relationship between Lokacija and Zemlja
//             .WithParam("naziv", nazivMesta) // Provide parameters
//             .WithParam("id", Guid.NewGuid()) // Generate a new ID for the Lokacija
//             .WithParam("nazivZemlje", zem.Naziv) // Ensure matching Zemlja
//             .Return(l => l.As<Lokacija>()) // Return the newly created Lokacija
//             .ResultsAsync)
//             .FirstOrDefault(); // Fetch the first result, which should be the created node.

//     }

//     return nl; // Return the found or newly created Lokacija.
// }

    // public async Task<Mesto> DodajMesto(string nazivMesta, Zemlja zem)
    // {     
    //     var nm = (await _client.Cypher.Match("(lm:Lokacija:Mesto)-[:PRIPADA_ZEMLJI]->(z:Zemlja)") // Match with relationship
    //                                   .Where("lm.Naziv = $nazivMesta AND z.Naziv = $nazivZemlje") 
    //                                   .WithParam("nazivMesta", nazivMesta)
    //                                   .WithParam("nazivZemlje", zem?.Naziv) 
    //                                   .Return(lm => lm.As<Mesto>())
    //                                   .ResultsAsync)
    //                                   .FirstOrDefault();
    //     if(nm == null)
    //     {
    //         //kreiraj ga sa sve zemljom
    //         nm = (await _client.Cypher.Match("(z:Zemlja {Naziv: $nazivZemlje})")
    //                         .Create("(lm:Lokacija:Mesto {ID: $id, Naziv: $naziv}) -[:PRIPADA_ZEMLJI]->(z)")
    //                         .WithParam("naziv", nazivMesta)
    //                         .WithParam("id", Guid.NewGuid())
    //                         .WithParam("nazivZemlje", zem?.Naziv)                           
    //                         .Return(lm => lm.As<Mesto>())
    //                         .ResultsAsync)
    //                         .FirstOrDefault();
    //     }
    //     return nm;
    // }
    


    
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
    
//}