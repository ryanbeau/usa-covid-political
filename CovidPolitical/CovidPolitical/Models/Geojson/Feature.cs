namespace CovidPolitical.Models.Geojson
{
    public class Feature
    {
        public string Type { get; set; } = "Feature";
        public CovidProperty Properties { get; set; }
        public GeometryPoint Geometry { get; set; }
    }
}
