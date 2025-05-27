using KrvNijeVoda.Models;

namespace KrvNijeVoda.Back.Models
{ 
    public class Rat : Dogadjaj {
        public Godina? GodinaDo { get; set; }
        public List<string> Bitke { get; set; } = new List<string>();
        //veze sa Bitkama
        public string Pobednik { get; set; }

    }
}