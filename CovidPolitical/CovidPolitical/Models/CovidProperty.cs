namespace CovidPolitical.Models
{
    public class CovidProperty
    {
        public string FIPS { get; set; }
        public int Confirmed { get; set; }
        public int Recovered { get; set; }
        public int Deaths { get; set; }
        public int Active { get; set; }
        public int Population { get; set; }
        public int ConfirmedPer100k { get; set; }
        public int RecoveredPer100k { get; set; }
        public int DeathsPer100k { get; set; }
        public int ActivePer100k { get; set; }
    }
}
