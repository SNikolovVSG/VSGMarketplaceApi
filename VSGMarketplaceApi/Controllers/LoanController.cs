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

        [HttpGet("~/MyLoans/{email}/")]
        public async Task<ActionResult<Loan[]>> MyLoans([FromRoute] string email)
        {
            var loans = await this.loansService.GetMyLoansAsync(email.ToLower()); //remove when added azure accounts

            return Ok(loans);
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpGet("~/LentItems")]
        public async Task<ActionResult<List<LentItemViewModel>>> LentItems()
        {
            var loans = await this.loansService.GetAllLoansAsync();

            return Ok(loans);
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpPut("~/LentItems/Return/{id}")]
        public async Task<IActionResult> Return([FromRoute] int id)
        {
            await this.loansService.ReturnLoanAsync(id);

            return Ok();
        }
    }
}
