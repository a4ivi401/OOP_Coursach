using System.Text.Json.Serialization;

namespace ElectroShop.Core.Pricing;

public sealed record PriceBreakdown(
    decimal Subtotal,
    decimal CouponDiscount,
    decimal LoyaltyDiscount,
    decimal DeliveryCost)
{
    public static PriceBreakdown Empty { get; } = new(0, 0, 0, 0);

    [JsonIgnore]
    public decimal TotalDiscount => CouponDiscount + LoyaltyDiscount;

    [JsonIgnore]
    public decimal Total => Subtotal - TotalDiscount + DeliveryCost;
}
