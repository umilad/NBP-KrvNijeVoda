using KrvNijeVoda.Models;
using KrvNijeVoda.Back.Helpers;

namespace KrvNijeVoda.Back.Models 
{
    public class Vladar : Licnost
    {
        public Dinastija? Dinastija { get; set; }
        public string? Teritorija { get; set; }//slika
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
}