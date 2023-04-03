using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VSGMarketplaceApi.DTOs;
using VSGMarketplaceApi.Models;

namespace VSGMarketplaceApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private ApplicationDbContext dbContext;

        public OrderController(ApplicationDbContext context)
        {
            this.dbContext = context;
        }

        [Authorize]
        [HttpPost("~/Marketplace/Buy")]
        public IActionResult Buy([FromBody] int code, int quantity, int userId)
        {
            if (!CheckForQuantity(code, quantity))
            {
                return BadRequest("Not enough quantities");
            }

            var orderPrice = GetPrice(code, quantity);
            var userEmail = GetUserEmail(userId);
            var getName = GetName(code);

            var order = new Order 
            { 
                Name = getName,
                OrderDate = DateTime.Now.Date, 
                UserId = userId,
                Code = code, 
                Quantity = quantity, 
                Status = Constants.Pending, 
                OrderedBy = userEmail, 
                OrderPrice = orderPrice 
            };

            ReduceItemQuantity(code, quantity);

            this.dbContext.Orders.Add(order);
            this.dbContext.SaveChanges();
            return Ok();
        }


        [Authorize]
        [HttpGet("~/MyOrders")]
        public ActionResult<List<MyOrdersViewModel>> MyOrders(int userId)
        {
            return GetMyOrders(userId);
        }

        [Authorize]
        [HttpGet("~/Order/{id}")]
        public ActionResult<Order> ById(int code)
        {
            return GetOrder(code);
        }

        //Cancel an order
        [Authorize]
        [HttpDelete("~/MyOrders/DeleteOrder")]
        public IActionResult Delete([FromBody] int id) 
        {
            this.dbContext.Orders.Where(x => x.Id == id).ExecuteDelete();

            return this.Redirect("/");
        }
        
        //Pending Orders
        [HttpGet("~/PendingOrders")]
        [Authorize(Roles = "Administrator")]
        public ActionResult<List<PendingOrderViewModel>> PendingOrders()
        {
            return GetPendingOrders();
        }

        [Authorize(Roles = "Administrator")]
        [HttpPut("~/PendingOrders/CompleteOrder")]
        public IActionResult Complete([FromBody] int id)
        {
            SetFinished(id);
            return Ok();
        }

        [NonAction]
        public void SetFinished(int id)
        {
            this.dbContext.Orders.Where(x => x.Id == id).First().Status = Constants.Finished;
            this.dbContext.SaveChanges();
        }

        [NonAction]
        public void ReduceItemQuantity(int code, int quantity)
        {
            var item = GetItem(code);
            item.QuantityForSale -= quantity; 
            this.dbContext.SaveChanges();
        }

        [NonAction]
        public Item GetItem(int code)
        {
            return this.dbContext.Items.Where(x => x.Code == code).First();
        }

        [NonAction]
        public bool CheckForQuantity(int code, int quantity)
        {
            return this.dbContext.Items.Where(x => x.Code == code).First().QuantityForSale > quantity;
        }

        [NonAction]
        public double GetPrice(int code, int quantity)
        {
            return this.dbContext.Items.Where(x => x.Code == code).First().Price * quantity;
        }

        [NonAction]
        public string GetUserEmail(int userId)
        {
            return this.dbContext.Users.Where(x => x.Id == userId).First().Email;
        }

        [NonAction]
        public List<PendingOrderViewModel> GetPendingOrders()
        {
            return this.dbContext.Orders.Where(x => x.Status == Constants.Pending)
                .Select(x => new PendingOrderViewModel
                {
                    Code = x.Code,
                    OrderBy = x.OrderedBy,
                    OrderDate = x.OrderDate,
                    OrderPrice = x.OrderPrice,
                    Quantity = x.Quantity,
                    Status = x.Status
                }).ToList();
        }

        [NonAction]
        private List<MyOrdersViewModel> GetMyOrders(int userId)
        {
            return this.dbContext.Orders.Where(x => x.UserId == userId)
                .Select(x => new MyOrdersViewModel
                {
                    Name = x.Name,
                    OrderDate = x.OrderDate,
                    OrderPrice = x.OrderPrice,
                    Quantity = x.Quantity,
                    Status = x.Status
                }).ToList();
        }

        [NonAction]
        private string GetName(int code)
        {
            return this.dbContext.Items.Where(x => x.Code == code).First().Name;
        }

        private Order GetOrder(int id) 
        {
            return this.dbContext.Orders.Where(x => x.Id == id).First();
        }
    }
}
