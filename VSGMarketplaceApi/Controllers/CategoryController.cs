using Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace VSGMarketplaceApi.Controllers
{

    [Authorize]
    [ApiController]
    public class CategoryController : Controller
    {

        [HttpGet]
        [Route("/GetCategories")]
        public string[] GetCategories()
        {
            return Constants.ItemCategories;
        }

    }
}
