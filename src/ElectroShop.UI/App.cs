using ElectroShop.Core;
using ElectroShop.Core.Exceptions;
using ElectroShop.UI.Infrastructure;
using ElectroShop.UI.Screens;
using Spectre.Console;

namespace ElectroShop.UI;

internal sealed class App
{
    private readonly Shop _shop;
    private readonly NotificationCenter _notifications = new();
    private bool _saved;

    public App(Shop shop)
    {
        _shop = shop;
        _shop.Orders.LowStock += _notifications.OnLowStock;
    }

    public void Run()
    {
        Console.CancelKeyPress += OnCancelKeyPress;

        if (!LoadData())
            return;

        new MainScreen(_shop, _notifications).Show();
        SaveWithRetry();
        AnsiConsole.MarkupLine($"\n[{Theme.AccentTag}]Дякуємо, що завітали до ElectroShop! До зустрічі.[/]");
    }

    /// <returns>false, якщо користувач вирішив вийти, не чіпаючи пошкоджений файл.</returns>
    private bool LoadData()
    {
        Ui.Header();
        if (!_shop.HasSavedData)
        {
            _shop.SeedDemoData();
            Ui.Info($"Файл даних не знайдено — магазин заповнено демонстраційними даними.\n  [{Theme.MutedTag}]Їх буде збережено у {Markup.Escape(_shop.StorageLocation)}[/]");
            Ui.Pause();
            return true;
        }

        try
        {
            AnsiConsole.Status().Spinner(Spinner.Known.Dots).Start("Завантаження даних...", _ => _shop.Load());
            return true;
        }
        catch (DataStorageException ex)
        {
            Ui.Error(ex);
            AnsiConsole.MarkupLine($"[{Theme.MutedTag}]{Markup.Escape(ex.Path)}[/]\n");

            const string seed = "Створити резервну копію та заповнити демо-даними";
            const string empty = "Створити резервну копію та почати з порожнього магазину";
            const string quit = "Вийти, нічого не змінюючи";
            var choice = Input.Choose("Що робимо з пошкодженим файлом?", [seed, empty, quit], s => s);
            if (choice == quit)
            {
                _saved = true;
                return false;
            }

            var backup = _shop.BackupStorage();
            if (choice == seed) _shop.SeedDemoData();
            else _shop.Reset();
            Ui.Success($"Резервну копію збережено: [underline]{Markup.Escape(backup)}[/]");
            return true;
        }
    }

    private void SaveWithRetry()
    {
        while (true)
        {
            try
            {
                AnsiConsole.Status().Spinner(Spinner.Known.Dots).Start("Збереження даних...", _ => _shop.Save());
                _saved = true;
                AnsiConsole.MarkupLine($"[springgreen2]✔[/] Дані збережено: [{Theme.MutedTag}]{Markup.Escape(_shop.StorageLocation)}[/]");
                return;
            }
            catch (DataStorageException ex)
            {
                Ui.Error(ex);
                if (!Input.Confirm("Спробувати ще раз?"))
                    return;
            }
        }
    }

    // Ctrl+C не повинен призводити до втрати накопичених даних
    private void OnCancelKeyPress(object? sender, ConsoleCancelEventArgs e)
    {
        if (_saved) return;
        try
        {
            _shop.Save();
            Console.WriteLine("\nПрограму перервано — дані збережено.");
        }
        catch (DataStorageException ex)
        {
            Console.WriteLine($"\nНе вдалося зберегти дані: {ex.Message}");
        }
    }
}
