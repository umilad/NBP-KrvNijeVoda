using KrvNijeVoda.Models;

namespace KrvNijeVoda.Back.Models{ 

    public enum TipDogadjaja {
        Bitka,
        Rat,
        Sporazum,
        Savez,
        Dokument
    }
    public class Dogadjaj {
        public Guid ID { get; set; }
        public string Ime { get; set; }
        public TipDogadjaja Tip { get; set; }
        public Godina Godina { get; set; }
        public Mesto Lokacija { get; set; }
        public List<Zemlja> Ucesnici { get; set; } = new List<Zemlja>();
        public string Tekst { get; set; }

    }
    
}