 public enum TipDogadjaja
    {
        Bitka,
        Rat,
        Ustanak,
        Sporazum,
        Savez,
        Dokument,
        Opsada
    }
public class DogadjajNeo
{
    public Guid ID { get; set; }
    public TipDogadjaja Tip { get; set; }
    public required string Ime { get; set; }//unique
    public GodinaNeo? Godina { get; set; }//mozda nekad null sad ne
    public string? Lokacija { get; set; }//STAVI VEZU SA ZEMLJOM
                                         //public Lokacija? Lokacija { get; set; }
                                         //public List<Zemlja> Ucesnici { get; set; } = new List<Zemlja>();
}