using KrvNijeVoda.Back.Models;
using KrvNijeVoda.Back.Helpers;

namespace KrvNijeVoda.Models 
{//TREBA DA DODAMO VEZE ZA DECU I ZA SUPRUZNIKE
    public class Licnost {
        public Guid ID { get; set;}
        //titula ime prezime zajedno moraju da budu unique ako ne znamo koji su "/"
        public string Titula { get; set;}
        public string Ime { get; set; }
        public string Prezime { get; set; }
        
        public int? GodinaRodjenja { get; set; }
        public bool? GodinaRodjenjaPNE { get; set; } = false;
        public int? GodinaSmrti { get; set; }
        public bool? GodinaSmrtiPNE { get; set; } = false;
        //VEZE POSTOJE SA GODINAMA
        public string Pol { get; set; }
        public string? Slika { get; set; }//na osnovu pola moze da stavlja one prazne slike kao na fb
        public string? MestoRodjenja { get; set; }
        //IMA VEZU SA LOKACIJOM
    }

}