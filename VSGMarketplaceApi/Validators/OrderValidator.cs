using FluentValidation;
using VSGMarketplaceApi.Data.Models;

namespace VSGMarketplaceApi.Validators
{
    public class OrderValidator : AbstractValidator<Order>
    {
        public OrderValidator()
        {
            RuleFor(order => order.UserId).NotEmpty();
            RuleFor(order => order.OrderPrice).NotEmpty().GreaterThanOrEqualTo(0);
            RuleFor(order => order.ItemCode).NotEmpty().GreaterThan(0);
            RuleFor(order => order.OrderDate).NotEmpty();
            RuleFor(order => order.Name).NotEmpty();
            RuleFor(order => order.Quantity).NotEmpty().GreaterThan(0);
        }

    }
}
