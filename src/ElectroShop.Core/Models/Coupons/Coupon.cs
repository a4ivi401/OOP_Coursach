using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using ElectroShop.Core.Abstractions;
using ElectroShop.Core.Exceptions;
using ElectroShop.Core.Pricing;

namespace ElectroShop.Core.Models.Coupons;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(PercentageCoupon), "percent")]
[JsonDerivedType(typeof(FixedAmountCoupon), "fixed")]
[JsonDerivedType(typeof(CategoryCoupon), "category")]
public abstract partial class Coupon : EntityBase, IDiscountPolicy
{
    private string _code = string.Empty;
    private decimal _minOrderAmount;
    private int? _usageLimit;
    private int _usedCount;

    [GeneratedRegex(@"^[A-Z0-9]{4,16}$")]
    private static partial Regex CodeRegex();

    public string Code
    {
        get => _code;
        set => _code = Guard.Pattern(value?.ToUpperInvariant(), "Код купона", CodeRegex(),
            "4–16 латинських літер або цифр.");
    }

    public DateTime ValidFrom { get; set; } = DateTime.Today;

    public DateTime? ValidUntil { get; set; }

    public decimal MinOrderAmount
    {
        get => _minOrderAmount;
        set => _minOrderAmount = Guard.Range(value, "Мінімальна сума", 0m, Product.MaxPrice);
    }

    /// <summary>null — без обмеження кількості використань.</summary>
    public int? UsageLimit
    {
        get => _usageLimit;
        set => _usageLimit = value is null ? null : Guard.Range(value.Value, "Ліміт використань", 1, 100_000);
    }

    public int UsedCount
    {
        get => _usedCount;
        set => _usedCount = Guard.Range(value, "Кількість використань", 0, int.MaxValue);
    }

    public bool IsActive { get; set; } = true;

    [JsonIgnore]
    public abstract string TypeName { get; }

    [JsonIgnore]
    public abstract string ValueText { get; }

    [JsonIgnore]
    public string Description => $"{TypeName}: {ValueText}";

    [JsonIgnore]
    public override string DisplayName => Code;

    public bool IsExpired(DateTime now) => ValidUntil is { } until && now.Date > until.Date;

    [JsonIgnore]
    public bool IsUsageLimitReached => UsageLimit is { } limit && UsedCount >= limit;

    // Шаблонний метод: спільні перевірки тут, специфічні — у нащадках
    public void EnsureApplicable(PricingContext context, DateTime now)
    {
        if (!IsActive)
            throw new InvalidDiscountException("купон деактивовано.", Code);
        if (now.Date < ValidFrom.Date)
            throw new InvalidDiscountException($"діє лише з {ValidFrom:dd.MM.yyyy}.", Code);
        if (IsExpired(now))
            throw new InvalidDiscountException($"термін дії завершився {ValidUntil:dd.MM.yyyy}.", Code);
        if (IsUsageLimitReached)
            throw new InvalidDiscountException("ліміт використань вичерпано.", Code);
        if (context.Subtotal < MinOrderAmount)
            throw new InvalidDiscountException($"мінімальна сума замовлення — {MinOrderAmount:N2} ₴.", Code);

        ValidateSpecific(context);
    }

    public decimal CalculateDiscount(PricingContext context) =>
        Math.Min(decimal.Round(CalculateRawDiscount(context), 2), context.Subtotal);

    public void RegisterUsage() => UsedCount++;

    public void ReleaseUsage()
    {
        if (UsedCount > 0) UsedCount--;
    }

    protected virtual void ValidateSpecific(PricingContext context) { }

    protected abstract decimal CalculateRawDiscount(PricingContext context);
}
