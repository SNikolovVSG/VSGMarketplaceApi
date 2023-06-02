using Microsoft.AspNetCore.Mvc;
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

        [HttpPost("~/Marketplace/Buy")]
        public async Task<IActionResult> Buy([FromBody] NewOrderAddModel input)
        {
            await this.ordersService.BuyAsync(input);

            return Ok();
        }

        [HttpGet("~/MyOrders")]
        public async Task<ActionResult<List<MyOrdersViewModel>>> MyOrders()
        {
            var result = await this.ordersService.GetByUserEmail();

            return Ok(result);
        }

        [HttpPut("~/MyOrders/DeleteOrder/{id}")]
        public async Task<IActionResult> DeleteAsync([FromRoute] int id)
        {
            await this.ordersService.DeleteAsync(id);
            
            return Ok();
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpGet("~/PendingOrders")]
        public async Task<ActionResult<List<PendingOrderViewModel>>> PendingOrders()
        {
            var orders = await this.ordersService.GetAllPendingOrdersAsync();
            
            return Ok(orders);
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpPut("~/PendingOrders/Complete/{id}")]
        public async Task<IActionResult> Complete([FromRoute] int id)
        {
            await this.ordersService.CompleteAsync(id);

            return Ok();
        }
    }
}
