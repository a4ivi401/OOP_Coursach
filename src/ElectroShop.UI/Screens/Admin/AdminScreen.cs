using ElectroShop.Core;
using ElectroShop.Core.Models;
using ElectroShop.UI.Infrastructure;
using Spectre.Console;

namespace ElectroShop.UI.Screens.Admin;

internal sealed class AdminScreen : MenuScreen
{
    private readonly NotificationCenter _notifications;

    public AdminScreen(Shop shop, NotificationCenter notifications) : base(shop) => _notifications = notifications;

    protected override string Title => "Адмін-панель";

    protected override string BackLabel => "← Вийти з адмін-панелі";

    protected override void RenderBody()
    {
        var summary = Shop.Reports.GetSummary();
        var newOrders = Shop.Orders.GetAll(OrderStatus.New).Count;
        var lowStock = Shop.Catalog.GetLowStock().Count;

        Ui.CardRow(
            Kpi("Виручка", Theme.MoneyShort(summary.Revenue), "springgreen2"),
            Kpi("Замовлень", summary.OrdersCount.ToString(), Theme.AccentTag),
            Kpi("Нові замовлення", newOrders.ToString(), newOrders > 0 ? "orange1" : Theme.AccentTag),
            Kpi("Закінчуються", lowStock.ToString(), lowStock > 0 ? "red1" : Theme.AccentTag));

        if (_notifications.Items.Count > 0)
        {
            var text = string.Join("\n", _notifications.Items.TakeLast(5)
                .Select(n => $"[{Theme.MutedTag}]{n.At:HH:mm}[/] [orange1]⚠[/] {Markup.Escape(n.Message)}"));
            AnsiConsole.Write(new Panel(new Markup(text)).Header(" [bold]Сповіщення за сесію[/] ")
                .Border(BoxBorder.Rounded).BorderColor(Theme.Warning).Expand());
        }

        var saved = Shop.LastSavedAt is { } at ? $"{at:dd.MM.yyyy HH:mm:ss}" : "ще не зберігалось";
        AnsiConsole.MarkupLine($"[{Theme.MutedTag}]Файл даних: {Markup.Escape(Shop.StorageLocation)} · збережено: {saved}[/]");
    }

    protected override IEnumerable<MenuItem> GetMenu()
    {
        yield return new("Товари та склад", () => new ProductsAdminScreen(Shop).Show());
        yield return new("Категорії", () => new CategoriesAdminScreen(Shop).Show());
        yield return new("Замовлення", () => new OrdersAdminScreen(Shop).Show());
        yield return new("Купони на знижку", () => new CouponsAdminScreen(Shop).Show());
        yield return new("Клієнти", () => new CustomersAdminScreen(Shop).Show());
        yield return new("Звіти та статистика", () => ReportsView.Show(Shop));
        yield return new("Зберегти дані зараз", () =>
        {
            AnsiConsole.Status().Spinner(Spinner.Known.Dots).Start("Збереження...", _ => Shop.Save());
            Ui.Success($"Дані збережено у файл [underline]{Markup.Escape(Shop.StorageLocation)}[/]");
        });
        if (_notifications.Items.Count > 0)
            yield return new("Очистити сповіщення", _notifications.Clear);
    }

    private static Panel Kpi(string title, string value, string color) =>
        Ui.Card(title, $"[bold {color}]{value}[/]", centered: true);
}
