using System.Collections.Generic;

namespace CovidPolitical.Models
{
    public class PopulationResponse
    {
        public List<CountyPopulation> Counties { get; set; }
    }

    public class CountyPopulation
    {
        public string FIPS { get; set; }
        public int Population { get; set; }
    }
}
