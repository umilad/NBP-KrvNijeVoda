using KrvNijeVoda.Models;

namespace KrvNijeVoda.Back.Models
{ 
    public class Rat : Dogadjaj {
        public Godina? GodinaDo { get; set; }
        public List<Bitka> Bitke { get; set; } = new List<Bitka>();
        public string Pobednik { get; set; }

    }
}