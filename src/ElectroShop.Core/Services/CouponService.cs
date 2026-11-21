using ElectroShop.Core.Exceptions;
using ElectroShop.Core.Models.Coupons;
using ElectroShop.Core.Persistence;
using ElectroShop.Core.Pricing;

namespace ElectroShop.Core.Services;

public sealed class CouponService
{
    private readonly ShopContext _context;
    private readonly TimeProvider _time;

    public CouponService(ShopContext context, TimeProvider time)
    {
        _context = context;
        _time = time;
    }

    public IReadOnlyList<Coupon> GetAll() => _context.Coupons.OrderBy(c => c.Code).ToList();

    public IReadOnlyList<Coupon> GetAvailable()
    {
        var now = _time.GetLocalNow().DateTime;
        return _context.Coupons
            .Where(c => c.IsActive && !c.IsExpired(now) && !c.IsUsageLimitReached && c.ValidFrom.Date <= now.Date)
            .OrderBy(c => c.Code).ToList();
    }

    public Coupon? FindByCode(string code)
    {
        var normalized = code.Trim().ToUpperInvariant();
        return _context.Coupons.FirstOrDefault(c => c.Code == normalized);
    }

    public void Add(Coupon coupon)
    {
        if (coupon.ValidUntil is { } until && until.Date < coupon.ValidFrom.Date)
            throw new InvalidDiscountException("дата завершення раніша за дату початку дії.", coupon.Code);
        if (FindByCode(coupon.Code) is not null)
            throw new DuplicateEntityException("Купон", coupon.Code);
        _context.Coupons.Add(coupon);
    }

    public void SetActive(Coupon coupon, bool isActive) => coupon.IsActive = isActive;

    public void Remove(Coupon coupon)
    {
        if (coupon.UsedCount > 0)
            throw new EntityInUseException($"Купон {coupon.Code} вже використано в замовленнях — його можна лише деактивувати.");
        _context.Coupons.Remove(coupon.Id);
    }

    /// <summary>Повертає купон, якщо його можна застосувати, інакше кидає InvalidDiscountException.</summary>
    public Coupon Resolve(string code, PricingContext context)
    {
        var coupon = FindByCode(code) ?? throw new InvalidDiscountException("такого купона не існує.", code.Trim().ToUpperInvariant());
        coupon.EnsureApplicable(context, _time.GetLocalNow().DateTime);
        return coupon;
    }
}
