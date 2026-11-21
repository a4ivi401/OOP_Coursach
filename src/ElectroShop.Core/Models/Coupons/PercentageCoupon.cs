using System.Text.Json.Serialization;
using ElectroShop.Core.Exceptions;
using ElectroShop.Core.Pricing;

namespace ElectroShop.Core.Models.Coupons;

public class PercentageCoupon : Coupon
{
    private decimal _percent;

    public PercentageCoupon() { }

    public PercentageCoupon(string code, decimal percent)
    {
        Code = code;
        Percent = percent;
    }

    public decimal Percent
    {
        get => _percent;
        set
        {
            if (value is <= 0 or > 50)
                throw new InvalidDiscountException("відсоток знижки має бути від 1 до 50.");
            _percent = value;
        }
    }

    [JsonIgnore]
    public override string TypeName => "Відсоткова";

    [JsonIgnore]
    public override string ValueText => $"−{Percent}% на все замовлення";

    protected override decimal CalculateRawDiscount(PricingContext context) =>
        context.Subtotal * Percent / 100m;
}
