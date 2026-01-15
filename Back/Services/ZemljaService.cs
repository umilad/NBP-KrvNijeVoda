using Neo4jClient;
//using KrvNijeVoda.Back.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http.HttpResults;
public class ZemljaService
{
    private readonly IGraphClient _client;

    public ZemljaService(Neo4jService neo4jService)
    {
        _client = neo4jService.GetClient();
    }

}
