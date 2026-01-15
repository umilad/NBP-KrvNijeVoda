namespace KrvNijeVoda.Back.Helpers
{
    public struct GodinaStruct
    {
        public int GodS { get; set; }
        public bool PneS { get; set; }

        public GodinaStruct(int god, bool pne = false)
        {
            GodS = god;
            PneS = pne;
        }
    }
}
