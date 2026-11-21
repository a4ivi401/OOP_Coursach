using ElectroShop.Core;
using ElectroShop.UI.Infrastructure;
using ElectroShop.UI.Screens.Admin;
using ElectroShop.UI.Screens.Customer;
using Spectre.Console;

namespace ElectroShop.UI.Screens;

internal sealed class MainScreen : MenuScreen
{
    private readonly NotificationCenter _notifications;

    public MainScreen(Shop shop, NotificationCenter notifications) : base(shop) => _notifications = notifications;

    protected override string Title => "Головне меню";

    protected override string BackLabel => "Зберегти та вийти";

    protected override void RenderBody()
    {
        AnsiConsole.Write(new FigletText("ElectroShop").Color(Theme.Accent));
        AnsiConsole.MarkupLine($"[{Theme.MutedTag}]Інтернет-магазин електроніки · {Shop.Catalog.GetAllProducts().Count} товарів у " +
                               $"{Shop.Catalog.GetCategories().Count} категоріях · безкоштовна доставка від 5 000 ₴[/]");
    }

    protected override IEnumerable<MenuItem> GetMenu()
    {
        yield return new("Увійти як покупець", Login);
        yield return new("Зареєструватися", Register);
        yield return new(Shop.Admin.IsLocked ? $"Адмін-панель [{Theme.MutedTag}](заблоковано)[/]" : "Адмін-панель", OpenAdmin);
        yield return new("Про програму", AboutView.Show);
    }

    private void Login()
    {
        var demo = Shop.Customers.GetAll().Take(3).Select(c => c.Email).ToList();
        if (demo.Count > 0)
            Ui.Info($"Демо-акаунти: [{Theme.MutedTag}]{string.Join(", ", demo)}[/]");

        var customer = Input.Ask("Email", Shop.Customers.Login);
        new CustomerScreen(Shop, customer).Show();
    }

    private void Register()
    {
        Ui.Header("Реєстрація нового покупця. Порожнє поле — скасувати.");
        var probe = new Core.Models.Customer();

        var name = Input.Ask("ПІБ", s => probe.FullName = s);
        var email = Input.Ask("Email", s =>
        {
            probe.Email = s;
            if (Shop.Customers.IsEmailTaken(probe.Email))
                throw new ArgumentException("Цей email вже зареєстровано — скористайтесь входом.");
            return probe.Email;
        });
        var phone = Input.Ask("Телефон [grey58](+380XXXXXXXXX)[/]", s => probe.Phone = s);
        var address = Input.Ask("Адреса доставки", s => probe.Address = s);

        var customer = Shop.Customers.Register(name, email, phone, address);
        Ui.Success($"Ласкаво просимо, [bold]{Markup.Escape(customer.FullName)}[/]! Акаунт створено.");
        new CustomerScreen(Shop, customer).Show();
    }

    private void OpenAdmin()
    {
        Ui.Info($"PIN для демонстрації: [bold]1234[/]. Спроб: {Shop.Admin.AttemptsLeft}.");
        var pin = Input.Secret("PIN-код адміністратора");
        if (pin.Length == 0)
            throw new InputCancelledException();

        Shop.Admin.Authenticate(pin);
        new AdminScreen(Shop, _notifications).Show();
    }
}
