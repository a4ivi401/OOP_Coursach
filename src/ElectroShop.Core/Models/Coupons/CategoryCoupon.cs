using System.Text.Json.Serialization;
using ElectroShop.Core.Exceptions;
using ElectroShop.Core.Pricing;

namespace ElectroShop.Core.Models.Coupons;

public class CategoryCoupon : Coupon
{
    private decimal _percent;
    private Guid _categoryId;

    public CategoryCoupon() { }

    public CategoryCoupon(string code, Guid categoryId, string categoryName, decimal percent)
    {
        Code = code;
        CategoryId = categoryId;
        CategoryName = categoryName;
        Percent = percent;
    }

    public Guid CategoryId
    {
        get => _categoryId;
        set => _categoryId = value == Guid.Empty
            ? throw new InvalidDiscountException("не вказано категорію.")
            : value;
    }

    public string CategoryName { get; set; } = string.Empty;

    public decimal Percent
    {
        get => _percent;
        set
        {
            if (value is <= 0 or > 70)
                throw new InvalidDiscountException("відсоток знижки на категорію має бути від 1 до 70.");
            _percent = value;
        }
    }

    [JsonIgnore]
    public override string TypeName => "На категорію";

    [JsonIgnore]
    public override string ValueText => $"−{Percent}% на «{CategoryName}»";

    protected override void ValidateSpecific(PricingContext context)
    {
        if (context.SubtotalForCategory(CategoryId) == 0)
            throw new InvalidDiscountException($"у кошику немає товарів категорії «{CategoryName}».", Code);
    }

    protected override decimal CalculateRawDiscount(PricingContext context) =>
        context.SubtotalForCategory(CategoryId) * Percent / 100m;
}
