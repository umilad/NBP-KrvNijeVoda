using System.Text.Json.Serialization;

    public class DogadjajTimelineDto
    {
        public Guid ID { get; set; }
         [JsonConverter(typeof(JsonStringEnumConverter))]
        public required string Ime { get; set; }
    }
