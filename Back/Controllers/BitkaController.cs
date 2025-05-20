using Microsoft.AspNetCore.Mvc;
using Neo4jClient;
using KrvNijeVoda.Back.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

[Route("api")]
[ApiController]
public class BitkaController : ControllerBase
{
    private readonly IGraphClient _client;
    // private readonly GodinaService _godinaService;
    // private readonly LokacijaService _lokacijaService;
    // private readonly ZemljaService _zemljaService;
    // private readonly RatService _ratService;

    public BitkaController(Neo4jService neo4jService/*, GodinaService godinaService, LokacijaService lokacijaService, RatService ratService, ZemljaService zemljaService*/)
    {
        _client = neo4jService.GetClient();
        // _godinaService = godinaService;
        // _lokacijaService = lokacijaService;
        // _ratService = ratService;
        // _zemljaService=zemljaService;
    }



}
