public interface IVladarService
{
    Task<List<LicnostFlatDto>> GetVladariFlat(Guid dinastijaId);
}
