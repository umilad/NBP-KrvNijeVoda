using KrvNijeVoda.Models;

namespace KrvNijeVoda.Back.Models{ 
    public class Bitka : Dogadjaj {
        public string Pobednik { get; set; }
        public Godina? GodinaDo { get; set; }
        public Rat? Rat { get; set; }

    }
}