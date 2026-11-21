using ElectroShop.Core.Abstractions;
using ElectroShop.Core.Models;
using ElectroShop.Core.Models.Coupons;

namespace ElectroShop.Core.Pricing;

public sealed class PriceCalculator
{
    public static decimal FreeDeliveryThreshold => 5_000m;
    public static decimal MaxDiscountShare => 0.5m;

    public static decimal GetDeliveryBaseCost(DeliveryMethod method) => method switch
    {
        DeliveryMethod.Pickup => 0m,
        DeliveryMethod.PostOffice => 80m,
        DeliveryMethod.Courier => 150m,
        _ => throw new ArgumentOutOfRangeException(nameof(method))
    };

    public PriceBreakdown Calculate(IReadOnlyList<OrderItem> items, DeliveryMethod delivery,
        Coupon? coupon = null, LoyaltyLevel loyalty = LoyaltyLevel.Bronze)
    {
        var context = new PricingContext(items);
        var couponDiscount = Apply(coupon, context);
        var loyaltyDiscount = Apply(new LoyaltyDiscountPolicy(loyalty), context);

        // Сумарна знижка не перевищує половини вартості товарів
        var cap = decimal.Round(context.Subtotal * MaxDiscountShare, 2);
        if (couponDiscount + loyaltyDiscount > cap)
            loyaltyDiscount = Math.Max(0, cap - couponDiscount);
        couponDiscount = Math.Min(couponDiscount, cap);

        var afterDiscount = context.Subtotal - couponDiscount - loyaltyDiscount;
        var deliveryCost = afterDiscount >= FreeDeliveryThreshold ? 0m : GetDeliveryBaseCost(delivery);

        return new PriceBreakdown(context.Subtotal, couponDiscount, loyaltyDiscount, deliveryCost);
    }

    private static decimal Apply(IDiscountPolicy? policy, PricingContext context) =>
        policy is null ? 0m : Math.Min(policy.CalculateDiscount(context), context.Subtotal);
}
