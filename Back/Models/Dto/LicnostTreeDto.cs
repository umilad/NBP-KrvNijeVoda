public class LicnostTreeDto : LicnostDto
{
    public List<Guid> DecaID { get; set; } = new();
    public List<Guid> RoditeljiID { get; set; } = new();

}