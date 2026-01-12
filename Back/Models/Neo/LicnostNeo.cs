public class LicnostNeo
{
    public Guid ID { get; set; }
    //titula ime prezime zajedno moraju da budu unique ako ne znamo koji su "/"
    public required string Titula { get; set; }
    public required string Ime { get; set; }
    public required string Prezime { get; set; }

    public int GodinaRodjenja { get; set; }
    public bool GodinaRodjenjaPNE { get; set; } = false;
    public int GodinaSmrti { get; set; }
    public bool GodinaSmrtiPNE { get; set; } = false;
    //VEZE POSTOJE SA GODINAMA
    public required string Pol { get; set; }
    public string? MestoRodjenja { get; set; }
}