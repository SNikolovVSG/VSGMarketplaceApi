using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using VSGMarketplaceApi.DTOs;
using VSGMarketplaceApi.Models;

namespace VSGMarketplaceApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private IConfiguration configuration;

        public OrderController(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        //Works
        [Authorize]
        [HttpPost("~/Marketplace/Buy")]
        public async Task<IActionResult> Buy([FromBody] NewOrderInputModel input)
        {
            using var connection = new SqlConnection(configuration.GetConnectionString("DefaultConnection"));

            var item = await connection.QueryFirstAsync<Item>("select * from items where code = @Code", new { Code = input.code });

            bool checkForQuantity = item.QuantityForSale <= input.quantity;
            if (checkForQuantity)
            {
                return BadRequest("Not enough quantities");
            }

            var orderPrice = item.Price * input.quantity;
            var userEmail = await connection.QueryFirstAsync<string>("select email from users where id = @Id", new { Id = input.userId });

            var order = new Order
            {
                Name = item.Name,
                OrderDate = DateTime.Now.Date,
                UserId = input.userId,
                Code = input.code, //change to itemId!!!
                Quantity = input.quantity,
                Status = Constants.Pending,
                OrderedBy = userEmail,
                OrderPrice = orderPrice
            };

            await connection.ExecuteAsync
                ("update items set quantityForSale = @Count where code = @Code", new { Count = item.QuantityForSale - input.quantity, Code = input.code });
            return Ok();
        }

        //works
        [Authorize]
        [HttpGet("~/MyOrders/{userId}")]
        public async Task<ActionResult<List<MyOrdersViewModel>>> MyOrders([FromRoute] int userId)
        {
            using var connection = new SqlConnection(configuration.GetConnectionString("DefaultConnection"));
            var orders = await connection.QueryAsync<Order>("SELECT Code, Name, Quantity, OrderPrice, OrderedBy, OrderDate, Status, UserId FROM [VSGMarketplace].[dbo].[Orders] AS orT  INNER JOIN Users as usS on usS.Email = orT.OrderedBy where usS.Id = @Id", new { Id = userId });
            var myOrdersViewModel = orders.Select(x => new MyOrdersViewModel
            {
                Name = x.Name,
                OrderDate = x.OrderDate,
                OrderPrice = x.OrderPrice,
                Quantity = x.Quantity,
                Status = x.Status
            });


            ;
            return Ok(myOrdersViewModel);
        }

        //works
        [Authorize]
        [HttpGet("~/Order/{id}")]
        public async Task<ActionResult<Order>> ById([FromRoute] int id)
        {
            using var connection = new SqlConnection(configuration.GetConnectionString("DefaultConnection"));
            var order = await connection.QueryFirstAsync<Order>("select * from orders where id = @Id", new { Id = id });

            return Ok(order);
        }

        //works
        [Authorize]
        [HttpDelete("~/MyOrders/DeleteOrder/{id}")]
        public async Task<IActionResult> DeleteAsync([FromRoute] int id)
        {
            using var connection = new SqlConnection(configuration.GetConnectionString("DefaultConnection"));
            await connection.ExecuteAsync("delete from orders where id = @Id", new { Id = id });

            return Ok();
        }

        //Works
        [HttpGet("~/PendingOrders")]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult<List<PendingOrderViewModel>>> PendingOrders()
        {
            using var connection = new SqlConnection(configuration.GetConnectionString("DefaultConnection"));
            var orders = await connection.QueryAsync<Order>("select * from orders where status = @Pending", new { Pending = Constants.Pending });
            var pendingOrders = orders.Select(x => new PendingOrderViewModel
            {
                Code = x.Code,
                OrderBy = x.OrderedBy,
                OrderDate = x.OrderDate,
                OrderPrice = x.OrderPrice,
                Quantity = x.Quantity,
                Status = x.Status
            });

            return Ok(pendingOrders);
        }

        //works
        [Authorize(Roles = "Administrator")]
        [HttpPut("~/PendingOrders/CompleteOrder/{id}")]
        public async Task<IActionResult> Complete([FromRoute] int id)
        {
            using var connection = new SqlConnection(configuration.GetConnectionString("DefaultConnection"));
            var order = await connection.ExecuteAsync("update Orders set status = @Status where id = @Id", new { Status = Constants.Finished, Id = id });

            return Ok();
        }
    }
}
