public class FamilyNodeDto
{
    public LicnostTreeDto? Otac { get; set; }
    public LicnostTreeDto? Majka { get; set; }

    public List<LicnostTreeDto> Deca { get; set; } = new();
}
