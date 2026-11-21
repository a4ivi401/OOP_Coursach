using ElectroShop.Core.Models;
using ElectroShop.Core.Services;
using ElectroShop.UI.Infrastructure;
using Spectre.Console;

namespace ElectroShop.UI.Views;

internal static class ProductViews
{
    public static Table Table(IReadOnlyList<Product> products, CatalogService catalog, bool adminMode = false)
    {
        var table = new Table().Border(TableBorder.Rounded).BorderColor(Theme.Muted).Expand();
        table.AddColumn(new TableColumn("[bold]#[/]").RightAligned());
        table.AddColumn("[bold]Артикул[/]");
        table.AddColumn("[bold]Товар[/]");
        table.AddColumn("[bold]Категорія[/]");
        table.AddColumn(new TableColumn("[bold]Ціна[/]").RightAligned());
        table.AddColumn(new TableColumn("[bold]Наявність[/]").RightAligned());
        if (adminMode)
            table.AddColumn(new TableColumn("[bold]Вартість залишку[/]").RightAligned());

        for (var i = 0; i < products.Count; i++)
        {
            var p = products[i];
            var row = new List<string>
            {
                $"[{Theme.MutedTag}]{i + 1}[/]",
                $"[{Theme.MutedTag}]{p.Sku}[/]",
                $"[bold]{Markup.Escape(p.Brand)}[/] {Markup.Escape(p.Name)}",
                Markup.Escape(catalog.GetCategoryName(p.CategoryId)),
                $"[bold]{Theme.Money(p.Price)}[/]",
                Theme.StockMarkup(p)
            };
            if (adminMode)
                row.Add(Theme.MoneyShort(p.Price * p.StockQuantity));
            table.AddRow(row.ToArray());
        }

        if (products.Count == 0)
            table.AddRow("", "", $"[{Theme.MutedTag}]Нічого не знайдено[/]", "", "", "");

        return table;
    }

    public static Panel Details(Product product, CatalogService catalog)
    {
        var grid = new Grid().AddColumn(new GridColumn().NoWrap().PadRight(3)).AddColumn();
        grid.AddRow($"[{Theme.MutedTag}]Артикул[/]", product.Sku);
        grid.AddRow($"[{Theme.MutedTag}]Категорія[/]", Markup.Escape(catalog.GetCategoryName(product.CategoryId)));
        grid.AddRow($"[{Theme.MutedTag}]Ціна[/]", $"[bold springgreen2]{Theme.Money(product.Price)}[/]");
        grid.AddRow($"[{Theme.MutedTag}]Наявність[/]", Theme.StockMarkup(product));
        grid.AddRow($"[{Theme.MutedTag}]Гарантія[/]", product.WarrantyMonths == 0 ? "без гарантії" : $"{product.WarrantyMonths} міс.");

        var rows = new List<Spectre.Console.Rendering.IRenderable> { grid };

        if (!string.IsNullOrWhiteSpace(product.Description))
        {
            rows.Add(new Text(""));
            rows.Add(new Markup($"[italic]{Markup.Escape(product.Description)}[/]"));
        }

        if (product.Specifications.Count > 0)
        {
            var specs = new Table().Border(TableBorder.Simple).BorderColor(Theme.Muted)
                .AddColumn($"[{Theme.MutedTag}]Характеристика[/]").AddColumn($"[{Theme.MutedTag}]Значення[/]");
            foreach (var (key, value) in product.Specifications)
                specs.AddRow(Markup.Escape(key), Markup.Escape(value));
            rows.Add(specs);
        }

        return new Panel(new Rows(rows))
            .Header($" [bold]{Markup.Escape(product.DisplayName)}[/] ")
            .Border(BoxBorder.Rounded)
            .BorderColor(Theme.Accent)
            .Padding(2, 1, 2, 1);
    }

    public static string Label(Product p) =>
        $"{Markup.Escape(p.DisplayName)} [{Theme.MutedTag}]· {p.Sku} · {Theme.Money(p.Price)} ·[/] {Theme.StockMarkup(p)}";
}
