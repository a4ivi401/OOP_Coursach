using ElectroShop.Core;
using ElectroShop.Core.Models;
using ElectroShop.UI.Infrastructure;
using Spectre.Console;

namespace ElectroShop.UI.Screens.Admin;

internal static class ReportsView
{
    private static readonly Color[] Palette =
    [
        Color.DeepSkyBlue1, Color.MediumPurple1, Color.SpringGreen2, Color.Orange1, Color.HotPink, Color.Gold1, Color.Aquamarine1
    ];

    public static void Show(Shop shop)
    {
        Ui.PushScreen("Звіти");
        try
        {
            Ui.Header("Скасовані замовлення не враховуються у виручці.");
            var s = shop.Reports.GetSummary();

            Ui.CardRow(Kpi("Виручка", Theme.MoneyShort(s.Revenue)), Kpi("Середній чек", Theme.MoneyShort(s.AverageCheck)),
                Kpi("Продано одиниць", s.ItemsSold.ToString()), Kpi("Надано знижок", Theme.MoneyShort(s.TotalDiscounts)));
            Ui.CardRow(Kpi("Замовлень", s.OrdersCount.ToString()), Kpi("Скасовано", s.CancelledCount.ToString()),
                Kpi("Клієнтів", s.CustomersCount.ToString()), Kpi("Вартість складу", Theme.MoneyShort(s.InventoryValue)));

            var byCategory = shop.Reports.GetRevenueByCategory();
            if (byCategory.Count > 0)
            {
                var chart = new BarChart().Width(70).UseValueFormatter((v, _) => Theme.MoneyShort((decimal)v));
                for (var i = 0; i < byCategory.Count; i++)
                    chart.AddItem(Markup.Escape(byCategory[i].CategoryName), (double)byCategory[i].Revenue, Palette[i % Palette.Length]);
                AnsiConsole.Write(new Panel(chart).Header(" [bold]Виручка за категоріями[/] ").Border(BoxBorder.Rounded)
                    .BorderColor(Theme.Muted).Expand());
            }

            var byStatus = shop.Reports.GetOrdersByStatus().Where(p => p.Value > 0).ToList();
            if (byStatus.Count > 0)
            {
                var total = byStatus.Sum(p => p.Value);
                var breakdown = new BreakdownChart().FullSize().HideTagValues();
                foreach (var (status, count) in byStatus)
                    breakdown.AddItem($"{status.ToDisplayName()}: {count} ({(double)count / total:P0})", count, StatusColor(status));
                AnsiConsole.Write(new Panel(breakdown).Header(" [bold]Замовлення за статусами[/] ").Border(BoxBorder.Rounded)
                    .BorderColor(Theme.Muted).Expand());
            }

            var top = new Table().Border(TableBorder.Rounded).BorderColor(Theme.Muted).Expand()
                .Title("[bold]Топ-5 товарів за продажами[/]")
                .AddColumn(new TableColumn("#").RightAligned())
                .AddColumn("Товар")
                .AddColumn(new TableColumn("Продано").RightAligned())
                .AddColumn(new TableColumn("Виручка").RightAligned());
            var rank = 1;
            foreach (var p in shop.Reports.GetTopProducts(5))
                top.AddRow($"[{Theme.AccentTag}]{rank++}[/]", Markup.Escape(p.ProductName), $"{p.Quantity} шт.", Theme.Money(p.Revenue));

            var lowStock = new Table().Border(TableBorder.Rounded).BorderColor(Theme.Warning).Expand()
                .Title("[bold orange1]Потребують поповнення[/]")
                .AddColumn("Товар")
                .AddColumn(new TableColumn("Залишок").RightAligned());
            foreach (var p in shop.Catalog.GetLowStock())
                lowStock.AddRow(Markup.Escape(p.DisplayName), Theme.StockMarkup(p));

            AnsiConsole.Write(new Columns(top, lowStock));
            Ui.Pause();
        }
        finally
        {
            Ui.PopScreen();
        }
    }

    private static Panel Kpi(string title, string value) => Ui.Card(title, $"[bold]{value}[/]", centered: true);

    private static Color StatusColor(OrderStatus status) => status switch
    {
        OrderStatus.New => Color.DeepSkyBlue1,
        OrderStatus.Paid => Color.MediumPurple1,
        OrderStatus.Shipped => Color.Orange1,
        OrderStatus.Delivered => Color.SpringGreen2,
        _ => Color.Grey50
    };
}
