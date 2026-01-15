using MongoDB.Driver;

public class MongoService
{
    private readonly IMongoDatabase _db;

    public MongoService(string connectionString, string dbName)
    {
        var client = new MongoClient(connectionString);
        _db = client.GetDatabase(dbName);
    }

    public IMongoCollection<T> GetCollection<T>(string name) =>
        _db.GetCollection<T>(name);

    public IMongoCollection<UserMongo> Users => _db.GetCollection<UserMongo>("Users");
}
