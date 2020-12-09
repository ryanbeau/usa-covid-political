using CovidPolitical.Models;
using CovidPolitical.Models.Geojson;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace CovidPolitical.Services
{
    public class CovidService
    {
        private static readonly HttpClient client = new HttpClient();

        private FeatureCollection Geojson { get; set; }
        private DateTime LastUpdated { get; set; }
        public TimeSpan MinimumUpdateInterval { get; set; }

        public async Task<FeatureCollection> GetGeojsonAsync()
        {
            DateTime now = DateTime.Now;

            // check the cached the results
            if (Geojson != null && LastUpdated.Subtract(MinimumUpdateInterval) < now)
            {
                return Geojson;
            }

            // get covid & population
            var covidTask = GetCovidGeojsonAsync();
            var populationTask = GetPopulationAsync();

            // simultaneously await the http request from both covid & population
            await Task.WhenAll(covidTask, populationTask);

            // transform
            Geojson = covidTask.Result;
            TransformGeojson(populationTask.Result);

            LastUpdated = now;

            return Geojson;
        }

        private async Task<FeatureCollection> GetCovidGeojsonAsync()
        {
            // get geojson
            HttpResponseMessage response = await client.GetAsync("https://opendata.arcgis.com/datasets/628578697fb24d8ea4c32fa0c5ae1843_0.geojson");
            string responseString = await response.Content.ReadAsStringAsync();

            // deserialize
            var geojson = JsonConvert.DeserializeObject<FeatureCollection>(responseString);

            return geojson;
        }

        private async Task<List<List<string>>> GetPopulationAsync()
        {
            // get population
            HttpResponseMessage response = await client.GetAsync("https://api.census.gov/data/2019/pep/population?get=POP&for=county:*");
            string responseString = await response.Content.ReadAsStringAsync();

            // deserialize
            List<List<string>> populationResponse = JsonConvert.DeserializeObject<List<List<string>>>(responseString);
            populationResponse.RemoveAt(0);

            return populationResponse;
        }

        private void TransformGeojson(List<List<string>> populationResponse)
        {
            Dictionary<string, CovidProperty> fipsCounty = new Dictionary<string, CovidProperty>();
            foreach (var feature in Geojson.Features)
            {
                if (feature.Properties.FIPS != null) 
                { 
                    fipsCounty[feature.Properties.FIPS] = feature.Properties;
                }
            }

            foreach (var population in populationResponse)
            {
                string fips = population[1] + population[2];
                if (fipsCounty.TryGetValue(fips, out CovidProperty property))
                {
                    property.Population = int.Parse(population[0]);
                    property.ActivePer100k = (int)((decimal)property.Active / property.Population * 100000m);
                    property.ConfirmedPer100k = (int)((decimal)property.Confirmed / property.Population * 100000m);
                    property.DeathsPer100k = (int)((decimal)property.Deaths / property.Population * 100000m);
                    property.RecoveredPer100k = (int)((decimal)property.Recovered / property.Population * 100000m);
                }
            }
        }
    }
}
