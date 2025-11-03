using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
public class LicnostMongo
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid ID { get; set; }
    public string? Slika { get; set; }//na osnovu pola moze da stavlja one prazne slike kao na fb, MONGO!!

    public string? Tekst { get; set; } //MONGO, BIOGRAFIJA
}