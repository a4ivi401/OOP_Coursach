using ElectroShop.Core;
using ElectroShop.Core.Models;
using ElectroShop.Core.Services;
using ElectroShop.UI.Infrastructure;
using Spectre.Console;

namespace ElectroShop.UI.Screens.Customer;

internal sealed class CartScreen : MenuScreen
{
    private readonly Core.Models.Customer _customer;
    private IReadOnlyList<CartLine> _lines = [];

    public CartScreen(Shop shop, Core.Models.Customer customer) : base(shop) => _customer = customer;

    protected override string Title => "Кошик";

    protected override void RenderBody()
    {
        _lines = Shop.Carts.GetLines(_customer);
        if (_lines.Count == 0)
        {
            AnsiConsole.Write(new Panel(new Markup($"[{Theme.MutedTag}]Кошик порожній. Загляньте в каталог![/]"))
                .Border(BoxBorder.Rounded).BorderColor(Theme.Muted));
            return;
        }

        AnsiConsole.Write(Table(_lines));

        if (_lines.Any(l => !l.HasEnoughStock))
            Ui.Warning("Деяких товарів на складі менше, ніж у кошику — зменшіть кількість перед оформленням.");
    }

    protected override IEnumerable<MenuItem> GetMenu()
    {
        if (_lines.Count == 0)
            yield break;

        yield return new("Оформити замовлення", () =>
        {
            if (CheckoutFlow.Run(Shop, _customer)) Close();
        });
        yield return new("Змінити кількість", ChangeQuantity);
        yield return new("Видалити товар", RemoveItem);
        yield return new("Очистити кошик", () =>
        {
            if (Input.Confirm("Видалити всі товари з кошика?", false))
                Shop.Carts.Clear(_customer);
        });
    }

    public static Table Table(IReadOnlyList<CartLine> lines)
    {
        var table = new Table().Border(TableBorder.Rounded).BorderColor(Theme.Muted).Expand()
            .AddColumn("[bold]Товар[/]")
            .AddColumn(new TableColumn("[bold]Ціна[/]").RightAligned())
            .AddColumn(new TableColumn("[bold]К-сть[/]").RightAligned())
            .AddColumn(new TableColumn("[bold]Сума[/]").RightAligned());

        foreach (var line in lines)
        {
            var qty = line.HasEnoughStock ? line.Quantity.ToString() : $"[red1]{line.Quantity} (є {line.Product.StockQuantity})[/]";
            table.AddRow(Markup.Escape(line.Product.DisplayName), Theme.Money(line.Product.Price), qty, Theme.Money(line.LineTotal));
        }

        table.AddEmptyRow();
        table.AddRow("[bold]Разом[/]", "", $"[bold]{lines.Sum(l => l.Quantity)}[/]",
            $"[bold springgreen2]{Theme.Money(lines.Sum(l => l.LineTotal))}[/]");
        return table;
    }

    private CartLine? SelectLine(string title) =>
        Input.Select(title, _lines, l => $"{Markup.Escape(l.Product.DisplayName)} [{Theme.MutedTag}]× {l.Quantity}[/]");

    private void ChangeQuantity()
    {
        var line = SelectLine("Кількість якого товару змінити?");
        if (line is null) return;

        var max = Math.Min(CartItem.MaxQuantity, Math.Max(1, line.Product.StockQuantity));
        var quantity = Input.Int($"Нова кількість [{Theme.MutedTag}](1–{max})[/]", 1, CartItem.MaxQuantity, line.Quantity);
        Shop.Carts.SetQuantity(_customer, line.Product.Id, quantity);
    }

    private void RemoveItem()
    {
        var line = SelectLine("Який товар видалити?");
        if (line is not null)
            Shop.Carts.Remove(_customer, line.Product.Id);
    }
}
