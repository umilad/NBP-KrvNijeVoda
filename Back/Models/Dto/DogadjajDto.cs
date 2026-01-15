using System.Text.Json.Serialization;

    public class DogadjajDto
    {
        public Guid ID { get; set; }
         [JsonConverter(typeof(JsonStringEnumConverter))]
        public TipDogadjaja Tip { get; set; }
        public required string Ime { get; set; }
        public GodinaNeo? Godina { get; set; } 
        public string? Lokacija { get; set; } 
        public string? Tekst { get; set; } 
    }

