using Microsoft.AspNetCore.Mvc;
using VSGMarketplaceApi.Data.Models;
using VSGMarketplaceApi.Services.Interfaces;
using VSGMarketplaceApi.ViewModels;

namespace VSGMarketplaceApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IOrdersService ordersService;

        public OrderController(IOrdersService ordersService)
        {
            this.ordersService = ordersService; 
        }

        [HttpPost("~/Marketplace/Buy")]
        public async Task<IActionResult> Buy([FromBody] NewOrderAddModel input)
        {
            string result = await this.ordersService.BuyAsync(input);
            if (result != Constants.Ok)
            {
                return BadRequest(result);
            }

            return Ok();
        }

        //[Authorize]
        [HttpGet("~/MyOrders/{userId}")]
        public async Task<ActionResult<List<MyOrdersViewModel>>> MyOrders([FromRoute] int userId)
        {
            var result = await this.ordersService.GetByUserId(userId);
            return Ok(result);
        }

        //[Authorize]
        [HttpGet("~/PendingOrder/{code}")]
        public async Task<ActionResult<Order>> ById([FromRoute] int code)
        {
            var result = await this.ordersService.GetByCodeAsync(code);
            return Ok(result);
        }

        //[Authorize]
        [HttpPut("~/MyOrders/DeleteOrder/{code}")]
        public async Task<IActionResult> DeleteAsync([FromRoute] int code, [FromBody] int userId)
        {
            string result = await this.ordersService.DeleteAsync(code, userId);

            if (result != Constants.Ok)
            {
                return BadRequest(result);
            }
            return Ok();
        }

        [HttpGet("~/PendingOrders")]
        //[Authorize(Roles = "Administrator")]
        public async Task<ActionResult<List<PendingOrderViewModel>>> PendingOrders()
        {
            var orders = await this.ordersService.GetAllPendingOrdersAsync();
            return Ok(orders);
        }

        //[Authorize(Roles = "Administrator")]
        [HttpPut("~/PendingOrders/Complete/{code}")]
        public async Task<IActionResult> Complete([FromRoute] int code)
        {
            string result = await this.ordersService.CompleteAsync(code);

            if (result != Constants.Ok)
            {
                return BadRequest(result);
            }

            return Ok();
        }
    }
}
