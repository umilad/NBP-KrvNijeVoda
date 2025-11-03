public class VladarNeo : LicnostNeo
    {
        public DinastijaNeo? Dinastija { get; set; }
        public int PocetakVladavineGod { get; set; }
        public bool PocetakVladavinePNE { get; set; } = false;
        public int KrajVladavineGod { get; set; }
        public bool KrajVladavinePNE { get; set; } = false;
        //VEZE OSTAJU 
        // public Godina PocetakVladavine  { get; set; }
        // public Godina KrajVladavine  { get; set; }
        //public List<Licnost> Deca { get; set; } = new List<Licnost>();
        //public List<Licnost> Supruznici { get; set; } = new List<Licnost>();
        //public List<Dogadjaj> Dogadjaji { get; set; } = new List<Dogadjaj>();
    }