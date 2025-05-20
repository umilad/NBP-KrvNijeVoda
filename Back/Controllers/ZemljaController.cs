using Microsoft.AspNetCore.Mvc;
using Neo4jClient;
using System;
using System.Threading.Tasks;
using KrvNijeVoda.Back.Models;
using System.Reflection.Metadata;
using KrvNijeVoda.Back;

[Route("api")]
[ApiController]
public class ZemljaController : ControllerBase
{
    private readonly IGraphClient _client;

    public ZemljaController(Neo4jService neo4jService)
    {
        _client = neo4jService.GetClient();  
    }

}