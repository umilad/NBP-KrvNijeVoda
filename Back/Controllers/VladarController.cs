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
    // private readonly GodinaService _godinaService;
    // private readonly LokacijaService _lokacijaService;
    // private readonly ZemljaService _zemljaService;
    // private readonly DinastijaService _dinastijaService;

    public VladarController(Neo4jService neo4jService/*, GodinaService godinaService, LokacijaService lokacijaService, ZemljaService zemljaService, DinastijaService dinastijaService*/)
    {
        _client = neo4jService.GetClient();  
        // _godinaService = godinaService;
        // _lokacijaService = lokacijaService;
        // _zemljaService = zemljaService;
        // _dinastijaService = dinastijaService;
    }

}