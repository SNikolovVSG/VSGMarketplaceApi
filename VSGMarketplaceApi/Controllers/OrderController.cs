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
    public class OrderController : ControllerBase
    {
        private readonly IOrdersService ordersService;

        public OrderController(IOrdersService ordersService)
        {
            this.ordersService = ordersService; 
        }

        //ok
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

        //ok
        [HttpGet("~/MyOrders")]
        public async Task<ActionResult<List<MyOrdersViewModel>>> MyOrders()
        {
            var result = await this.ordersService.GetByUserEmail();
            return Ok(result);
        }

        //ok
        [HttpPut("~/MyOrders/DeleteOrder/{id}")]
        public async Task<IActionResult> DeleteAsync([FromRoute] int id)
        {
            string result = await this.ordersService.DeleteAsync(id);

            if (result != Constants.Ok)
            {
                return BadRequest(result);
            }
            return Ok();
        }

        //ok
        [Authorize(Policy = "AdminOnly")]
        [HttpGet("~/PendingOrders")]
        public async Task<ActionResult<List<PendingOrderViewModel>>> PendingOrders()
        {
            var orders = await this.ordersService.GetAllPendingOrdersAsync();
            return Ok(orders);
        }

        //ok
        [Authorize(Policy = "AdminOnly")]
        [HttpPut("~/PendingOrders/Complete/{id}")]
        public async Task<IActionResult> Complete([FromRoute] int id)
        {
            string result = await this.ordersService.CompleteAsync(id);

            if (result != Constants.Ok)
            {
                return BadRequest(result);
            }

            return Ok();
        }
    }
}
