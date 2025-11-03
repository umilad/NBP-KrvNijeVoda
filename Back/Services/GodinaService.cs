using Neo4jClient;
//using KrvNijeVoda.Back.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http.HttpResults;
public class GodinaService
{
    private readonly IGraphClient _client;

    public GodinaService(Neo4jService neo4jService)
    {
        _client = neo4jService.GetClient();
    }

    //MORA DA SE DODA ZA P.N.E. ----dodav treba da se testira

    public async Task<GodinaNeo> DodajGodinu(int go, bool pne)
    {
        var ng = (await _client.Cypher.Match("(g:Godina)")
                                      .Where((GodinaNeo g) => g.God == go && g.IsPNE == pne)
                                      .Return(g => g.As<GodinaNeo>())
                                      .ResultsAsync)
                                      .FirstOrDefault();
        if (ng == null)
        {
            var god = new GodinaNeo
            {
                ID = Guid.NewGuid(),
                God = go,
                IsPNE = pne
            };
            ng = (await _client.Cypher.Create("(g:Godina $godina)")
                            .WithParam("godina", god)
                            .Return(g => g.As<GodinaNeo>())
                            .ResultsAsync)
                            .FirstOrDefault();
        }
        return ng!;
    }

//NIKAKO NE MOZE DA SE NAPRAVI HILJADU PARAMETARA BI IMALO I NI TO NE BI BILO DOVOLJNO 
    // public async Task<IActionResult> GodinaUpdate(int stara, int nova, bool pneS, bool pneN)
    // {
    //     if (nova != 0)//uneta godina
    //         {
    //             if (stara != 0)//postoji vec neka godina
    //             {
    //                 if (stara != nova || pneS != pneN)
    //                 {//promenjena je 
    //                     await _godinaService.DodajGodinu(nova, pneN);
    //                     query = query.With("l")
    //                                  .Match("(l)-[r1:UMRO]->(sgs:Godina)")
    //                                  .Match("(g2:Godina {God: $gods, IsPNE: $pnes})")
    //                                  .WithParam("gods", licnost.GodinaSmrti)
    //                                  .WithParam("pnes", licnost.GodinaSmrtiPNE)
    //                                  .Delete("r1")
    //                                  .Create("(l)-[:UMRO]->(g2)")
    //                                  .Set("l.GodinaSmrti = $gods, l.GodinaSmrtiPNE = $pnes");

    //                 }
    //                 //else ista je godina ne radi se nista 
    //             }
    //             else
    //             {
    //                 //ne postoji godina samo unosimo novu 
    //                 await _godinaService.DodajGodinu(licnost.GodinaSmrti, licnost.GodinaSmrtiPNE);
    //                 query = query.With("l")
    //                              .Match("(g2:Godina {God: $gods, IsPNE: $pnes})")
    //                              .WithParam("gods", licnost.GodinaSmrti)
    //                              .WithParam("pnes", licnost.GodinaSmrtiPNE)
    //                              .Create("(l)-[:UMRO]->(g2)")
    //                              .Set("l.GodinaSmrti = $gods, l.GodinaSmrtiPNE = $pnes");
    //             }

    //         }
    // }
}
