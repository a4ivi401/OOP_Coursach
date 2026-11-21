using ElectroShop.Core;
using ElectroShop.Core.Comparers;
using ElectroShop.Core.Models;
using ElectroShop.Core.Services;
using ElectroShop.UI.Infrastructure;
using ElectroShop.UI.Views;
using Spectre.Console;

namespace ElectroShop.UI.Screens.Admin;

internal sealed class ProductsAdminScreen : MenuScreen
{
    private readonly ProductFilter _filter = new() { SortBy = ProductSortOption.Name };
    private bool _lowStockOnly;
    private IReadOnlyList<Product> _products = [];

    public ProductsAdminScreen(Shop shop) : base(shop) { }

    protected override string Title => "Товари";

    protected override void RenderBody()
    {
        _products = _lowStockOnly ? Shop.Catalog.GetLowStock() : Shop.Catalog.Search(_filter);
        AnsiConsole.Write(ProductViews.Table(_products, Shop.Catalog, adminMode: true));
        var summary = Shop.Reports.GetSummary();
        AnsiConsole.MarkupLine($"[{Theme.MutedTag}]Позицій: {_products.Count} · Вартість складу: {Theme.MoneyShort(summary.InventoryValue)}" +
                               (_lowStockOnly ? " · показано лише товари, що закінчуються" : "") + "[/]");
    }

    protected override IEnumerable<MenuItem> GetMenu()
    {
        yield return new("Додати товар", Add);
        yield return new("Переглянути картку товару", () =>
        {
            var product = Pick("Оберіть товар");
            if (product is null) return;
            Ui.Header();
            AnsiConsole.Write(ProductViews.Details(product, Shop.Catalog));
            Ui.Pause();
        });
        yield return new("Редагувати товар", Edit);
        yield return new("Поповнити склад", Restock);
        yield return new("Характеристики товару", EditSpecs);
        yield return new("Видалити товар", Remove);
        yield return new(_lowStockOnly ? "Показати всі товари" : "Показати лише ті, що закінчуються",
            () => _lowStockOnly = !_lowStockOnly);
        yield return new("Сортування", () => _filter.SortBy = Input.Choose("Як сортувати?",
            Enum.GetValues<ProductSortOption>(), o => o.ToDisplayName()));
    }

    private Product? Pick(string title) => Input.Select(title, _products, ProductViews.Label, searchable: true);

    private Category ChooseCategory(Guid? current = null)
    {
        var categories = Shop.Catalog.GetCategories();
        if (categories.Count == 0)
            throw new InvalidOperationException("Спершу створіть хоча б одну категорію.");
        var ordered = current is null ? categories : categories.OrderByDescending(c => c.Id == current).ToList();
        return Input.Choose("Категорія", ordered, c => Markup.Escape(c.Name));
    }

    private void Add()
    {
        Ui.Header("Заповніть поля. Порожнє значення в обов'язковому полі — скасувати додавання.");
        var draft = new Product();

        var category = ChooseCategory();
        draft.CategoryId = category.Id;
        AnsiConsole.MarkupLine($"[{Theme.MutedTag}]Категорія:[/] {Markup.Escape(category.Name)}");

        Input.Ask("Артикул [grey58](напр. PHN-1010)[/]", s =>
        {
            draft.Sku = s;
            if (Shop.Catalog.GetAllProducts().Any(p => p.Sku == draft.Sku))
                throw new ArgumentException($"Артикул {draft.Sku} вже використовується.");
            return s;
        });
        Input.Ask("Назва", s => draft.Name = s);
        Input.Ask("Бренд", s => draft.Brand = s);
        draft.Price = Input.Decimal("Ціна, ₴", 0.01m, Product.MaxPrice);
        draft.StockQuantity = Input.Int("Кількість на складі", 0, 100_000);
        draft.WarrantyMonths = Input.Int("Гарантія, міс.", 0, 60, 12, editable: false);
        draft.Description = Input.OptionalText("Опис");

        while (Input.Confirm("Додати характеристику?", false))
            AddSpec(draft);

        Shop.Catalog.AddProduct(draft);
        Ui.Success($"Товар «{Markup.Escape(draft.DisplayName)}» додано до каталогу.");
    }

    private void Edit()
    {
        var product = Pick("Який товар редагувати?");
        if (product is null) return;

        Ui.Header("Відредагуйте значення або натисніть Enter, щоб залишити без змін.");
        AnsiConsole.MarkupLine($"[bold]{Markup.Escape(product.DisplayName)}[/]\n");

        // Значення перевіряються на чернетці, а застосовуються лише після введення всіх полів
        var probe = new Product();
        var category = ChooseCategory(product.CategoryId);
        var sku = Input.Ask("Артикул", s => probe.Sku = s, product.Sku);
        var name = Input.Ask("Назва", s => probe.Name = s, product.Name);
        var brand = Input.Ask("Бренд", s => probe.Brand = s, product.Brand);
        var price = Input.Decimal("Ціна, ₴", 0.01m, Product.MaxPrice, product.Price);
        var warranty = Input.Int("Гарантія, міс.", 0, 60, product.WarrantyMonths);
        var description = Input.OptionalText("Опис", product.Description);

        Shop.Catalog.ChangeSku(product, sku);
        Shop.Catalog.ChangeCategory(product, category.Id);
        product.Name = name;
        product.Brand = brand;
        product.Price = price;
        product.WarrantyMonths = warranty;
        product.Description = description;
        Ui.Success("Зміни збережено.");
    }

    private void Restock()
    {
        var product = Pick("Який товар поповнити?");
        if (product is null) return;
        var quantity = Input.Int($"Скільки одиниць додати [{Theme.MutedTag}](зараз {product.StockQuantity})[/]", 1, 10_000);
        Shop.Catalog.Restock(product.Id, quantity);
        Ui.Success($"Залишок «{Markup.Escape(product.DisplayName)}»: {product.StockQuantity} шт.");
    }

    private void EditSpecs()
    {
        var product = Pick("Характеристики якого товару змінити?");
        if (product is null) return;

        while (true)
        {
            Ui.Header();
            AnsiConsole.Write(ProductViews.Details(product, Shop.Catalog));
            var action = Input.Choose("Дія", ["Додати / змінити", "Видалити", "Готово"], s => s);
            if (action == "Готово") return;

            if (action == "Видалити")
            {
                var key = Input.Select("Яку характеристику видалити?", product.Specifications.Keys.ToList(), Markup.Escape);
                if (key is not null) product.RemoveSpecification(key);
            }
            else
            {
                Ui.Run(() => AddSpec(product));
            }
        }
    }

    private static void AddSpec(Product product)
    {
        var key = Input.Text("Назва характеристики");
        Input.Ask("Значення", value =>
        {
            product.SetSpecification(key, value);
            return value;
        });
    }

    private void Remove()
    {
        var product = Pick("Який товар видалити?");
        if (product is null || !Input.Confirm($"Видалити «{Markup.Escape(product.DisplayName)}»? Його буде прибрано й з кошиків клієнтів.", false))
            return;
        Shop.Catalog.RemoveProduct(product.Id);
        Ui.Success("Товар видалено. Історія замовлень збереглася.");
    }
}
