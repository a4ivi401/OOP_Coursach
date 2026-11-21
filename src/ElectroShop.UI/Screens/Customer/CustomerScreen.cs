using ElectroShop.Core;
using ElectroShop.Core.Models;
using ElectroShop.Core.Pricing;
using ElectroShop.UI.Infrastructure;
using ElectroShop.UI.Views;
using Spectre.Console;

namespace ElectroShop.UI.Screens.Customer;

internal sealed class CustomerScreen : MenuScreen
{
    private readonly Core.Models.Customer _customer;

    public CustomerScreen(Shop shop, Core.Models.Customer customer) : base(shop) => _customer = customer;

    protected override string Title => _customer.FullName;

    protected override string BackLabel => "← Вийти з акаунта";

    protected override void RenderBody()
    {
        var level = _customer.LoyaltyLevel;
        var firstName = _customer.FullName.Split(' ')[0];
        var cartLines = Shop.Carts.GetLines(_customer);
        var activeOrders = Shop.Orders.GetForCustomer(_customer.Id)
            .Count(o => o.Status is OrderStatus.New or OrderStatus.Paid or OrderStatus.Shipped);

        AnsiConsole.MarkupLine($"Вітаємо, [bold {Theme.AccentTag}]{Markup.Escape(firstName)}[/]!\n");
        Ui.CardRow(
            Ui.Card("Рівень лояльності", $"{Theme.LoyaltyMarkup(level)}\n[{Theme.MutedTag}]знижка {LoyaltyProgram.GetPercent(level)}%[/]"),
            Ui.Card("Кошик", $"[bold]{cartLines.Sum(l => l.Quantity)} шт.[/]\n[{Theme.MutedTag}]{Theme.Money(cartLines.Sum(l => l.LineTotal))}[/]"),
            Ui.Card("Активні замовлення", $"[bold]{activeOrders}[/]\n[{Theme.MutedTag}]усього покупок на {Theme.MoneyShort(_customer.TotalSpent)}[/]"));
    }

    protected override IEnumerable<MenuItem> GetMenu()
    {
        var count = _customer.Cart.TotalQuantity;
        yield return new("Каталог товарів", () => new CatalogScreen(Shop, _customer).Show());
        yield return new(count > 0 ? $"Кошик [{Theme.AccentTag}]({count})[/]" : "Кошик", () => new CartScreen(Shop, _customer).Show());
        yield return new("Оформити замовлення", () => CheckoutFlow.Run(Shop, _customer));
        yield return new("Мої замовлення", () => new MyOrdersScreen(Shop, _customer).Show());
        yield return new("Акції та купони", ShowCoupons);
        yield return new("Профіль і програма лояльності", () => new ProfileScreen(Shop, _customer).Show());
    }

    private void ShowCoupons()
    {
        Ui.Header("Введіть код купона під час оформлення замовлення.");
        AnsiConsole.Write(CouponViews.Table(Shop.Coupons.GetAvailable(), DateTime.Now, adminMode: false));
        Ui.Pause();
    }

}
