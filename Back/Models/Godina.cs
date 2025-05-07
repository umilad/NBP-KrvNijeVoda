namespace KrvNijeVoda.Back.Models
{ 
    public class Godina {
        public Guid ID { get; set; }
        public required int God { get; set; }//UNIQUE
        public bool IsPNE { get; set; } = false;
        //listaj dogadjaje
    }
}
