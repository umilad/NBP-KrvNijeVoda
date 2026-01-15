using Neo4jClient;
//using KrvNijeVoda.Back.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http.HttpResults;
public class DinastijaService
{
    private readonly IGraphClient _client;

    public DinastijaService(Neo4jService neo4jService)
    {
        _client = neo4jService.GetClient();
    }

}
