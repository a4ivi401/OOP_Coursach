using ElectroShop.Core;
using ElectroShop.Core.Models;
using ElectroShop.UI.Infrastructure;
using ElectroShop.UI.Views;
using Spectre.Console;

namespace ElectroShop.UI.Screens.Customer;

internal sealed class MyOrdersScreen : MenuScreen
{
    private readonly Core.Models.Customer _customer;
    private IReadOnlyList<Order> _orders = [];

    public MyOrdersScreen(Shop shop, Core.Models.Customer customer) : base(shop) => _customer = customer;

    protected override string Title => "Мої замовлення";

    protected override void RenderBody()
    {
        _orders = Shop.Orders.GetForCustomer(_customer.Id);
        AnsiConsole.Write(OrderViews.Table(_orders));
    }

    protected override IEnumerable<MenuItem> GetMenu()
    {
        if (_orders.Count == 0) yield break;

        yield return new("Деталі замовлення", () =>
        {
            var order = Input.Select("Оберіть замовлення", _orders, OrderViews.Label);
            if (order is null) return;
            Ui.Header();
            OrderViews.RenderDetails(order, _customer.FullName);
            Ui.Pause();
        });

        yield return new("Скасувати замовлення", () =>
        {
            var cancellable = _orders.Where(o => o.CanBeCancelled).ToList();
            var order = Input.Select("Яке замовлення скасувати? (можна лише нові та оплачені)", cancellable, OrderViews.Label);
            if (order is null || !Input.Confirm($"Скасувати замовлення {order.DisplayName}?", false)) return;
            Shop.Orders.Cancel(order);
            Ui.Success($"Замовлення {order.DisplayName} скасовано, товари повернуто на склад.");
        });
    }
}
