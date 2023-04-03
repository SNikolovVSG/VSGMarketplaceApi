using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VSGMarketplaceApi.DTOs;
using VSGMarketplaceApi.Models;

namespace VSGMarketplaceApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ItemController : ControllerBase
    {
        private ApplicationDbContext dbContext;

        public ItemController(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        [Authorize(Roles = "Administrator")]
        [HttpPost("~/AddItem")]
        public IActionResult Add([FromBody] Item item)
        {
            this.dbContext.Items.Add(item);
            this.dbContext.SaveChanges();
            return Ok();
        }

        [Authorize(Roles = "Administrator")]
        [HttpPut("~/Edit")]
        public IActionResult Edit([FromBody] Item item)
        {
            var oldItem = GetItem(item.Code);
            oldItem = item;
            this.dbContext.SaveChanges();
            return Ok();
        }

        [Authorize(Roles = "Administrator")]
        [HttpDelete("~/DeleteItem")]
        public IActionResult Delete([FromBody] int code)
        {
            DeleteItem(code);

            return Ok();
        }

        [Authorize(Roles = "Administrator")]
        [HttpGet("~/Inventory")]
        public ActionResult<List<InventoryItemViewModel>> Inventory()
        {
            return GetInventoryItemViewModels();
        }

        //[Authorize]
        [HttpGet("~/Marketplace")]
        public ActionResult<List<MarketplaceItemViewModel>> Marketplace()
        {
            return GetMarketplaceItemViewModels();
        }

        [Authorize]
        [HttpGet("~/Item/{id}")]
        public ActionResult<Item> ById(int code)
        {
            return GetItem(code);
        }

        [NonAction]
        private ActionResult<List<MarketplaceItemViewModel>> GetMarketplaceItemViewModels()
        {
            return this.dbContext.Items
                .Select(x => new MarketplaceItemViewModel
                {
                    Category = x.Category,
                    Code = x.Code,
                    QuantityForSale = x.QuantityForSale,
                    Price = x.Price
                }).ToList();
        }

        [NonAction]
        private List<InventoryItemViewModel> GetInventoryItemViewModels()
        {
            return this.dbContext.Items
                .Select(x => new InventoryItemViewModel 
                { 
                    Category = x.Category, 
                    Name = x.Name, 
                    Code = x.Code, 
                    Quantity = x.Quantity, 
                    QuantityForSale = x.QuantityForSale 
                }).ToList();
        }

        [NonAction]
        private void DeleteItem(int code)
        {
            this.dbContext.Items.Where(x => x.Code == code).ExecuteDelete();
        }

        [NonAction]
        public Item GetItem(int code)
        {
            return this.dbContext.Items.Where(x => x.Code == code).First();
        }
    }
}
