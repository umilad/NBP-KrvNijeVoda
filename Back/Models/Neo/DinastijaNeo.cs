public class DinastijaNeo
    {
        public Guid ID { get; set; }
        public required string Naziv { get; set; }

        public int PocetakVladavineGod { get; set; }
        public bool PocetakVladavinePNE { get; set; } = false;
        public int KrajVladavineGod { get; set; }
        public bool KrajVladavinePNE { get; set; } = false;
    }