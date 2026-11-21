using ElectroShop.Core.Comparers;
using ElectroShop.Core.Exceptions;
using ElectroShop.Core.Models;
using ElectroShop.Core.Persistence;

namespace ElectroShop.Core.Services;

public sealed class CatalogService
{
    private readonly ShopContext _context;

    public CatalogService(ShopContext context) => _context = context;

    public IReadOnlyList<Category> GetCategories() =>
        _context.Categories.OrderBy(c => c.Name, StringComparer.CurrentCulture).ToList();

    public Category GetCategory(Guid id) => _context.Categories.GetById(id);

    public string GetCategoryName(Guid id) => _context.Categories.FindById(id)?.Name ?? "—";

    public int CountProductsIn(Guid categoryId) => _context.Products.Find(p => p.CategoryId == categoryId).Count;

    public Category AddCategory(string name, string description)
    {
        EnsureCategoryNameIsFree(name, null);
        var category = new Category(name, description);
        _context.Categories.Add(category);
        return category;
    }

    public void UpdateCategory(Category category, string name, string description)
    {
        EnsureCategoryNameIsFree(name, category.Id);
        category.Name = name;
        category.Description = description;
    }

    public void RemoveCategory(Guid id)
    {
        var category = _context.Categories.GetById(id);
        var count = CountProductsIn(id);
        if (count > 0)
            throw new EntityInUseException($"Категорія «{category.Name}» містить {count} товар(ів). Спершу перенесіть або видаліть їх.");
        _context.Categories.Remove(id);
    }

    public IReadOnlyList<Product> GetAllProducts() => Search(new ProductFilter());

    public Product GetProduct(Guid id) => _context.Products.GetById(id);

    public IReadOnlyList<Product> Search(ProductFilter filter)
    {
        var result = _context.Products.Find(filter.Matches).ToList();
        result.Sort(ProductComparers.For(filter.SortBy));
        return result;
    }

    public IReadOnlyList<string> GetBrands() => _context.Products
        .Select(p => p.Brand)
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .Order(StringComparer.CurrentCultureIgnoreCase)
        .ToList();

    public (decimal Min, decimal Max) GetPriceBounds() => _context.Products.Count == 0
        ? (0, 0)
        : (_context.Products.Min(p => p.Price), _context.Products.Max(p => p.Price));

    public IReadOnlyList<Product> GetLowStock() =>
        _context.Products.Find(p => !p.IsAvailable || p.IsLowStock)
            .OrderBy(p => p.StockQuantity).ToList();

    public void AddProduct(Product product)
    {
        _context.Categories.GetById(product.CategoryId);
        EnsureSkuIsFree(product.Sku, null);
        _context.Products.Add(product);
    }

    public void ChangeSku(Product product, string sku)
    {
        EnsureSkuIsFree(sku, product.Id);
        product.Sku = sku;
    }

    public void ChangeCategory(Product product, Guid categoryId)
    {
        _context.Categories.GetById(categoryId);
        product.CategoryId = categoryId;
    }

    public void RemoveProduct(Guid id)
    {
        _context.Products.GetById(id);
        _context.Products.Remove(id);

        // Видалений товар зникає і з кошиків клієнтів
        foreach (var customer in _context.Customers)
            customer.Cart.Remove(id);
    }

    public void Restock(Guid productId, int quantity) => GetProduct(productId).Restock(quantity);

    private void EnsureCategoryNameIsFree(string name, Guid? exceptId)
    {
        var trimmed = name.Trim();
        if (_context.Categories.Exists(c => c.Id != exceptId && c.Name.Equals(trimmed, StringComparison.CurrentCultureIgnoreCase)))
            throw new DuplicateEntityException("Категорія", trimmed);
    }

    private void EnsureSkuIsFree(string sku, Guid? exceptId)
    {
        var normalized = sku.Trim().ToUpperInvariant();
        if (_context.Products.Exists(p => p.Id != exceptId && p.Sku == normalized))
            throw new DuplicateEntityException("Товар з артикулом", normalized);
    }
}
