using ElectroShop.Core.Abstractions;
using ElectroShop.Core.Models;

namespace ElectroShop.Core.Pricing;

public sealed class LoyaltyDiscountPolicy : IDiscountPolicy
{
    private readonly LoyaltyLevel _level;

    public LoyaltyDiscountPolicy(LoyaltyLevel level) => _level = level;

    public decimal Percent => LoyaltyProgram.GetPercent(_level);

    public string Description => $"Знижка постійного клієнта ({_level.ToDisplayName()}, {Percent}%)";

    public decimal CalculateDiscount(PricingContext context) =>
        decimal.Round(context.Subtotal * Percent / 100m, 2);
}
