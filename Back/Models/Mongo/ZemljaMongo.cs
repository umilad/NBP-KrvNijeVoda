using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class ZemljaMongo
{
    [BsonId] 
    [BsonRepresentation(BsonType.String)]
    public Guid ID { get; set; }
    public string? Grb { get; set; }
    public int? BrojStanovnika { get; set; }
}