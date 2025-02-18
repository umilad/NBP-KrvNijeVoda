using KrvNijeVoda.Back.Models;

namespace KrvNijeVoda.Models 
{//TREBA DA DODAMO VEZE ZA DECU I ZA SUPRUZNIKE
    public class Licnost {
        public Guid ID { get; set;}
        //titula ime prezime zajedno moraju da budu unique ako ne znamo koji su "/"
        public string Titula { get; set;}
        public string Ime { get; set; }
        public string Prezime { get; set; }
        public Godina? GodinaRodjenja { get; set; }
        public Godina? GodinaSmrti { get; set; }
        public string Pol { get; set; }
        public string? Slika { get; set; }//na osnovu pola moze da stavlja one prazne slike kao na fb
        public Lokacija? MestoRodjenja { get; set; }
    }

}