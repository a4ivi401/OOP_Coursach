using ElectroShop.Core.Exceptions;
using ElectroShop.Core.Models;
using ElectroShop.Core.Models.Coupons;
using ElectroShop.Core.Pricing;

namespace ElectroShop.Tests;

public class CouponTests
{
    private static readonly DateTime Now = new(2026, 11, 20);
    private static readonly Guid PhonesId = Guid.NewGuid();
    private static readonly Guid AudioId = Guid.NewGuid();

    private static PricingContext Context() => new(new List<OrderItem>
    {
        new(new Product("PHN-111", "Phone", "Acme", PhonesId, 10_000m, 10), 1),
        new(new Product("AUD-111", "Buds", "Acme", AudioId, 1_000m, 10), 2)
    });

    [Fact]
    public void PercentageCoupon_CalculatesFromSubtotal() =>
        Assert.Equal(1_200m, new PercentageCoupon("TEN10", 10).CalculateDiscount(Context()));

    [Fact]
    public void FixedCoupon_IsCappedBySubtotal()
    {
        var context = new PricingContext([new OrderItem(new Product("ACC-111", "Cable", "Acme", PhonesId, 100m, 5), 1)]);
        Assert.Equal(100m, new FixedAmountCoupon("BIG500", 500).CalculateDiscount(context));
    }

    [Fact]
    public void CategoryCoupon_AppliesOnlyToCategoryItems() =>
        Assert.Equal(400m, new CategoryCoupon("AUDIO20", AudioId, "Аудіо", 20).CalculateDiscount(Context()));

    [Fact]
    public void CategoryCoupon_WithoutCategoryItems_Throws()
    {
        var coupon = new CategoryCoupon("TV10", Guid.NewGuid(), "ТВ", 10);
        Assert.Throws<InvalidDiscountException>(() => coupon.EnsureApplicable(Context(), Now));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(51)]
    public void PercentageCoupon_RejectsInvalidPercent(decimal percent) =>
        Assert.Throws<InvalidDiscountException>(() => new PercentageCoupon("BAD1", percent));

    [Fact]
    public void ExpiredCoupon_Throws()
    {
        var coupon = new PercentageCoupon("OLD10", 10) { ValidFrom = Now.AddMonths(-2), ValidUntil = Now.AddDays(-1) };
        var ex = Assert.Throws<InvalidDiscountException>(() => coupon.EnsureApplicable(Context(), Now));
        Assert.Equal("OLD10", ex.CouponCode);
    }

    [Fact]
    public void NotYetActiveCoupon_Throws()
    {
        var coupon = new PercentageCoupon("SOON10", 10) { ValidFrom = Now.AddDays(3) };
        Assert.Throws<InvalidDiscountException>(() => coupon.EnsureApplicable(Context(), Now));
    }

    [Fact]
    public void MinOrderAmountNotReached_Throws()
    {
        var coupon = new FixedAmountCoupon("RICH1000", 1000) { ValidFrom = Now, MinOrderAmount = 50_000 };
        Assert.Throws<InvalidDiscountException>(() => coupon.EnsureApplicable(Context(), Now));
    }

    [Fact]
    public void UsageLimitReached_Throws()
    {
        var coupon = new FixedAmountCoupon("ONCE100", 100) { ValidFrom = Now, UsageLimit = 1 };
        coupon.RegisterUsage();
        Assert.Throws<InvalidDiscountException>(() => coupon.EnsureApplicable(Context(), Now));
    }

    [Fact]
    public void DeactivatedCoupon_Throws()
    {
        var coupon = new PercentageCoupon("OFF10", 10) { ValidFrom = Now, IsActive = false };
        Assert.Throws<InvalidDiscountException>(() => coupon.EnsureApplicable(Context(), Now));
    }
}
