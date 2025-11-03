
    public class DogadjajDto
    {
        public Guid? ID { get; set; } // nullable kod kreiranja
        public TipDogadjaja Tip { get; set; }
        public required string Ime { get; set; }
        public GodinaNeo? Godina { get; set; }  // veza ka čvoru Godina u Neo4j
        public string? Lokacija { get; set; } // ime zemlje / reference name
        public string? Tekst { get; set; } // opis, ide u Mongo
    }

