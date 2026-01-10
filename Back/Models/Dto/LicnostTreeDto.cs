public class LicnostTreeDto : LicnostDto
{
    //public List<FamilyNodeDto> Porodice { get; set; } = new();
    public List<LicnostTreeDto> Deca { get; set; } = new();
}