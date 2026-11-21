using ElectroShop.Core.Pricing;

namespace ElectroShop.Core.Abstractions;

public interface IDiscountPolicy
{
    string Description { get; }

    decimal CalculateDiscount(PricingContext context);
}
