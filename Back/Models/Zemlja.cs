namespace KrvNijeVoda.Back.Models{ 
 
    public class Zemlja {
        public Guid ID { get; set; }
        public string Naziv { get; set; }
        public string? Trajanje { get; set; }
        public string? Grb { get; set; }//slika
    }
}