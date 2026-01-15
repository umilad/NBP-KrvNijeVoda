using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
public class LicnostMongo
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid ID { get; set; }
    public string? Slika { get; set; }

    public string? Tekst { get; set; } 
}