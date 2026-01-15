public class LicnostDto
{
    public Guid ID { get; set; }
    public string Titula { get; set; }
    public string Ime { get; set; }
    public string Prezime { get; set; }

    public int GodinaRodjenja { get; set; }
    public bool GodinaRodjenjaPNE { get; set; } = false;
    public int GodinaSmrti { get; set; }
    public bool GodinaSmrtiPNE { get; set; } = false;
    public string Pol { get; set; }
    public string? MestoRodjenja { get; set; }
    public string? Slika { get; set; }

    public string? Tekst { get; set; }
}

