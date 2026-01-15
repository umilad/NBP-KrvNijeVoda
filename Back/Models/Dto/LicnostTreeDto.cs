public class LicnostTreeDto : LicnostFlatDto
{
    public List<LicnostTreeDto> Deca { get; set; } = new();
    public List<LicnostTreeDto> Supruznici { get; set; } = new();
}