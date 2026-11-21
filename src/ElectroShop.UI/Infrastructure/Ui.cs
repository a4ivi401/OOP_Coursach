using ElectroShop.Core.Exceptions;
using Spectre.Console;

namespace ElectroShop.UI.Infrastructure;

internal static class Ui
{
    private static readonly Stack<string> Breadcrumbs = new();

    public static void PushScreen(string title) => Breadcrumbs.Push(title);

    public static void PopScreen() => Breadcrumbs.TryPop(out _);

    public static void Header(string? subtitle = null)
    {
        AnsiConsole.Clear();
        var path = string.Join($" [{Theme.MutedTag}]›[/] ", Breadcrumbs.Reverse().Select(Markup.Escape));
        var title = $"[bold {Theme.AccentTag}]⚡ ElectroShop[/]" + (path.Length > 0 ? $" [{Theme.MutedTag}]›[/] [bold]{path}[/]" : "");
        AnsiConsole.Write(new Rule(title) { Justification = Justify.Left, Style = new Style(Theme.Accent) });
        if (subtitle is not null)
            AnsiConsole.MarkupLine($"[{Theme.MutedTag}]{subtitle}[/]");
        AnsiConsole.WriteLine();
    }

    public static void Section(string title) =>
        AnsiConsole.Write(new Rule($"[bold]{title}[/]") { Justification = Justify.Left, Style = new Style(Theme.Muted) });

    public static Panel Card(string title, string body, bool centered = false)
    {
        var markup = new Markup(body);
        return new Panel(centered ? markup.Centered() : markup)
            .Header($" [{Theme.MutedTag}]{title}[/] ")
            .Border(BoxBorder.Rounded)
            .BorderColor(Theme.Muted)
            .Expand();
    }

    /// <summary>Ряд карток однакової ширини на всю ширину консолі.</summary>
    public static void CardRow(params Panel[] cards)
    {
        var width = (AnsiConsole.Profile.Width - 2 * (cards.Length - 1)) / cards.Length;
        var grid = new Grid();
        foreach (var _ in cards)
            grid.AddColumn(new GridColumn { Width = width, Padding = new Padding(0, 0, 2, 0) });
        grid.AddRow(cards);
        AnsiConsole.Write(grid);
    }

    public static void Success(string message, bool pause = true)
    {
        AnsiConsole.Write(new Panel($"[springgreen2]✔[/] {message}")
            .Border(BoxBorder.Rounded).BorderColor(Theme.Success));
        if (pause) Pause();
    }

    public static void Info(string message) =>
        AnsiConsole.MarkupLine($"[{Theme.AccentTag}]ℹ[/] {message}");

    public static void Warning(string message) =>
        AnsiConsole.MarkupLine($"[orange1]⚠ {message}[/]");

    public static void Error(Exception ex)
    {
        var (title, color) = ex switch
        {
            DataStorageException => ("Помилка файлу даних", Theme.Error),
            ShopException => ("Операцію відхилено", Theme.Warning),
            ArgumentException or FormatException => ("Некоректні дані", Theme.Warning),
            _ => ("Неочікувана помилка", Theme.Error)
        };

        var body = new Rows(
            new Markup($"[bold]{Markup.Escape(CleanMessage(ex))}[/]"),
            new Markup($"[{Theme.MutedTag}]{ex.GetType().Name}[/]"));

        AnsiConsole.Write(new Panel(body)
            .Header($" ✖ {title} ")
            .Border(BoxBorder.Rounded)
            .BorderColor(color));
    }

    /// <summary>Виконує дію, перехоплюючи помилки, щоб програма ніколи не падала через некоректні дані.</summary>
    public static void Run(Action action)
    {
        try
        {
            action();
        }
        catch (InputCancelledException)
        {
            AnsiConsole.MarkupLine($"[{Theme.MutedTag}]Дію скасовано.[/]");
            Pause();
        }
        catch (Exception ex) when (ex is ShopException or ArgumentException or FormatException or InvalidOperationException)
        {
            Error(ex);
            Pause();
        }
    }

    public static void Pause()
    {
        AnsiConsole.MarkupLine($"\n[{Theme.MutedTag}]Натисніть будь-яку клавішу, щоб продовжити...[/]");
        if (Console.IsInputRedirected)
            Console.ReadLine();
        else
            Console.ReadKey(intercept: true);
    }

    // Прибирає технічні хвости стандартних винятків: " (Parameter 'x')" та "Actual value was ..."
    public static string CleanMessage(Exception ex)
    {
        var message = ex.Message.Split('\n')[0].Trim();
        var index = message.IndexOf(" (Parameter '", StringComparison.Ordinal);
        return index > 0 ? message[..index] : message;
    }
}
