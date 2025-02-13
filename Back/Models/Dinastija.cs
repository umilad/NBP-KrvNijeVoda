using KrvNijeVoda.Models;

namespace KrvNijeVoda.Back.Models{ 
    public class Dinastija  {
        public Guid ID { get; set; }
        public string Naziv { get; set; }
        public Godina PocetakVladavine  { get; set; }//sta ako nisu vladali je l moze to??
        public Godina KrajVladavine  { get; set; }//AKO SU NULLABLE MORA DA SE MENJA CREATE
        public string? Slika { get; set; }
        public List<Licnost> Clanovi { get; set; } = new List<Licnost>();
        
    }
}