using System.Collections.Generic;

namespace CovidPolitical.Models.Geojson
{
    public class FeatureCollection
    {
        public FeatureCollection()
        {
            // anonymous type with the expected values for geojson FeatureCollection
            Crs = new
            {
                Type = "name",
                Properties = new
                {
                    Name = "urn:ogc:def:crs:OGC:1.3:CRS84",
                },
            };
        }

        public string Type { get; set; } = "FeatureCollection";
        public object Crs { get; set; }
        public List<Feature> Features { get; set; }
    }
}
