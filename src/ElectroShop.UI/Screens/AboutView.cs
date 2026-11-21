using ElectroShop.UI.Infrastructure;
using Spectre.Console;

namespace ElectroShop.UI.Screens;

internal static class AboutView
{
    public static void Show()
    {
        Ui.Header();
        var tree = new Tree($"[bold {Theme.AccentTag}]ElectroShop[/] — курсова робота з ООП, варіант 2");

        var core = tree.AddNode("[bold]ElectroShop.Core.dll[/] — бізнес-логіка");
        core.AddNode("Моделі: Product, Category, Customer, Cart, Order, OrderItem");
        core.AddNode("Купони: Coupon → PercentageCoupon / FixedAmountCoupon / CategoryCoupon");
        core.AddNode("Узагальнений репозиторій Repository<T> на List<T>");
        core.AddNode("Власні винятки: OutOfStockException, InvalidDiscountException та ін.");
        core.AddNode("Серіалізація в JSON (System.Text.Json)");

        var ui = tree.AddNode("[bold]ElectroShop.UI[/] — консольний інтерфейс (Spectre.Console)");
        ui.AddNode("Режими покупця та адміністратора");
        ui.AddNode("Перевірка всіх даних, що вводяться");

        tree.AddNode($"[{Theme.MutedTag}]Автор: Бєлінський Олександр Павлович, група Б-F2-25-1-ПІ[/]");

        AnsiConsole.Write(new Panel(tree).Border(BoxBorder.Rounded).BorderColor(Theme.Accent).Padding(2, 1, 2, 1));
        Ui.Pause();
    }
}
