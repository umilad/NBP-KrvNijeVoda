using Neo4jClient;
//unique da se definise ovde i onda da se handleuje taj error po fjama vidi s chatgpt ako treba
    public class Neo4jService
    {
        private readonly IGraphClient _client;

        public Neo4jService(string uri, string user, string password)
        {
            _client = new BoltGraphClient(new Uri(uri), user, password);
            _client.ConnectAsync().Wait(); // Ensures the connection is established
        }

        public IGraphClient GetClient()
        {
            return _client;
        }
    }




// using Neo4j.Driver;

// public class Neo4jService
// {
//     private readonly IDriver _driver;

//     public Neo4jService(string uri, string user, string password)
//     {
//         _driver = GraphDatabase.Driver(uri, AuthTokens.Basic(user, password));
//     }

//     public IAsyncSession GetSession()
//     {
//         return _driver.AsyncSession();
//     }
// }
