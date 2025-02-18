namespace KrvNijeVoda.Back.Models
{ 
//dodati enum za p.n.e.
    public class Godina {
        public Guid ID { get; set; }
        
        public required int God { get; set; }//UNIQUE
        //listaj dogadjaje
    }
}
