using ElectroShop.Core;
using ElectroShop.Core.Models;
using ElectroShop.UI.Infrastructure;
using ElectroShop.UI.Views;
using Spectre.Console;

namespace ElectroShop.UI.Screens.Admin;

internal sealed class OrdersAdminScreen : MenuScreen
{
    private OrderStatus? _statusFilter;
    private IReadOnlyList<Order> _orders = [];

    public OrdersAdminScreen(Shop shop) : base(shop) { }

    protected override string Title => "Замовлення";

    protected override void RenderBody()
    {
        _orders = Shop.Orders.GetAll(_statusFilter);
        AnsiConsole.Write(OrderViews.Table(_orders, Shop.Orders.GetCustomerName));
        AnsiConsole.MarkupLine($"[{Theme.MutedTag}]Показано: {(_statusFilter is { } s ? s.ToDisplayName().ToLower() : "усі")} · {_orders.Count} шт.[/]");
    }

    protected override IEnumerable<MenuItem> GetMenu()
    {
        yield return new("Деталі замовлення", () =>
        {
            var order = Pick("Оберіть замовлення");
            if (order is null) return;
            Ui.Header();
            OrderViews.RenderDetails(order, Shop.Orders.GetCustomerName(order));
            Ui.Pause();
        });
        yield return new("Змінити статус", ChangeStatus);
        yield return new("Знайти за номером", () =>
        {
            var number = Input.Int("Номер замовлення", 1, 99_999);
            var order = Shop.Orders.GetByNumber(number);
            Ui.Header();
            OrderViews.RenderDetails(order, Shop.Orders.GetCustomerName(order));
            Ui.Pause();
        });
        yield return new("Фільтр за статусом", () =>
        {
            const int all = -1;
            var options = Enum.GetValues<OrderStatus>().Select(s => (int)s).Prepend(all);
            var choice = Input.Choose("Показати замовлення", options,
                i => i == all ? "Усі замовлення" : Theme.StatusMarkup((OrderStatus)i));
            _statusFilter = choice == all ? null : (OrderStatus)choice;
        });
    }

    private Order? Pick(string title) => Input.Select(title, _orders, OrderViews.Label, searchable: true);

    private void ChangeStatus()
    {
        var order = Pick("Статус якого замовлення змінити?");
        if (order is null) return;

        var next = order.Status.NextStatuses();
        if (next.Count == 0)
        {
            Ui.Info($"Замовлення у статусі {Theme.StatusMarkup(order.Status)} вже завершене — змінити статус неможливо.");
            Ui.Pause();
            return;
        }

        var status = Input.Choose($"Новий статус для {order.DisplayName} (зараз {Theme.StatusMarkup(order.Status)})", next, Theme.StatusMarkup);
        if (status == OrderStatus.Cancelled && !Input.Confirm("Скасувати замовлення? Товари повернуться на склад.", false))
            return;

        Shop.Orders.ChangeStatus(order, status);
        Ui.Success($"Замовлення {order.DisplayName}: {Theme.StatusMarkup(status)}");
    }
}
