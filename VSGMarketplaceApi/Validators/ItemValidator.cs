using FluentValidation;
using VSGMarketplaceApi.Data.Models;

namespace VSGMarketplaceApi.Validators
{
    public class ItemValidator : AbstractValidator<Item>
    {
        public ItemValidator()
        {
            RuleFor(item => item.Price).GreaterThanOrEqualTo(0);
            RuleFor(item => item.Name).NotEmpty();
            RuleFor(item => item.Quantity).GreaterThanOrEqualTo(0);
            RuleFor(item => item.QuantityForSale).GreaterThanOrEqualTo(0);
            RuleFor(item => item.Category).NotEmpty();
        }
    }
}
