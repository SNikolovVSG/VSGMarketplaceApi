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
    public class ItemController : ControllerBase
    {
        private IConfiguration configuration;
        private readonly IMapper mapper;

        public ItemController(IConfiguration config, IMapper mapper)
        {
            this.configuration = config;
            this.mapper = mapper;
        }

        //works
        [Authorize(Roles = "Administrator")]
        [HttpPost("~/AddItem")]
        public async Task<IActionResult> AddAsync([FromBody] ItemAddModel item)
        {
            if (item == null || !ModelState.IsValid)
            {
                return BadRequest();
            }

            using var connection = new SqlConnection(configuration.GetConnectionString("DefaultConnection"));
            await connection.ExecuteAsync("insert into items (name, price, category, quantity, quantityForSale, description) values (@Name, @Price, @Category, @Quantity, @QuantityForSale, @Description)", item);
            return Ok();
        }

        //works
        [Authorize(Roles = "Administrator")]
        [HttpPut("~/Edit/{code}")]
        public async Task<IActionResult> Edit([FromRoute] int code, [FromBody] ItemAddModel item)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var editItem = mapper.Map<Item>(item);
            editItem.Code = code;

            using var connection = new SqlConnection(configuration.GetConnectionString("DefaultConnection"));
            await connection.ExecuteAsync
                ("update items set " +
                "name = @Name, " +
                "price = @Price, " +
                "category = @Category, " +
                "quantity = @Quantity, " +
                "quantityForSale = @QuantityForSale, " +
                "description = @Description " +
                "where code = @code", editItem);

            return Ok();
        }

        //works
        [Authorize(Roles = "Administrator")]
        [HttpDelete("~/DeleteItem/{code}")]
        public async Task<IActionResult> DeleteAsync([FromRoute] int code)
        {
            using var connection = new SqlConnection(configuration.GetConnectionString("DefaultConnection"));
            await connection.ExecuteAsync("delete from items where code = @Code", new { Code = code });

            return Ok();
        }

        //works
        [Authorize(Roles = "Administrator")]
        [HttpGet("~/Inventory")]
        public async Task<ActionResult<List<InventoryItemViewModel>>> Inventory()
        {
            using var connection = new SqlConnection(configuration.GetConnectionString("DefaultConnection"));
            var items = await connection.QueryAsync<InventoryItemViewModel>("select * from Items");

            return Ok(items);
        }

        //works
        [Authorize]
        [HttpGet("~/Marketplace")]
        public async Task<ActionResult<List<MarketplaceItemViewModel>>> MarketplaceAsync()
        {
            using var connection = new SqlConnection(configuration.GetConnectionString("DefaultConnection"));
            var items = await connection.QueryAsync<MarketplaceItemViewModel>("select * from Items where quantityForSale > 0");
            
            return Ok(items);
        }

        //works
        [Authorize]
        [HttpGet("~/Marketplace/{code}")]
        public async Task<ActionResult<MarketplaceByIdItemViewModel>> ById([FromRoute] int code)
        {
            using var connection = new SqlConnection(configuration.GetConnectionString("DefaultConnection"));
            var item = await connection.QueryFirstAsync<MarketplaceByIdItemViewModel>("select * from Items where code = @Code", new { Code = code });
            if (item == null || item.QuantityForSale <= 0)
            {
                return BadRequest();
            }

            return Ok(item);
        }
    }
}
