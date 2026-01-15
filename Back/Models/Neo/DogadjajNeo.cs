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
    public required string Ime { get; set; }
    public GodinaNeo? Godina { get; set; }
    public string? Lokacija { get; set; }
}