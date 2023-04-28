using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VSGMarketplaceApi.DTOs;
using VSGMarketplaceApi.Repositories.Interfaces;

namespace VSGMarketplaceApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ItemController : ControllerBase
    {
        private readonly IUnitOfWork unitOfWork;

        public ItemController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        //[Authorize(Roles = "Administrator")]
        [HttpPost("~/Inventory/AddItem")]
        public async Task<IActionResult> AddAsync([FromForm] ItemAddModelString item)
        {
            int? result = await this.unitOfWork.Items.AddAsync(item);

            if (result == null || result == 0) { return BadRequest(); }

            return Ok();
        }

        //[Authorize(Roles = "Administrator")]
        [HttpPut("~/Inventory/Modify/{code}")]
        public async Task<IActionResult> Edit([FromRoute] int code, [FromForm] ItemAddModelString item)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var result = await unitOfWork.Items.UpdateAsync(item, code);
            if (result > 0) { return Ok(); };
            return BadRequest();
        }

        //[Authorize(Roles = "Administrator")]
        [HttpDelete("~/DeleteItem/{code}")]
        public async Task<IActionResult> DeleteAsync([FromRoute] int code)
        {
            var result = await this.unitOfWork.Items.DeleteAsync(code);
            if (result > 0) { return Ok(); }
            return BadRequest();
        }

        //[Authorize(Roles = "Administrator")]
        [HttpGet("~/Inventory")]
        public async Task<ActionResult<List<InventoryItemViewModel>>> Inventory()
        {
            var items = await this.unitOfWork.Items.GetInventoryItemsAsync();
            return Ok(items);
        }

        //[Authorize]
        [HttpGet("~/Marketplace")]
        public async Task<ActionResult<List<MarketplaceItemViewModel>>> MarketplaceAsync()
        {
            var items = await this.unitOfWork.Items.GetMarketplaceItemsAsync();
            return Ok(items);
        }

        //[Authorize]
        [HttpGet("~/Marketplace/{code}")]
        public async Task<ActionResult<MarketplaceByIdItemViewModel>> ById([FromRoute] int code)
        {
            var item = await this.unitOfWork.Items.GetMarketplaceItemAsync(code);
            return Ok(item);
        }
    }
}
