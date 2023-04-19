using AutoMapper;
using Dapper;
using FluentValidation;
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
    public class ItemController : ControllerBase
    {
        private IConfiguration configuration;
        private readonly IMapper mapper;
        private readonly IUnitOfWork unitOfWork;
        private readonly IValidator validator;

        public ItemController(IUnitOfWork unitOfWork, IConfiguration config, IMapper mapper)
        {
            this.configuration = config;
            this.mapper = mapper;
            this.unitOfWork = unitOfWork;
        }

        //works
        [Authorize(Roles = "Administrator")]
        [HttpPost("~/Inventory/AddItem")]
        public async Task<IActionResult> AddAsync([FromBody] ItemAddModel item)
        {
            var result = await this.unitOfWork.Items.AddAsync(item);
            if (result > 0) { return Ok(); }
            return BadRequest();
        }

        //works
        [Authorize(Roles = "Administrator")]
        [HttpPut("~/Inventory/Modify/{code}")]
        public async Task<IActionResult> Edit([FromRoute] int code, [FromBody] ItemAddModel item)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var result = await unitOfWork.Items.UpdateAsync(item, code);
            if (result > 0) { return Ok(); };
            return BadRequest();
        }


        //dexter064 dexter356 
        //works
        [Authorize(Roles = "Administrator")]
        [HttpDelete("~/DeleteItem/{code}")]
        public async Task<IActionResult> DeleteAsync([FromRoute] int code)
        {
            var result = await this.unitOfWork.Items.DeleteAsync(code);
            if (result > 0) { return Ok(); }
            return BadRequest();
        }

        //works
        [Authorize(Roles = "Administrator")]
        [HttpGet("~/Inventory")]
        public async Task<ActionResult<List<InventoryItemViewModel>>> Inventory()
        {
            var items = await this.unitOfWork.Items.GetInventoryItemsAsync();
            return Ok(items);
        }

        //works
        [Authorize]
        [HttpGet("~/Marketplace")]
        public async Task<ActionResult<List<MarketplaceItemViewModel>>> MarketplaceAsync()
        {
            var items = await this.unitOfWork.Items.GetMarketplaceItemsAsync();
            return Ok(items);
        }

        //works
        [Authorize]
        [HttpGet("~/Marketplace/{code}")]
        public async Task<ActionResult<MarketplaceByIdItemViewModel>> ById([FromRoute] int code)
        {
            var item = await this.unitOfWork.Items.GetMarketplaceItemAsync(code);
            return Ok(item);
        }
    }
}
