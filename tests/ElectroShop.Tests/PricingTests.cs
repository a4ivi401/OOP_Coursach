using ElectroShop.Core.Models;
using ElectroShop.Core.Models.Coupons;
using ElectroShop.Core.Pricing;

namespace ElectroShop.Tests;

public class PricingTests
{
    private static readonly Guid CategoryId = Guid.NewGuid();
    private readonly PriceCalculator _calculator = new();

    private static List<OrderItem> Items(decimal price, int qty = 1) =>
        [new OrderItem(new Product("PHN-111", "Phone", "Acme", CategoryId, price, 20), qty)];

    [Theory]
    [InlineData(0, LoyaltyLevel.Bronze)]
    [InlineData(20_000, LoyaltyLevel.Silver)]
    [InlineData(49_999.99, LoyaltyLevel.Silver)]
    [InlineData(50_000, LoyaltyLevel.Gold)]
    [InlineData(150_000, LoyaltyLevel.Platinum)]
    public void LoyaltyLevel_DependsOnTotalSpent(decimal spent, LoyaltyLevel expected) =>
        Assert.Equal(expected, LoyaltyProgram.GetLevel(spent));

    [Fact]
    public void SmallOrder_PaysForDelivery()
    {
        var result = _calculator.Calculate(Items(1_000m), DeliveryMethod.Courier);
        Assert.Equal(150m, result.DeliveryCost);
        Assert.Equal(1_150m, result.Total);
    }

    [Fact]
    public void LargeOrder_GetsFreeDelivery()
    {
        var result = _calculator.Calculate(Items(6_000m), DeliveryMethod.Courier);
        Assert.Equal(0m, result.DeliveryCost);
    }

    [Fact]
    public void CouponAndLoyalty_AreCombined()
    {
        var result = _calculator.Calculate(Items(10_000m), DeliveryMethod.Pickup,
            new PercentageCoupon("TEN10", 10), LoyaltyLevel.Gold);

        Assert.Equal(1_000m, result.CouponDiscount);
        Assert.Equal(500m, result.LoyaltyDiscount);
        Assert.Equal(8_500m, result.Total);
    }

    [Fact]
    public void TotalDiscount_IsCappedAtHalfOfSubtotal()
    {
        var result = _calculator.Calculate(Items(1_000m), DeliveryMethod.Pickup,
            new FixedAmountCoupon("HUGE900", 900), LoyaltyLevel.Platinum);

        Assert.Equal(500m, result.TotalDiscount);
        Assert.Equal(500m, result.Total);
    }
}
