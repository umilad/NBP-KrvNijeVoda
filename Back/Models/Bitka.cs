using KrvNijeVoda.Models;

namespace KrvNijeVoda.Back.Models
{ 
    public class Bitka : Dogadjaj {
        public string Pobednik { get; set; }
        public string? Rat { get; set; }//veza sa ratom
        public int BrojZrtava { get; set; }

    }
}

//CREATE CONSTRAINT unique_titula FOR (v:Vladar) REQUIRE v.Titula IS UNIQUE;
