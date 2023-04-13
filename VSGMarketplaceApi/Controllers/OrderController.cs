using AutoMapper;
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
        private readonly IConfiguration configuration;
        //private readonly IMapper mapper;

        public OrderController(IConfiguration configuration/*, IMapper mapper*/)
        {
            this.configuration = configuration;
            //this.mapper = mapper;
        }

        //works
        [Authorize]
        [HttpPost("~/Marketplace/Buy")]
        public async Task<IActionResult> Buy([FromBody] NewOrderAddModel input)
        {
            using var connection = new SqlConnection(configuration.GetConnectionString("DefaultConnection"));

            var item = await connection.QueryFirstAsync<Item>("select * from items where code = @Code", new { Code = input.ItemCode });

            bool checkForQuantity = item.QuantityForSale < input.Quantity;
            if (checkForQuantity)
            {
                return BadRequest("Not enough quantities");
            }

            var orderPrice = item.Price * input.Quantity;
            var userEmail = await connection.QueryFirstAsync<string>("select email from users where id = @Id", new { Id = input.UserId });

            var order = new Order
            {
                ItemCode = input.ItemCode,
                Name = item.Name,
                Quantity = input.Quantity,
                OrderPrice = orderPrice,
                OrderedBy = userEmail,
                OrderDate = DateTime.Now.Date,
                Status = Constants.Pending,
                UserId = input.UserId,
                IsDeleted = false,
            };

            await connection.ExecuteAsync
                ("insert into Orders (ItemCode, Name, Quantity, OrderPrice, OrderedBy, OrderDate, Status, UserId, IsDeleted) values (@ItemCode, @Name, @Quantity, @OrderPrice, @OrderedBy, @OrderDate, @Status, @UserId, @IsDeleted)", order);

            await connection.ExecuteAsync
                ("update items set quantityForSale = @Count where code = @Code", new { Count = item.QuantityForSale - input.Quantity, Code = input.ItemCode });
            return Ok();
        }

        //works
        [Authorize]
        [HttpGet("~/MyOrders/{userId}")]
        public async Task<ActionResult<List<MyOrdersViewModel>>> MyOrders([FromRoute] int userId)
        {
            using var connection = new SqlConnection(configuration.GetConnectionString("DefaultConnection"));
            var orders = await connection.QueryAsync<MyOrdersViewModel>
                ("SELECT ItemCode, Name, Quantity, OrderPrice, OrderedBy, OrderDate, Status, UserId FROM [VSGMarketplace].[dbo].[Orders] AS orT  INNER JOIN Users as usS on usS.Email = orT.OrderedBy where usS.Id = @Id and IsDeleted = 0", new { Id = userId });

            return Ok(orders);
        }

        //works
        [Authorize]
        [HttpGet("~/Order/{code}")]                                                                                                     
        public async Task<ActionResult<Order>> ById([FromRoute] int code)
        {
            using var connection = new SqlConnection(configuration.GetConnectionString("DefaultConnection"));
            var order = await connection.QueryFirstAsync<Order>("select * from orders where code = @Code and IsDeleted = 0", new { Code = code });

            return Ok(order);
        }

        //works
        [Authorize]
        [HttpPut("~/MyOrders/DeleteOrder/{code}")]
        public async Task<IActionResult> DeleteAsync([FromRoute] int code, [FromBody] int userId)
        {
            using var connection = new SqlConnection(configuration.GetConnectionString("DefaultConnection"));

            ;

            try
            {
                var order = await connection.QueryFirstAsync<Order>("Select * from orders where code = @Code and IsDeleted = 0", new { Code = code });

                if (order == null && order.UserId != userId)
                {
                    return BadRequest();
                }

                if (order.Status == Constants.Pending)
                {
                    var item = await connection.QueryFirstAsync<Item>("Select * from items where code = @Code", new { Code = order.ItemCode });

                    if (item == null) { return BadRequest(); }

                    item.QuantityForSale += order.Quantity;

                    await connection.ExecuteAsync("update items set QuantityForSale = @QuantityForSale where Code = @Code", item);
                }

                await connection.ExecuteAsync("update orders set IsDeleted = 1 where code = @Code", new { Code = code });
            }
            catch (Exception)
            {
                return BadRequest();
            }

            return Ok();
        }

        //works
        [HttpGet("~/PendingOrders")]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult<List<PendingOrderViewModel>>> PendingOrders()
        {
            using var connection = new SqlConnection(configuration.GetConnectionString("DefaultConnection"));
            var orders = await connection.QueryAsync<PendingOrderViewModel>("select * from orders where status = @Pending and IsDeleted = 0", new { Pending = Constants.Pending });

            return Ok(orders);
        }

        //works
        [Authorize(Roles = "Administrator")]
        [HttpPut("~/PendingOrders/CompleteOrder/{code}")]
        public async Task<IActionResult> Complete([FromRoute] int code)
        {
            using var connection = new SqlConnection(configuration.GetConnectionString("DefaultConnection"));
            var order = await connection.ExecuteAsync("update Orders set status = @Status where code = @Code and IsDeleted = 0", new { Status = Constants.Finished, Code = code });

            return Ok();
        }
    }
}
