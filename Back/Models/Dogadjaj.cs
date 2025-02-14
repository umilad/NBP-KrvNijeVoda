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
        public Lokacija Lokacija { get; set; }
        //public List<Zemlja> Ucesnici { get; set; } = new List<Zemlja>();
        public string Tekst { get; set; }
        //ne mora da ga nasledi rat nego samo da ima nullable GOdina do i taman bitka ne mora to da ima
        //bolje da nasledi da ne bude mng null propertya i zbog prosirljivosti
    }
    
}