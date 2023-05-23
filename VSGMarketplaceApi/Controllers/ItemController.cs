using Microsoft.AspNetCore.Mvc;
using Data.Models;
using Services.Interfaces;
using Data.ViewModels;
using Microsoft.AspNetCore.Authorization;

namespace VSGMarketplaceApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ItemController : ControllerBase
    {
        private readonly IItemsService itemsService;
        
        public ItemController(IItemsService itemsService)
        {
            this.itemsService = itemsService;
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpPost("~/Inventory/AddItem")]
        public async Task<IActionResult> AddAsync([FromForm] ItemAddModelString item)
        {
            var isAdmin = HttpContext.User.Claims.Any(x => x.Value == "f2123818-3d51-4fe4-990b-b072a80da143");
            if (!isAdmin) { return BadRequest("You don't have permission to do this!"); }

            string result = await this.itemsService.AddAsync(item);

            if (result != Constants.Ok)
            {
                return BadRequest(result);
            }

            return Ok();
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpPut("~/Inventory/Modify/{code}")]
        public async Task<IActionResult> Edit([FromRoute] int code, [FromForm] ItemAddModelString item)
        {
            string result = await itemsService.UpdateAsync(item, code);
            if (result != Constants.Ok)
            {
                return BadRequest(result);
            }

            return Ok();
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpDelete("~/DeleteItem/{code}")]
        public async Task<IActionResult> DeleteAsync([FromRoute] int code)
        {
            string result = await this.itemsService.DeleteAsync(code);
            if (result != Constants.Ok)
            {
                return BadRequest(result);
            }

            return Ok();
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpGet("~/Inventory")]
        public async Task<ActionResult<List<InventoryItemViewModel>>> Inventory()
        {
            var items = await this.itemsService.GetInventoryItemsAsync();
            return Ok(items);
        }

        [HttpGet("~/")]
        public async Task<ActionResult<List<MarketplaceItemViewModel>>> MarketplaceAsync()
        {
            var items = await this.itemsService.GetMarketplaceItemsAsync();
            return Ok(items);
        }

        [HttpGet("~/Marketplace/{code}")]
        public async Task<ActionResult<MarketplaceByIdItemViewModel>> ById([FromRoute] int code)
        {
            var item = await this.itemsService.GetMarketplaceItemAsync(code);
            return Ok(item);
        }
    }
}
