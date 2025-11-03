public class LicnostNeo
{
    public Guid ID { get; set; }
    //titula ime prezime zajedno moraju da budu unique ako ne znamo koji su "/"
    public string Titula { get; set; }
    public string Ime { get; set; }
    public string Prezime { get; set; }

    public int GodinaRodjenja { get; set; }
    public bool GodinaRodjenjaPNE { get; set; } = false;
    public int GodinaSmrti { get; set; }
    public bool GodinaSmrtiPNE { get; set; } = false;
    //VEZE POSTOJE SA GODINAMA
    public string Pol { get; set; }
    public string? MestoRodjenja { get; set; }
}