using KrvNijeVoda.Models;

namespace KrvNijeVoda.Back.Models{ 

    public class Rat : Dogadjaj {
        public Godina GodinaDo { get; set; }
        public List<Zemlja> Pobednik { get; set; } = new List<Zemlja>();
    }
}