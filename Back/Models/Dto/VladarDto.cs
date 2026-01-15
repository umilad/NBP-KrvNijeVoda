public class VladarDto : LicnostDto
{
    public DinastijaNeo? Dinastija { get; set; }
    public string? Teritorija { get; set; }
    public int PocetakVladavineGod { get; set; }
    public bool PocetakVladavinePNE { get; set; } = false;
    public int KrajVladavineGod { get; set; }
    public bool KrajVladavinePNE { get; set; } = false;
}