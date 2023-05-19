using Microsoft.AspNetCore.Mvc;
using Data.Models;
using Services.Interfaces;
using Data.ViewModels;

namespace VSGMarketplaceApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ItemController : ControllerBase
    {
        private readonly IItemsService itemsService;
        
        public ItemController(IItemsService itemsService)
        {
            this.itemsService = itemsService;
        }

        //[Authorize(Roles = "Administrator")]
        [HttpPost("~/Inventory/AddItem")]
        public async Task<IActionResult> AddAsync([FromForm] ItemAddModelString item)
        {
            string result = await this.itemsService.AddAsync(item);

            if (result != Constants.Ok)
            {
                return BadRequest(result);
            }

            return Ok();
        }

        //[Authorize(Roles = "Administrator")]
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

        //[Authorize(Roles = "Administrator")]
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

        //[Authorize(Roles = "Administrator")]
        [HttpGet("~/Inventory")]
        public async Task<ActionResult<List<InventoryItemViewModel>>> Inventory()
        {
            var items = await this.itemsService.GetInventoryItemsAsync();
            return Ok(items);
        }

        //[Authorize]
        [HttpGet("~/Marketplace")]
        public async Task<ActionResult<List<MarketplaceItemViewModel>>> MarketplaceAsync()
        {
            var items = await this.itemsService.GetMarketplaceItemsAsync();
            return Ok(items);
        }

        //[Authorize]
        [HttpGet("~/Marketplace/{code}")]
        public async Task<ActionResult<MarketplaceByIdItemViewModel>> ById([FromRoute] int code)
        {
            var item = await this.itemsService.GetMarketplaceItemAsync(code);
            return Ok(item);
        }
    }
}
