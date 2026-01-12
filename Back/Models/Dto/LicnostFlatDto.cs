public class LicnostFlatDto : LicnostDto
{
    public List<Guid> DecaID { get; set; } = new();
    public List<Guid> RoditeljiID { get; set; } = new();    
    public List<Guid> SupruzniciID { get; set; } = new();
}

