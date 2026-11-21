using ElectroShop.Core;
using ElectroShop.Core.Comparers;
using ElectroShop.Core.Models;
using ElectroShop.Core.Services;
using ElectroShop.UI.Infrastructure;
using ElectroShop.UI.Views;
using Spectre.Console;

namespace ElectroShop.UI.Screens.Customer;

internal sealed class CatalogScreen : MenuScreen
{
    private readonly Core.Models.Customer _customer;
    private readonly ProductFilter _filter = new();
    private IReadOnlyList<Product> _results = [];

    public CatalogScreen(Shop shop, Core.Models.Customer customer) : base(shop) => _customer = customer;

    protected override string Title => "Каталог";

    protected override void RenderBody()
    {
        _results = Shop.Catalog.Search(_filter);
        AnsiConsole.Write(ProductViews.Table(_results, Shop.Catalog));
        AnsiConsole.MarkupLine($"[{Theme.MutedTag}]Знайдено: {_results.Count} · Сортування: {_filter.SortBy.ToDisplayName()}[/]");
        if (!_filter.IsEmpty)
            AnsiConsole.MarkupLine($"[{Theme.AccentTag}]Фільтри:[/] {DescribeFilter()}");
    }

    protected override IEnumerable<MenuItem> GetMenu()
    {
        yield return new("Переглянути товар / додати в кошик", OpenProduct);
        yield return new("Пошук за назвою чи артикулом", () =>
            _filter.SearchText = Input.OptionalText("Що шукаємо", _filter.SearchText));
        yield return new("Фільтр: категорія", ChooseCategory);
        yield return new("Фільтр: бренди", ChooseBrands);
        yield return new("Фільтр: діапазон цін", ChoosePriceRange);
        yield return new($"Фільтр: лише в наявності [{Theme.MutedTag}]({(_filter.InStockOnly ? "увімк." : "вимк.")})[/]",
            () => _filter.InStockOnly = !_filter.InStockOnly);
        yield return new("Сортування", () => _filter.SortBy = Input.Choose("Як сортувати?",
            Enum.GetValues<ProductSortOption>(), o => o.ToDisplayName()));
        if (!_filter.IsEmpty)
            yield return new("Скинути фільтри", _filter.Reset);
    }

    private void OpenProduct()
    {
        var product = Input.Select("Оберіть товар", _results, ProductViews.Label, searchable: true);
        if (product is null) return;

        Ui.Header();
        AnsiConsole.Write(ProductViews.Details(product, Shop.Catalog));

        var inCart = _customer.Cart.QuantityOf(product.Id);
        if (inCart > 0)
            Ui.Info($"У вашому кошику вже {inCart} шт.");

        if (!product.IsAvailable)
        {
            Ui.Warning("Товар тимчасово відсутній.");
            Ui.Pause();
            return;
        }

        if (!Input.Confirm("Додати в кошик?"))
            return;

        var quantity = Input.Int("Кількість", 1, CartItem.MaxQuantity);
        Shop.Carts.Add(_customer, product.Id, quantity);
        Ui.Success($"«{Markup.Escape(product.DisplayName)}» × {quantity} додано в кошик.");
    }

    private void ChooseCategory()
    {
        var all = new Category("Усі категорії");
        var choice = Input.Choose("Категорія", Shop.Catalog.GetCategories().Prepend(all),
            c => c == all ? $"[{Theme.MutedTag}]Усі категорії[/]" : Markup.Escape(c.Name));
        _filter.CategoryId = choice == all ? null : choice.Id;
    }

    private void ChooseBrands()
    {
        var brands = Shop.Catalog.GetBrands();
        var prompt = new MultiSelectionPrompt<string>()
            .Title("[bold]Оберіть бренди[/] [grey58](нічого не обрано — усі бренди)[/]")
            .InstructionsText("[grey58](Пробіл — позначити, Enter — підтвердити)[/]")
            .NotRequired()
            .PageSize(15)
            .HighlightStyle(Theme.Highlight)
            .AddChoices(brands);
        foreach (var brand in _filter.Brands)
            prompt.Select(brand);
        _filter.SetBrands(AnsiConsole.Prompt(prompt));
    }

    private void ChoosePriceRange()
    {
        var (min, max) = Shop.Catalog.GetPriceBounds();
        Ui.Info($"Ціни в каталозі: від {Theme.MoneyShort(min)} до {Theme.MoneyShort(max)}. Порожнє поле — без обмеження.");
        var from = Input.OptionalDecimal("Ціна від");
        var to = Input.OptionalDecimal("Ціна до");
        _filter.SetPriceRange(from, to);
    }

    private string DescribeFilter()
    {
        var parts = new List<string>();
        if (!string.IsNullOrWhiteSpace(_filter.SearchText)) parts.Add($"«{Markup.Escape(_filter.SearchText)}»");
        if (_filter.CategoryId is { } id) parts.Add(Markup.Escape(Shop.Catalog.GetCategoryName(id)));
        if (_filter.Brands.Count > 0) parts.Add(Markup.Escape(string.Join(", ", _filter.Brands)));
        if (_filter.MinPrice is not null || _filter.MaxPrice is not null)
            parts.Add($"{(_filter.MinPrice is { } a ? Theme.MoneyShort(a) : "0")} – {(_filter.MaxPrice is { } b ? Theme.MoneyShort(b) : "∞")}");
        if (_filter.InStockOnly) parts.Add("в наявності");
        return string.Join($" [{Theme.MutedTag}]·[/] ", parts);
    }
}
