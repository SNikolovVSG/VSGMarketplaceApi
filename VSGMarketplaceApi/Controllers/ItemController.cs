using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NLog;
using NLog.Web;
using VSGMarketplaceApi.Data.Models;
using VSGMarketplaceApi.Data.Repositories.Interfaces;
using VSGMarketplaceApi.ViewModels;

namespace VSGMarketplaceApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ItemController : ControllerBase
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly ILogger<ItemController> logger;

        public ItemController(IUnitOfWork unitOfWork, ILogger<ItemController> logger)
        {
            this.unitOfWork = unitOfWork;
            this.logger = logger;
        }

        //[Authorize(Roles = "Administrator")]
        [HttpPost("~/Inventory/AddItem")]
        public async Task<IActionResult> AddAsync([FromForm] ItemAddModelString item)
        {
            string result = await this.unitOfWork.Items.AddAsync(item);

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
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var result = await unitOfWork.Items.UpdateAsync(item, code);
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
            var result = await this.unitOfWork.Items.DeleteAsync(code);
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
            throw new Exception(" ebago");

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
