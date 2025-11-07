using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class UserMongo
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    public string Username { get; set; }
   // public string Email { get; set; }
    public string PasswordHash { get; set; }
    public string Role { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
}
