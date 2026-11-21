using ElectroShop.Core;
using Spectre.Console;

namespace ElectroShop.UI.Infrastructure;

/// <summary>
/// Базовий екран-меню (шаблонний метод): заголовок → вміст → вибір пункту → виконання.
/// Нащадки визначають лише вміст і пункти меню.
/// </summary>
internal abstract class MenuScreen
{
    private bool _closeRequested;

    protected MenuScreen(Shop shop) => Shop = shop;

    protected Shop Shop { get; }

    protected abstract string Title { get; }

    protected virtual string BackLabel => "← Назад";

    protected virtual void RenderBody() { }

    protected abstract IEnumerable<MenuItem> GetMenu();

    protected void Close() => _closeRequested = true;

    public void Show()
    {
        Ui.PushScreen(Title);
        try
        {
            while (!_closeRequested)
            {
                Ui.Header();
                RenderBody();
                AnsiConsole.WriteLine();

                var items = GetMenu().ToList();
                var back = MenuItem.Back($"[{Theme.MutedTag}]{BackLabel}[/]");
                items.Add(back);

                var choice = AnsiConsole.Prompt(new SelectionPrompt<MenuItem>()
                    .Title($"[{Theme.MutedTag}]Оберіть дію (↑↓, Enter):[/]")
                    .PageSize(15)
                    .HighlightStyle(Theme.Highlight)
                    .UseConverter(i => i.Label)
                    .AddChoices(items)
                    .AddCancelResult(back));

                if (choice.ClosesScreen)
                    break;

                Ui.Run(choice.Action);
            }
        }
        finally
        {
            Ui.PopScreen();
        }
    }
}
