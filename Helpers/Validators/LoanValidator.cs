using Data.Models;
using FluentValidation;

namespace Helpers.Validators
{
    public class LoanValidator : AbstractValidator<Loan>
    {
        public LoanValidator()
        {
            RuleFor(loan => loan.ItemId).NotEmpty();
            RuleFor(loan => loan.OrderedBy).NotEmpty();
            RuleFor(loan => loan.Quantity).GreaterThan(0).NotEmpty();
            RuleFor(loan => loan.LoanStartDate).NotEmpty();
        }
    }
}
