using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
public class DinastijaMongo
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid ID { get; set; }
    public string? Slika { get; set; } //mongo
    //public List<Licnost> Clanovi { get; set; } = new List<Licnost>();

}