using ElectroShop.Core.Models;
using ElectroShop.Core.Pricing;
using ElectroShop.UI.Infrastructure;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace ElectroShop.UI.Views;

internal static class OrderViews
{
    public static Table Table(IReadOnlyList<Order> orders, Func<Order, string>? customerName = null)
    {
        var table = new Table().Border(TableBorder.Rounded).BorderColor(Theme.Muted).Expand();
        table.AddColumn("[bold]№[/]");
        table.AddColumn("[bold]Дата[/]");
        if (customerName is not null)
            table.AddColumn("[bold]Клієнт[/]");
        table.AddColumn(new TableColumn("[bold]Товарів[/]").RightAligned());
        table.AddColumn("[bold]Доставка[/]");
        table.AddColumn(new TableColumn("[bold]Сума[/]").RightAligned());
        table.AddColumn("[bold]Статус[/]");

        foreach (var order in orders)
        {
            var row = new List<string> { $"[bold]{order.DisplayName}[/]", $"{order.CreatedAt:dd.MM.yyyy HH:mm}" };
            if (customerName is not null)
                row.Add(Markup.Escape(customerName(order)));
            row.Add(order.Items.Sum(i => i.Quantity).ToString());
            row.Add(Markup.Escape(order.Delivery.ToDisplayName()));
            row.Add($"[bold]{Theme.Money(order.Total)}[/]");
            row.Add(Theme.StatusMarkup(order.Status));
            table.AddRow(row.ToArray());
        }

        if (orders.Count == 0)
            table.AddRow($"[{Theme.MutedTag}]Замовлень ще немає[/]");

        return table;
    }

    public static string Label(Order o) =>
        $"{o.DisplayName} [{Theme.MutedTag}]· {o.CreatedAt:dd.MM.yyyy} · {Theme.Money(o.Total)} ·[/] {Theme.StatusMarkup(o.Status)}";

    public static void RenderDetails(Order order, string customerName)
    {
        var info = new Grid().AddColumn(new GridColumn().NoWrap().PadRight(3)).AddColumn();
        info.AddRow($"[{Theme.MutedTag}]Клієнт[/]", Markup.Escape(customerName));
        info.AddRow($"[{Theme.MutedTag}]Створено[/]", $"{order.CreatedAt:dd.MM.yyyy HH:mm}");
        info.AddRow($"[{Theme.MutedTag}]Статус[/]", Theme.StatusMarkup(order.Status));
        info.AddRow($"[{Theme.MutedTag}]Доставка[/]", Markup.Escape(order.Delivery.ToDisplayName()));
        info.AddRow($"[{Theme.MutedTag}]Адреса[/]", Markup.Escape(order.ShippingAddress));
        if (order.CouponCode is not null)
            info.AddRow($"[{Theme.MutedTag}]Купон[/]", $"[mediumpurple1]{order.CouponCode}[/]");

        var items = new Table().Border(TableBorder.Simple).BorderColor(Theme.Muted).Expand()
            .AddColumn("Товар")
            .AddColumn(new TableColumn("Ціна").RightAligned())
            .AddColumn(new TableColumn("К-сть").RightAligned())
            .AddColumn(new TableColumn("Сума").RightAligned());
        foreach (var item in order.Items)
            items.AddRow(Markup.Escape(item.ProductName), Theme.Money(item.UnitPrice), item.Quantity.ToString(), Theme.Money(item.LineTotal));

        var history = string.Join($" [{Theme.MutedTag}]→[/] ",
            order.History.Select(h => $"{Theme.StatusMarkup(h.Status)} [{Theme.MutedTag}]{h.ChangedAt:dd.MM HH:mm}[/]"));

        AnsiConsole.Write(new Panel(new Rows(info, new Text(""), items, Breakdown(order.Pricing), new Text(""),
                new Markup($"[{Theme.MutedTag}]Історія:[/] {history}")))
            .Header($" [bold]Замовлення {order.DisplayName}[/] ")
            .Border(BoxBorder.Rounded)
            .BorderColor(Theme.Accent)
            .Padding(2, 1, 2, 1)
            .Expand());
    }

    public static IRenderable Breakdown(PriceBreakdown pricing)
    {
        var grid = new Grid().AddColumn(new GridColumn().PadRight(4)).AddColumn(new GridColumn().RightAligned());
        grid.AddRow("Вартість товарів", Theme.Money(pricing.Subtotal));
        if (pricing.CouponDiscount > 0)
            grid.AddRow("Знижка за купоном", $"[mediumpurple1]−{Theme.Money(pricing.CouponDiscount)}[/]");
        if (pricing.LoyaltyDiscount > 0)
            grid.AddRow("Знижка постійного клієнта", $"[mediumpurple1]−{Theme.Money(pricing.LoyaltyDiscount)}[/]");
        grid.AddRow("Доставка", pricing.DeliveryCost == 0 ? "[springgreen2]безкоштовно[/]" : Theme.Money(pricing.DeliveryCost));
        grid.AddRow("[bold]До сплати[/]", $"[bold springgreen2]{Theme.Money(pricing.Total)}[/]");
        return new Padder(grid).PadLeft(0);
    }
}
