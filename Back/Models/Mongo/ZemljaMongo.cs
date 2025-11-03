using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class ZemljaMongo
{
    [BsonId] // ovo označava da je primarni ključ u MongoDB-u (_id)
    [BsonRepresentation(BsonType.String)]
    public Guid ID { get; set; }
    public string? Grb { get; set; }
    public int? BrojStanovnika { get; set; }
}