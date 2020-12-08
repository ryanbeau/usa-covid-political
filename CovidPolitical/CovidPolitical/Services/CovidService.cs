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

        private async Task<PopulationResponse> GetPopulationAsync()
        {
            // get population
            HttpResponseMessage response = await client.GetAsync("https://api.census.gov/data/2019/pep/population?get=POP&for=county:*");
            string responseString = await response.Content.ReadAsStringAsync();

            // deserialize
            List<List<string>> populationResponse = JsonConvert.DeserializeObject<List<List<string>>>(responseString);

            // TODO : map to -> new PopulationResponse

            return null;
        }

        private void TransformGeojson(PopulationResponse population)
        {
            // TODO: Map population to Geojson property - ie: calculate per100k (covidStat / population * 100k)
        }
    }
}
