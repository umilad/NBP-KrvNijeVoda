public class LicnostTreeDto : LicnostDto
{
    public List<LicnostNeo> Deca { get; set; } = new();
    public List<Guid> RoditeljiID { get; set; } = new();

}