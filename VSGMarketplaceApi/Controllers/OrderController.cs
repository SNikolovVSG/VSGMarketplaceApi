using Microsoft.AspNetCore.Mvc;
using Data.Models;
using Services.Interfaces;
using Data.ViewModels;
using Microsoft.AspNetCore.Authorization;

namespace VSGMarketplaceApi.Controllers
{
    //[Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IOrdersService ordersService;

        public OrderController(IOrdersService ordersService)
        {
            this.ordersService = ordersService; 
        }

        //[Authorize(Policy = "AdminOnly")]
        [HttpPost("~/Marketplace/Buy")]
        public async Task<IActionResult> Buy([FromBody] NewOrderAddModel input)
        {
            var userEmail = HttpContext.User.Claims.First(x => x.Value.Contains("vsgbg.com")).Value;
            input.UserEmail = userEmail;
            string result = await this.ordersService.BuyAsync(input);
            if (result != Constants.Ok)
            {
                return BadRequest(result);
            }

            return Ok();
        }

        [HttpGet("~/MyOrders")]
        public async Task<ActionResult<List<MyOrdersViewModel>>> MyOrders()
        {
            var userEmail = HttpContext.User.Claims.First(x => x.Value.Contains("vsgbg.com")).Value;
            var result = await this.ordersService.GetByUserEmail(userEmail);
            return Ok(result);
        }

        [HttpGet("~/PendingOrder/{code}")]
        public async Task<ActionResult<Order>> ById([FromRoute] int code)
        {
            var result = await this.ordersService.GetByCodeAsync(code);
            return Ok(result);
        }

        [HttpPut("~/MyOrders/DeleteOrder/{code}")]
        public async Task<IActionResult> DeleteAsync([FromRoute] int code)
        {
            var userEmail = HttpContext.User.Claims.First(x => x.Value.Contains("vsgbg.com")).Value;
            string result = await this.ordersService.DeleteAsync(code, userEmail);

            if (result != Constants.Ok)
            {
                return BadRequest(result);
            }
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
