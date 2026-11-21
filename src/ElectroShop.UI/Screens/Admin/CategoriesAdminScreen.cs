using ElectroShop.Core;
using ElectroShop.Core.Models;
using ElectroShop.UI.Infrastructure;
using Spectre.Console;

namespace ElectroShop.UI.Screens.Admin;

internal sealed class CategoriesAdminScreen : MenuScreen
{
    private IReadOnlyList<Category> _categories = [];

    public CategoriesAdminScreen(Shop shop) : base(shop) { }

    protected override string Title => "Категорії";

    protected override void RenderBody()
    {
        _categories = Shop.Catalog.GetCategories();
        var table = new Table().Border(TableBorder.Rounded).BorderColor(Theme.Muted).Expand()
            .AddColumn("[bold]Назва[/]")
            .AddColumn("[bold]Опис[/]")
            .AddColumn(new TableColumn("[bold]Товарів[/]").RightAligned());

        foreach (var c in _categories)
            table.AddRow($"[bold]{Markup.Escape(c.Name)}[/]", $"[{Theme.MutedTag}]{Markup.Escape(c.Description)}[/]",
                Shop.Catalog.CountProductsIn(c.Id).ToString());

        AnsiConsole.Write(table);
    }

    protected override IEnumerable<MenuItem> GetMenu()
    {
        yield return new("Додати категорію", () =>
        {
            var probe = new Category();
            var name = Input.Ask("Назва", s => probe.Name = s);
            var description = Input.OptionalText("Опис");
            Shop.Catalog.AddCategory(name, description);
            Ui.Success($"Категорію «{Markup.Escape(name)}» створено.");
        });

        yield return new("Редагувати категорію", () =>
        {
            var category = Select("Яку категорію редагувати?");
            if (category is null) return;
            var probe = new Category();
            var name = Input.Ask("Назва", s => probe.Name = s, category.Name);
            var description = Input.OptionalText("Опис", category.Description);
            Shop.Catalog.UpdateCategory(category, name, description);
            Ui.Success("Категорію оновлено.");
        });

        yield return new("Видалити категорію", () =>
        {
            var category = Select("Яку категорію видалити?");
            if (category is null || !Input.Confirm($"Видалити «{Markup.Escape(category.Name)}»?", false)) return;
            Shop.Catalog.RemoveCategory(category.Id);
            Ui.Success("Категорію видалено.");
        });
    }

    private Category? Select(string title) =>
        Input.Select(title, _categories, c => Markup.Escape(c.Name));
}
