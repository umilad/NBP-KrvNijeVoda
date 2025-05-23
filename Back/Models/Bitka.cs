using KrvNijeVoda.Models;

namespace KrvNijeVoda.Back.Models
{ 
    public class Bitka : Dogadjaj {
        public required string Pobednik { get; set; }
        public Guid? RatID { get; set; }
        public int BrojZrtava { get; set; }

    }
}

//CREATE CONSTRAINT unique_titula FOR (v:Vladar) REQUIRE v.Titula IS UNIQUE;
