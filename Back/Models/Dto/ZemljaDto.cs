public class ZemljaDto
{
    
    public Guid ID { get; set; }
    public required string Naziv { get; set; }
    public string? Trajanje { get; set; }
    public string? Grb { get; set; }
    public int? BrojStanovnika { get; set; }
}