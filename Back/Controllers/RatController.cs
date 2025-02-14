using Microsoft.AspNetCore.Mvc;
using Neo4jClient;
using System;
using System.Threading.Tasks;
using KrvNijeVoda.Back.Models;
using System.Reflection.Metadata;
using KrvNijeVoda.Back;
[Route("api")]
[ApiController]
public class RatController : ControllerBase
{
    private readonly IGraphClient _client;

    public RatController(Neo4jService neo4jService)
    {
        _client = neo4jService.GetClient();  
    }


}