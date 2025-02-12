using KrvNijeVoda.Models;

namespace KrvNijeVoda.Back.Models{ 
    public class Dinastija  {
        public Guid ID { get; set; }
        public string Naziv { get; set; }
        public Godina PocetakVladavine  { get; set; }
        public Godina KrajVladavine  { get; set; }
        public string? Slika { get; set; }
        public List<Licnost> Clanovi { get; set; } = new List<Licnost>();
        
    }
}