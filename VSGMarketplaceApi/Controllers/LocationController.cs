using Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace VSGMarketplaceApi.Controllers
{

    [Authorize]
    [ApiController]
    public class LocationController : Controller
    {

        [HttpGet]
        [Route("/GetLocations")]
        public string[] GetLocations()
        {
            return Constants.LocationsArray;
        }

    }
}
