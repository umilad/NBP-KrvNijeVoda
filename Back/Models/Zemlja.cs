namespace KrvNijeVoda.Back.Models
{ 
    public class Zemlja {
        public Guid ID { get; set; }
        public required string Naziv { get; set; }// UNIQUE
        public string? Trajanje { get; set; }
        public string? Grb { get; set; }//slika
        public int? BrojStanovnika { get; set; }
    }
}