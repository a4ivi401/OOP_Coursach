using ElectroShop.Core;
using ElectroShop.UI.Infrastructure;
using ElectroShop.UI.Views;
using Spectre.Console;

namespace ElectroShop.UI.Screens.Admin;

internal sealed class CustomersAdminScreen : MenuScreen
{
    private IReadOnlyList<Core.Models.Customer> _customers = [];

    public CustomersAdminScreen(Shop shop) : base(shop) { }

    protected override string Title => "Клієнти";

    protected override void RenderBody()
    {
        _customers = Shop.Customers.GetAll();
        var table = new Table().Border(TableBorder.Rounded).BorderColor(Theme.Muted).Expand()
            .AddColumn("[bold]ПІБ[/]")
            .AddColumn("[bold]Email[/]")
            .AddColumn("[bold]Телефон[/]")
            .AddColumn("[bold]Рівень[/]")
            .AddColumn(new TableColumn("[bold]Замовлень[/]").RightAligned())
            .AddColumn(new TableColumn("[bold]Сума покупок[/]").RightAligned());

        foreach (var c in _customers)
            table.AddRow($"[bold]{Markup.Escape(c.FullName)}[/]", Markup.Escape(c.Email), c.Phone,
                Theme.LoyaltyMarkup(c.LoyaltyLevel), Shop.Customers.CountOrders(c.Id).ToString(), Theme.Money(c.TotalSpent));

        AnsiConsole.Write(table);
        AnsiConsole.MarkupLine($"[{Theme.MutedTag}]Відсортовано за сумою покупок (CustomerSpendingComparer)[/]");
    }

    protected override IEnumerable<MenuItem> GetMenu()
    {
        yield return new("Замовлення клієнта", () =>
        {
            var customer = Input.Select("Оберіть клієнта", _customers, c => Markup.Escape(c.DisplayName), searchable: true);
            if (customer is null) return;
            Ui.Header();
            AnsiConsole.MarkupLine($"[bold]{Markup.Escape(customer.FullName)}[/] · {Theme.LoyaltyMarkup(customer.LoyaltyLevel)} · " +
                                   $"{Markup.Escape(customer.Address)}\n");
            AnsiConsole.Write(OrderViews.Table(Shop.Orders.GetForCustomer(customer.Id)));
            Ui.Pause();
        });
    }
}
