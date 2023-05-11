using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VSGMarketplaceApi.Data.Models;
using VSGMarketplaceApi.Data.Repositories.Interfaces;
using VSGMarketplaceApi.ViewModels;

namespace VSGMarketplaceApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IUnitOfWork unitOfWork;

        public OrderController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork; //ExceptionHandling => if result = 0 => lo6o bace
        }

        [HttpPost("~/Marketplace/Buy")]
        public async Task<IActionResult> Buy([FromBody] NewOrderAddModel input)
        {
            var result = await this.unitOfWork.Orders.AddAsync(input);
            if (result > 0) { return Ok(); }
            return BadRequest();
        }

        //[Authorize]
        [HttpGet("~/MyOrders/{userId}")]
        public async Task<ActionResult<List<MyOrdersViewModel>>> MyOrders([FromRoute] int userId)
        {
            var result = await this.unitOfWork.Orders.GetByUserId(userId);
            return Ok(result);
        }

        //[Authorize]
        [HttpGet("~/PendingOrder/{code}")]
        public async Task<ActionResult<Order>> ById([FromRoute] int code)
        {
            var result = await this.unitOfWork.Orders.GetByCodeAsync(code);
            return Ok(result);
        }

        //[Authorize]
        [HttpPut("~/MyOrders/DeleteOrder/{code}")]
        public async Task<IActionResult> DeleteAsync([FromRoute] int code, [FromBody] int userId)
        {
            var result = await this.unitOfWork.Orders.DeleteAsync(code, userId);

            if (result > 0) { return Ok(); }
            return BadRequest();
        }

        [HttpGet("~/PendingOrders")]
        //[Authorize(Roles = "Administrator")]
        public async Task<ActionResult<List<PendingOrderViewModel>>> PendingOrders()
        {
            var orders = await this.unitOfWork.Orders.GetAllPendingOrdersAsync();
            return Ok(orders);
        }

        //[Authorize(Roles = "Administrator")]
        [HttpPut("~/PendingOrders/Complete/{code}")]
        public async Task<IActionResult> Complete([FromRoute] int code)
        {
            var result = await this.unitOfWork.Orders.CompleteAsync(code);
            if (result == 0) { return BadRequest(); }
            return Ok();
        }
    }
}
