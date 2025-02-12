namespace KrvNijeVoda.Back.Models{ 
 
    public class Zemlja : Lokacija {
        public Guid ID { get; set; }
        public string Trajanje { get; set; }
        public string Grb { get; set; }
    }
}