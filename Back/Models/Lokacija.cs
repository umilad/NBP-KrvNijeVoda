namespace KrvNijeVoda.Back.Models{ 

    public class Lokacija
    {        
        public Guid ID { get; set; }
        public string Naziv { get; set; }//UNIQUE sa Zemljom 
        public Zemlja PripadaZemlji { get; set; }//da zemlja mora da postoji a da moze da postoji isti naziv mesta u razl zemljama
    }
}