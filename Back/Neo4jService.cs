using Neo4jClient;
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

