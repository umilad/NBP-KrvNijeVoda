public class DinastijaNeo
    {
        public Guid ID { get; set; }
        public required string Naziv { get; set; }//unique najbolje REQUIRED?

        //public GodinaStruct? PocetakVladavineGod  { get; set; }
        public int PocetakVladavineGod { get; set; }
        public bool PocetakVladavinePNE { get; set; } = false;
        //public PocetakVladavine  { get; set; }//sta ako nisu vladali je l moze to??
        public int KrajVladavineGod { get; set; }//AKO SU NULLABLE MORA DA SE MENJA CREATE
        public bool KrajVladavinePNE { get; set; } = false;
        //VEZE POSTOJE SA GODINAMA
        // public Godina? PocetakVladavine  { get; set; }//sta ako nisu vladali je l moze to??
        // public Godina? KrajVladavine  { get; set; }//AKO SU NULLABLE MORA DA SE MENJA CREATE
        //public List<Licnost> Clanovi { get; set; } = new List<Licnost>();
    }