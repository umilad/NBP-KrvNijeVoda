using Neo4jClient;
//using KrvNijeVoda.Back.Models;
using System;
using System.Threading.Tasks;
public class RatService
{
    private readonly IGraphClient _client;

    public RatService(Neo4jService neo4jService)
    {
        _client = neo4jService.GetClient();
    }

}
