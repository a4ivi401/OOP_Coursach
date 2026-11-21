using ElectroShop.Core;
using ElectroShop.Core.Models.Coupons;
using ElectroShop.UI.Infrastructure;
using ElectroShop.UI.Views;
using Spectre.Console;

namespace ElectroShop.UI.Screens.Admin;

internal sealed class CouponsAdminScreen : MenuScreen
{
    private IReadOnlyList<Coupon> _coupons = [];

    public CouponsAdminScreen(Shop shop) : base(shop) { }

    protected override string Title => "Купони";

    protected override void RenderBody()
    {
        _coupons = Shop.Coupons.GetAll();
        AnsiConsole.Write(CouponViews.Table(_coupons, DateTime.Now, adminMode: true));
    }

    protected override IEnumerable<MenuItem> GetMenu()
    {
        yield return new("Створити купон", Create);
        yield return new("Активувати / деактивувати", () =>
        {
            var coupon = Input.Select("Оберіть купон", _coupons, CouponViews.Label);
            if (coupon is null) return;
            Shop.Coupons.SetActive(coupon, !coupon.IsActive);
            Ui.Success($"Купон {coupon.Code} {(coupon.IsActive ? "активовано" : "деактивовано")}.");
        });
        yield return new("Видалити купон", () =>
        {
            var coupon = Input.Select("Який купон видалити?", _coupons, CouponViews.Label);
            if (coupon is null || !Input.Confirm($"Видалити купон {coupon.Code}?", false)) return;
            Shop.Coupons.Remove(coupon);
            Ui.Success("Купон видалено.");
        });
    }

    private void Create()
    {
        var type = Input.Choose("Тип купона",
            ["Відсоткова знижка на все замовлення", "Фіксована сума знижки", "Відсоткова знижка на категорію"], s => s);

        // Конкретний тип обирається під час виконання — далі працюємо з ним через базовий клас
        Coupon coupon = type switch
        {
            "Фіксована сума знижки" => new FixedAmountCoupon(),
            "Відсоткова знижка на категорію" => new CategoryCoupon(),
            _ => new PercentageCoupon()
        };

        Input.Ask("Код [grey58](4–16 латинських літер/цифр)[/]", s =>
        {
            coupon.Code = s;
            if (Shop.Coupons.FindByCode(coupon.Code) is not null)
                throw new ArgumentException($"Купон {coupon.Code} вже існує.");
            return s;
        });

        switch (coupon)
        {
            case PercentageCoupon percent:
                Input.Ask("Відсоток знижки (1–50)", s => percent.Percent = Input.ParseDecimal(s));
                break;
            case FixedAmountCoupon fixedAmount:
                Input.Ask("Сума знижки, ₴", s => fixedAmount.Amount = Input.ParseDecimal(s));
                break;
            case CategoryCoupon category:
                var target = Input.Choose("Категорія", Shop.Catalog.GetCategories(), c => Markup.Escape(c.Name));
                category.CategoryId = target.Id;
                category.CategoryName = target.Name;
                Input.Ask("Відсоток знижки на категорію (1–70)", s => category.Percent = Input.ParseDecimal(s));
                break;
        }

        coupon.MinOrderAmount = Input.Decimal("Мінімальна сума замовлення, ₴", 0, 1_000_000, 0, editable: false);
        coupon.ValidFrom = Input.Date("Діє з", DateTime.Today, editable: false);
        coupon.ValidUntil = Input.OptionalDate("Обмежити термін дії?", "Діє до");
        if (Input.Confirm("Обмежити кількість використань?", false))
            coupon.UsageLimit = Input.Int("Максимум використань", 1, 100_000);

        Shop.Coupons.Add(coupon);
        Ui.Success($"Купон [mediumpurple1]{coupon.Code}[/] створено: {Markup.Escape(coupon.Description)}.");
    }
}
