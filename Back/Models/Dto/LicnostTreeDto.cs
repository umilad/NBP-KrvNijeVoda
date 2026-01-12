public class LicnostTreeDto : LicnostFlatDto//LicnostDto 
//promenila si da nasledjuje LicnostFlatDto da bi dobila RoditeljiID i DecaID na frontu vidi sta ti treba pa ispravi i dodaj samo to 
{
    //public List<FamilyNodeDto> Porodice { get; set; } = new();
    public List<LicnostTreeDto> Deca { get; set; } = new();
    public List<LicnostTreeDto> Supruznici { get; set; } = new();
    //public List<Guid> SupruzniciID { get; set; } = new();
}