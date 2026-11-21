using ElectroShop.Core;
using ElectroShop.Core.Exceptions;
using ElectroShop.Core.Models;
using ElectroShop.Core.Pricing;
using ElectroShop.UI.Infrastructure;
using ElectroShop.UI.Views;
using Spectre.Console;

namespace ElectroShop.UI.Screens.Customer;

/// <summary>Покрокове оформлення: кошик → доставка → купон → підсумок → підтвердження.</summary>
internal static class CheckoutFlow
{
    /// <returns>true, якщо замовлення створено.</returns>
    public static bool Run(Shop shop, Core.Models.Customer customer)
    {
        Ui.PushScreen("Оформлення замовлення");
        try
        {
            Ui.Header();
            var lines = shop.Carts.GetLines(customer);
            if (lines.Count == 0)
                throw new EmptyCartException();

            Ui.Section("1. Ваше замовлення");
            AnsiConsole.Write(CartScreen.Table(lines));

            // Перевіряємо наявність одразу, щоб не змушувати користувача проходити всі кроки даремно
            shop.Orders.Preview(customer, DeliveryMethod.Pickup);

            Ui.Section("2. Доставка");
            var delivery = Input.Choose("Спосіб доставки", Enum.GetValues<DeliveryMethod>(), DeliveryLabel);
            AnsiConsole.MarkupLine($"[{Theme.AccentTag}]›[/] {DeliveryLabel(delivery)}");

            Ui.Section("3. Купон");
            var available = shop.Coupons.GetAvailable();
            if (available.Count > 0)
                AnsiConsole.MarkupLine($"[{Theme.MutedTag}]Діючі купони:[/] " +
                                       string.Join(", ", available.Select(c => $"[mediumpurple1]{c.Code}[/]")));

            string? couponCode = null;
            if (Input.Confirm("Маєте купон на знижку?", false))
            {
                // Валідація купона прямо в полі вводу: невдалий код запитується повторно
                couponCode = Input.Ask("Код купона", code =>
                {
                    shop.Orders.Preview(customer, delivery, code);
                    return code.ToUpperInvariant();
                });
            }

            var pricing = shop.Orders.Preview(customer, delivery, couponCode);

            Ui.Section("4. Підсумок");
            AnsiConsole.Write(new Panel(OrderViews.Breakdown(pricing))
                .Header(" [bold]Розрахунок вартості[/] ").Border(BoxBorder.Rounded).BorderColor(Theme.Accent));
            AnsiConsole.MarkupLine($"[{Theme.MutedTag}]Адреса:[/] {(delivery == DeliveryMethod.Pickup ? "самовивіз з магазину" : Markup.Escape(customer.Address))}");
            if (pricing.LoyaltyDiscount > 0)
                AnsiConsole.MarkupLine($"[{Theme.MutedTag}]Застосовано знижку рівня[/] {Theme.LoyaltyMarkup(customer.LoyaltyLevel)}");

            var levelBefore = customer.LoyaltyLevel;
            if (!Input.Confirm($"Підтвердити замовлення на [bold]{Theme.Money(pricing.Total)}[/]?"))
                throw new InputCancelledException();

            var order = shop.Orders.PlaceOrder(customer, delivery, couponCode);

            var message = $"Замовлення [bold]{order.DisplayName}[/] оформлено! Сума: [bold]{Theme.Money(order.Total)}[/].";
            if (customer.LoyaltyLevel != levelBefore)
                message += $"\n[bold]Вітаємо![/] Ваш новий рівень лояльності: {Theme.LoyaltyMarkup(customer.LoyaltyLevel)}";
            Ui.Success(message);
            return true;
        }
        finally
        {
            Ui.PopScreen();
        }
    }

    private static string DeliveryLabel(DeliveryMethod method)
    {
        var cost = PriceCalculator.GetDeliveryBaseCost(method);
        var price = cost == 0 ? "безкоштовно" : $"{Theme.MoneyShort(cost)}, безкоштовно від {Theme.MoneyShort(PriceCalculator.FreeDeliveryThreshold)}";
        return $"{method.ToDisplayName()} [{Theme.MutedTag}]({price})[/]";
    }
}
