using Microsoft.AspNetCore.Mvc;
using Data.Models;
using Services.Interfaces;
using Data.ViewModels;
using Microsoft.AspNetCore.Authorization;

namespace VSGMarketplaceApi.Controllers
{
    [Authorize]
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
        public async Task<IActionResult> AddAsync([FromForm] ItemAddModelWithFormFile item)
        {
            string result = await this.itemsService.AddAsync(item);

            //unnecessary
            if (result != Constants.Ok)
            {
                return BadRequest(result);
            }

            return Ok();
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpPut("~/Inventory/Modify/{code}")]
        public async Task<IActionResult> Edit([FromRoute] int code, [FromForm] ItemAddModelWithFormFile item)
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

        [HttpGet("~/Marketplace")]
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
