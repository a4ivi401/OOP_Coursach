using System.Text.Json.Serialization;
using ElectroShop.Core.Exceptions;
using ElectroShop.Core.Pricing;

namespace ElectroShop.Core.Models.Coupons;

public class FixedAmountCoupon : Coupon
{
    private decimal _amount;

    public FixedAmountCoupon() { }

    public FixedAmountCoupon(string code, decimal amount)
    {
        Code = code;
        Amount = amount;
    }

    public decimal Amount
    {
        get => _amount;
        set
        {
            if (value is <= 0 or > 50_000)
                throw new InvalidDiscountException("сума знижки має бути від 1 до 50 000 ₴.");
            _amount = value;
        }
    }

    [JsonIgnore]
    public override string TypeName => "Фіксована";

    [JsonIgnore]
    public override string ValueText => $"−{Amount:N0} ₴";

    protected override decimal CalculateRawDiscount(PricingContext context) => Amount;
}
