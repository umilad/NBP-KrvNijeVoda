public interface ILicnostService
{
    Task<List<LicnostFlatDto>> GetLicnostiFlat(Guid dinastijaId);
}

