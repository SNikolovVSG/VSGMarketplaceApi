using AutoMapper;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using VSGMarketplaceApi.DTOs;
using VSGMarketplaceApi.Models;
using VSGMarketplaceApi.Repositories.Interfaces;

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

        //works exceptions ok
        [Authorize]
        [HttpPost("~/Marketplace/Buy")]
        public async Task<IActionResult> Buy([FromBody] NewOrderAddModel input)
        {
            var result = await unitOfWork.Orders.AddAsync(input);

            if (result > 0) { return Ok(); }
            return BadRequest();
        }

        //works exceptions ok
        [Authorize]
        [HttpGet("~/MyOrders")]
        public async Task<ActionResult<List<MyOrdersViewModel>>> MyOrders([FromBody] int userId)
        {
            var result = await unitOfWork.Orders.GetByUserId(userId);
            return Ok(result);
        }

        //works exceptions ok
        [Authorize]
        [HttpGet("~/PendingOrder/{code}")]
        public async Task<ActionResult<Order>> ById([FromRoute] int code)
        {
            var result = await unitOfWork.Orders.GetByCodeAsync(code);
            return Ok(result);
        }

        //works exception ok
        [Authorize]
        [HttpPut("~/MyOrders/DeleteOrder/{code}")]
        public async Task<IActionResult> DeleteAsync([FromRoute] int code, [FromBody] int userId)
        {
            var result = await unitOfWork.Orders.DeleteAsync(code, userId);

            if (result > 0) { return Ok(); }
            return BadRequest();
        }

        //works
        [HttpGet("~/PendingOrders")]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult<List<PendingOrderViewModel>>> PendingOrders()
        {
            var orders = await unitOfWork.Orders.GetAllPendingOrdersAsync();
            return Ok(orders);
        }

        //works
        [Authorize(Roles = "Administrator")]
        [HttpPut("~/PendingOrders/Complete/{code}")]
        public async Task<IActionResult> Complete([FromRoute] int code)
        {
            var result = unitOfWork.Orders.CompleteAsync(code);
            if (result.Exception != null) { return BadRequest(); }
            return Ok();
        }
    }
}
