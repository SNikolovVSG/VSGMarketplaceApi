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
            await this.itemsService.AddAsync(item);

            return Ok();
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpPut("~/Inventory/Modify/{id}")]
        public async Task<IActionResult> Update([FromRoute] int id, [FromForm] ItemAddModelWithFormFile item)
        {
            await itemsService.UpdateAsync(item, id);

            return Ok();
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpDelete("~/DeleteItem/{id}")]
        public async Task<IActionResult> DeleteAsync([FromRoute] int id)
        {
            await this.itemsService.DeleteAsync(id);

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

        [HttpGet("~/Marketplace/{id}")]
        public async Task<ActionResult<MarketplaceByIdItemViewModel>> ById([FromRoute] int id)
        {
            var item = await this.itemsService.GetMarketplaceItemAsync(id);

            return Ok(item);
        }
    }
}
