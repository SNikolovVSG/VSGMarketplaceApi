using Data.Models;
using Data.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;

namespace VSGMarketplaceApi.Controllers
{
    [Authorize]
    [ApiController]
    public class LoanController : Controller
    {
        private readonly ILoansService loansService;

        public LoanController(ILoansService loansService)
        {
            this.loansService = loansService;
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpPost("~/Inventory/Loan/{itemId}")]
        public async Task<IActionResult> Loan([FromRoute] int itemId, [FromBody] CreateLoanViewModel input)
        {
            await this.loansService.LoanAsync(itemId, input);

            return Ok();
        }

        [HttpGet("~/MyLoans")]
        public async Task<ActionResult<Loan[]>> MyLoans([FromBody] string userEmail)
        {
            var loans = await this.loansService.GetMyLoansAsync(userEmail);

            return Ok(loans);
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpGet("~/LentItems")]
        public async Task<ActionResult<List<KeyValuePair<string,int>>>> LentItems()
        {
            var loans = await this.loansService.GetAllLoansAsync();

            return Ok(loans);
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpPut("~/LentItems/Return/{id}")]
        public async Task<IActionResult> Return([FromRoute] int id)
        {
            await this.loansService.ReturnLoan(id);

            return Ok();
        }
    }
}
