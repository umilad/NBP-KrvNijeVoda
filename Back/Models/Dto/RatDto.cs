public class RatDto : DogadjajDto {
        public GodinaNeo? GodinaDo { get; set; }
        public List<string> Bitke { get; set; } = new List<string>();
        //veze sa Bitkama
        public string Pobednik { get; set; }
    }