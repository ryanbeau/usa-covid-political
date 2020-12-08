using CovidPolitical.Models.Geojson;
using CovidPolitical.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace CovidPolitical.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CovidController : ControllerBase
    {
        private readonly CovidService _covidService;

        public CovidController(CovidService covidService)
        {
            _covidService = covidService;
        }

        [HttpGet]
        public async Task<FeatureCollection> Geojson()
        {
            return await _covidService.GetGeojsonAsync();
        }
    }
}
