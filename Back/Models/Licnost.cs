using KrvNijeVoda.Back.Models;

namespace KrvNijeVoda.Models { 

public class Licnost {
    public Guid ID { get; set;}
    public string Titula { get; set;}
    public string Ime { get; set; }
    public string Prezime { get; set; }
    public Godina GodinaRodjenja { get; set; }
    public Godina GodinaSmrti { get; set; }
    public string Pol { get; set; }
    public string Slika { get; set; }
    public Mesto MestoRodjenja { get; set; }
}

}