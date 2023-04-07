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
    public class ItemController : ControllerBase
    {
        private IConfiguration configuration;

        public ItemController(IConfiguration config)
        {
            this.configuration = config;
        }

        //Works
        [Authorize(Roles = "Administrator")]
        [HttpPost("~/AddItem")]
        public async Task<IActionResult> AddAsync([FromBody] ItemAddDTO item)
        {
            if (item == null || !ModelState.IsValid) 
            {
                return BadRequest();
            }

            using var connection = new SqlConnection(configuration.GetConnectionString("DefaultConnection"));
            await connection.ExecuteAsync("insert into items (code, name, price, category, quantity, quantityForSale, description) values (@Code, @Name, @Price, @Category, @Quantity, @QuantityForSale, @Description)", item);
            return Ok();
        }

        //works
        [Authorize(Roles = "Administrator")]
        [HttpPut("~/Edit/{id}")]
        public async Task<IActionResult> Edit([FromRoute] int id, [FromBody] Item item)
        {
            if (!ModelState.IsValid) 
            {
                return BadRequest();
            }

            item.Id = id;
            using var connection = new SqlConnection(configuration.GetConnectionString("DefaultConnection"));
            await connection.ExecuteAsync
                ("update items set code = @Code, " +
                "name = @Name, " +
                "price = @Price, " +
                "category = @Category, " +
                "quantity = @Quantity, " +
                "quantityForSale = @QuantityForSale, " +
                "description = @Description " +
                "where id = @Id", item);

            return Ok();
        }

        //Works
        [Authorize(Roles = "Administrator")]
        [HttpDelete("~/DeleteItem/{id}")]
        public async Task<IActionResult> DeleteAsync([FromRoute] int id)
        {
            using var connection = new SqlConnection(configuration.GetConnectionString("DefaultConnection"));
            await connection.ExecuteAsync("delete from items where id = @Id", new { Id = id });

            return Ok();
        }

        //Works
        [Authorize(Roles = "Administrator")]
        [HttpGet("~/Inventory")]
        public async Task<ActionResult<List<InventoryItemViewModel>>> Inventory()
        {
            using var connection = new SqlConnection(configuration.GetConnectionString("DefaultConnection")); //that's the new command to get connection string
            var items = await connection.QueryAsync<Item>("select * from Items");
            var inventoryViewItems = items.Select(x => new InventoryItemViewModel
            {
                Category = x.Category,
                Code = x.Code,
                QuantityForSale = x.QuantityForSale,
                Name = x.Name,
                Quantity = x.Quantity,
            });

            return Ok(inventoryViewItems);
        }

        //Works
        [Authorize]
        [HttpGet("~/Marketplace")]
        public async Task<ActionResult<List<MarketplaceItemViewModel>>> MarketplaceAsync()
        {
            using var connection = new SqlConnection(configuration.GetConnectionString("DefaultConnection")); //that's the new command to get connection string
            var items = await connection.QueryAsync<Item>("select * from Items where quantityForSale > 0");
            var marketplaceItems = items.Select(x => new MarketplaceItemViewModel
            {
                Category = x.Category,
                Code = x.Code,
                Price = x.Price,
                QuantityForSale = x.QuantityForSale
            });

            return Ok(marketplaceItems);
        }

        //Works
        [Authorize]
        [HttpGet("~/Item/{id}")]
        public async Task<ActionResult<Item>> ById([FromRoute] int id)
        {
            using var connection = new SqlConnection(configuration.GetConnectionString("DefaultConnection"));
            var item = await connection.QueryFirstAsync<Item>("select * from Items where id = @Id", new { Id = id });
            if (item == null || item.QuantityForSale <= 0) 
            {
                return BadRequest();
            }

            return Ok(item);
        }
    }
}
