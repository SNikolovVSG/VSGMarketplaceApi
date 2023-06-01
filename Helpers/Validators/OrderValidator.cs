using FluentValidation;
using Data.Models;

namespace Helpers.Validators
{
    public class OrderValidator : AbstractValidator<Order>
    {
        public OrderValidator()
        {
            RuleFor(order => order.OrderedBy).NotEmpty();
            RuleFor(order => order.OrderPrice).GreaterThanOrEqualTo(0);
            RuleFor(order => order.ItemCode);
            RuleFor(order => order.OrderDate).NotEmpty();
            RuleFor(order => order.Name).NotEmpty();
            RuleFor(order => order.Quantity).NotEmpty().GreaterThan(0);
        }

    }
}
