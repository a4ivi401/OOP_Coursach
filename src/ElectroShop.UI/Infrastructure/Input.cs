using System.Globalization;
using ElectroShop.Core.Exceptions;
using Spectre.Console;

namespace ElectroShop.UI.Infrastructure;

/// <summary>
/// Введення з обов'язковою перевіркою: значення повторно запитується, доки не стане коректним.
/// Сама перевірка делегується функції parse — зазвичай це setter моделі з бізнес-бібліотеки.
/// </summary>
internal static class Input
{
    /// <param name="current">Поточне значення — його можна відредагувати; Enter залишає без змін.</param>
    /// <param name="editable">false — значення лише показується в дужках як підказка за замовчуванням.</param>
    public static T Ask<T>(string label, Func<string, T> parse, string? current = null, bool editable = true) =>
        TryAsk(label, parse, current, out var result, editable) ? result : throw new InputCancelledException();

    /// <returns>false, якщо користувач залишив поле порожнім.</returns>
    public static bool TryAsk<T>(string label, Func<string, T> parse, string? current, out T result, bool editable = true)
    {
        T parsed = default!;
        // Spectre сам додає двокрапку після показаного значення за замовчуванням
        var colon = current is not null && !editable ? "" : ":";
        var prompt = new TextPrompt<string>($"[{Theme.AccentTag}]›[/] {label}{colon}")
            .PromptStyle("white")
            .AllowEmpty()
            .Validate(text =>
            {
                if (string.IsNullOrWhiteSpace(text))
                    return ValidationResult.Success();
                try
                {
                    parsed = parse(text.Trim());
                    return ValidationResult.Success();
                }
                catch (Exception ex) when (IsValidationError(ex))
                {
                    return ValidationResult.Error($"  [red1]✖ {Markup.Escape(Ui.CleanMessage(ex))}[/]");
                }
            });

        if (current is not null)
        {
            prompt.DefaultValue(current);
            if (editable) prompt.EditableDefaultValue(true).HideDefaultValue();
            else prompt.ShowDefaultValue().DefaultValueStyle(new Style(Theme.Muted));
        }

        var text = AnsiConsole.Prompt(prompt);
        // Якщо прийнято значення за замовчуванням, валідатор міг його не побачити
        result = text == current && current is not null ? parse(current) : parsed;
        return !string.IsNullOrWhiteSpace(text);
    }

    public static decimal? OptionalDecimal(string label) =>
        TryAsk($"{label} [{Theme.MutedTag}](необов'язково)[/]", ParseDecimal, null, out var value) ? value : null;

    public static string Text(string label, string? current = null) => Ask(label, s => s, current);

    public static string OptionalText(string label, string? current = null)
    {
        var prompt = new TextPrompt<string>($"[{Theme.AccentTag}]›[/] {label} [{Theme.MutedTag}](необов'язково)[/]:")
            .AllowEmpty();
        if (!string.IsNullOrEmpty(current))
            prompt.DefaultValue(current).EditableDefaultValue(true).HideDefaultValue();
        return AnsiConsole.Prompt(prompt).Trim();
    }

    public static decimal Decimal(string label, decimal min, decimal max, decimal? current = null, bool editable = true) =>
        Ask(label, s =>
        {
            var value = ParseDecimal(s);
            if (value < min || value > max)
                throw new ArgumentException($"Введіть число від {min:N2} до {max:N2}.");
            return value;
        }, current?.ToString("0.##", CultureInfo.InvariantCulture), editable);

    public static int Int(string label, int min, int max, int? current = null, bool editable = true) =>
        Ask(label, s =>
        {
            if (!int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value))
                throw new FormatException("Введіть ціле число.");
            if (value < min || value > max)
                throw new ArgumentException($"Введіть число від {min} до {max}.");
            return value;
        }, current?.ToString(CultureInfo.InvariantCulture), editable);

    public static DateTime Date(string label, DateTime? current = null, bool editable = true) =>
        Ask($"{label} [{Theme.MutedTag}](дд.мм.рррр)[/]", s =>
        {
            if (!DateTime.TryParseExact(s, ["dd.MM.yyyy", "d.M.yyyy"], CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out var date))
                throw new FormatException("Дата має бути у форматі дд.мм.рррр, наприклад 31.12.2026.");
            return date;
        }, current?.ToString("dd.MM.yyyy"), editable);

    public static DateTime? OptionalDate(string question, string label) =>
        Confirm(question, false) ? Date(label) : null;

    // Приймає і кирилицю, і латиницю, щоб не залежати від розкладки клавіатури
    public static bool Confirm(string question, bool defaultValue = true)
    {
        var hint = defaultValue ? "Т/н" : "т/Н";
        AnsiConsole.Markup($"[{Theme.AccentTag}]?[/] {question} [{Theme.MutedTag}][[{hint}]][/] ");

        while (true)
        {
            var key = Console.IsInputRedirected
                ? (Console.ReadLine() ?? string.Empty).Trim().ToLowerInvariant().FirstOrDefault()
                : char.ToLowerInvariant(Console.ReadKey(intercept: true).KeyChar);

            bool? answer = key switch
            {
                'т' or 'y' or 'д' => true,
                'н' or 'n' => false,
                '\r' or '\n' or '\0' => defaultValue,
                _ => null
            };

            if (answer is { } result)
            {
                AnsiConsole.MarkupLine(result ? "[springgreen2]так[/]" : "[red1]ні[/]");
                return result;
            }
        }
    }

    public static string Secret(string label) =>
        AnsiConsole.Prompt(new TextPrompt<string>($"[{Theme.AccentTag}]›[/] {label}:").Secret('●').AllowEmpty());

    /// <summary>Вибір зі списку стрілками. Повертає null, якщо обрано «Назад» або натиснуто Esc.</summary>
    public static T? Select<T>(string title, IEnumerable<T> items, Func<T, string> display, bool searchable = false)
        where T : class
    {
        var options = items.Select(i => new Option<T>(i, display(i))).ToList();
        if (options.Count == 0)
        {
            Ui.Info("Список порожній.");
            return null;
        }

        var back = new Option<T>(null, $"[{Theme.MutedTag}]← Назад[/]");
        options.Add(back);

        var prompt = new SelectionPrompt<Option<T>>()
            .Title($"[bold]{title}[/]")
            .PageSize(14)
            .HighlightStyle(Theme.Highlight)
            .MoreChoicesText($"[{Theme.MutedTag}](гортайте ↑↓ для інших варіантів)[/]")
            .UseConverter(o => o.Label)
            .AddChoices(options)
            .AddCancelResult(back);

        if (searchable)
            prompt.EnableSearch().SearchPlaceholderText($"[{Theme.MutedTag}]почніть вводити для пошуку...[/]");

        return AnsiConsole.Prompt(prompt).Value;
    }

    public static T Choose<T>(string title, IEnumerable<T> items, Func<T, string> display) where T : notnull =>
        AnsiConsole.Prompt(new SelectionPrompt<T>()
            .Title($"[bold]{title}[/]")
            .HighlightStyle(Theme.Highlight)
            .UseConverter(display)
            .AddChoices(items));

    public static decimal ParseDecimal(string text)
    {
        var normalized = text.Replace(" ", "").Replace(" ", "").Replace("₴", "").Replace(',', '.');
        if (!decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.InvariantCulture, out var value))
            throw new FormatException("Введіть число, наприклад 1499.99.");
        return value;
    }

    private static bool IsValidationError(Exception ex) =>
        ex is ArgumentException or FormatException or OverflowException or ShopException;

    private sealed record Option<T>(T? Value, string Label);
}
