using FluentValidation;
using Data.Models;

namespace Helpers.Validators
{
    public class ItemValidator : AbstractValidator<Item>
    {
        public ItemValidator()
        {
            RuleFor(item => item.Code).NotEmpty().GreaterThanOrEqualTo(0);
            RuleFor(item => item.Price).GreaterThanOrEqualTo(0);
            RuleFor(item => item.Name).NotEmpty();
            RuleFor(item => item.Quantity).GreaterThanOrEqualTo(0);
            RuleFor(item => item.QuantityForSale).GreaterThanOrEqualTo(0);
            RuleFor(item => item.Category).NotEmpty();
        }
    }
}
