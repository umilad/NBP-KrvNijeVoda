using KrvNijeVoda.Models;

namespace KrvNijeVoda.Back.Models
{ 
    public enum TipDogadjaja {
        Bitka,
        Rat,
        Ustanak, 
        Sporazum,
        Savez,
        Dokument
    }
    public class Dogadjaj {
        public Guid ID { get; set; }
        public string Ime { get; set; }//unique
        public TipDogadjaja Tip { get; set; }
        public Godina? Godina { get; set; }//mozda nekad null sad ne
        public string? Lokacija { get; set; }//STAVI VEZU SA ZEMLJOM
        //public Lokacija? Lokacija { get; set; }
        //public List<Zemlja> Ucesnici { get; set; } = new List<Zemlja>();
        public string? Tekst { get; set; }
        //ne mora da ga nasledi rat nego samo da ima nullable GOdina do i taman bitka ne mora to da ima
        //bolje da nasledi da ne bude mng null propertya i zbog prosirljivosti
    }
    
}