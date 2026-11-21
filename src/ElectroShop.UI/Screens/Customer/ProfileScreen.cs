using ElectroShop.Core;
using ElectroShop.Core.Pricing;
using ElectroShop.UI.Infrastructure;
using Spectre.Console;

namespace ElectroShop.UI.Screens.Customer;

internal sealed class ProfileScreen : MenuScreen
{
    private readonly Core.Models.Customer _customer;

    public ProfileScreen(Shop shop, Core.Models.Customer customer) : base(shop) => _customer = customer;

    protected override string Title => "Профіль";

    protected override void RenderBody()
    {
        var grid = new Grid().AddColumn(new GridColumn().NoWrap().PadRight(3)).AddColumn();
        grid.AddRow($"[{Theme.MutedTag}]ПІБ[/]", Markup.Escape(_customer.FullName));
        grid.AddRow($"[{Theme.MutedTag}]Email[/]", Markup.Escape(_customer.Email));
        grid.AddRow($"[{Theme.MutedTag}]Телефон[/]", _customer.Phone);
        grid.AddRow($"[{Theme.MutedTag}]Адреса[/]", Markup.Escape(_customer.Address));
        grid.AddRow($"[{Theme.MutedTag}]З нами з[/]", $"{_customer.RegisteredAt:dd.MM.yyyy}");

        AnsiConsole.Write(new Panel(grid).Header(" [bold]Особисті дані[/] ").Border(BoxBorder.Rounded)
            .BorderColor(Theme.Accent).Padding(2, 1, 2, 1).Expand());
        AnsiConsole.Write(LoyaltyPanel());
    }

    protected override IEnumerable<MenuItem> GetMenu()
    {
        yield return new("Редагувати профіль", Edit);
    }

    private Panel LoyaltyPanel()
    {
        var level = _customer.LoyaltyLevel;
        var rows = new List<Spectre.Console.Rendering.IRenderable>
        {
            new Markup($"Ваш рівень: {Theme.LoyaltyMarkup(level)} — знижка [bold]{LoyaltyProgram.GetPercent(level)}%[/] на кожне замовлення"),
            new Markup($"[{Theme.MutedTag}]Сума покупок:[/] {Theme.Money(_customer.TotalSpent)}")
        };

        if (LoyaltyProgram.GetNextLevel(level) is { } next)
        {
            var from = LoyaltyProgram.GetThreshold(level);
            var to = LoyaltyProgram.GetThreshold(next);
            var progress = (double)((_customer.TotalSpent - from) / (to - from));
            rows.Add(new Text(""));
            rows.Add(new Markup($"До рівня {Theme.LoyaltyMarkup(next)} ({LoyaltyProgram.GetPercent(next)}%) залишилось [bold]{Theme.Money(to - _customer.TotalSpent)}[/]"));
            rows.Add(new Markup(ProgressBar(progress, 40)));
        }
        else
        {
            rows.Add(new Markup("[aquamarine1]Ви досягли максимального рівня. Дякуємо, що ви з нами![/]"));
        }

        return new Panel(new Rows(rows)).Header(" [bold]Програма лояльності[/] ").Border(BoxBorder.Rounded)
            .BorderColor(Theme.Secondary).Padding(2, 1, 2, 1).Expand();
    }

    private static string ProgressBar(double progress, int width)
    {
        var filled = (int)Math.Round(Math.Clamp(progress, 0, 1) * width);
        return $"[mediumpurple1]{new string('█', filled)}[/][grey23]{new string('█', width - filled)}[/] {progress:P0}";
    }

    private void Edit()
    {
        Ui.Info("Відредагуйте значення або натисніть Enter, щоб залишити без змін.");
        var probe = new Core.Models.Customer();
        var name = Input.Ask("ПІБ", s => probe.FullName = s, _customer.FullName);
        var phone = Input.Ask("Телефон", s => probe.Phone = s, _customer.Phone);
        var address = Input.Ask("Адреса доставки", s => probe.Address = s, _customer.Address);

        Shop.Customers.UpdateProfile(_customer, name, phone, address);
        Ui.Success("Профіль оновлено.");
    }
}
