using System.Collections.Generic;

namespace CovidPolitical.Models.Geojson
{
    public class GeometryPoint
    {
        public string Type { get; set; } = "Point";
        public List<decimal> Coordinates { get; set; }

        public GeometryPoint()
        {

        }

        public GeometryPoint(decimal longitude, decimal latitude)
        {
            Coordinates = new List<decimal> { longitude, latitude };
        }
    }
}
