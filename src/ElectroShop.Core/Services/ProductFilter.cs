using ElectroShop.Core.Comparers;
using ElectroShop.Core.Models;

namespace ElectroShop.Core.Services;

public sealed class ProductFilter
{
    private decimal? _minPrice;
    private decimal? _maxPrice;
    private readonly HashSet<string> _brands = new(StringComparer.OrdinalIgnoreCase);

    public string? SearchText { get; set; }

    public Guid? CategoryId { get; set; }

    public bool InStockOnly { get; set; }

    public ProductSortOption SortBy { get; set; } = ProductSortOption.Name;

    public decimal? MinPrice => _minPrice;

    public decimal? MaxPrice => _maxPrice;

    public IReadOnlyCollection<string> Brands => _brands;

    public bool IsEmpty => string.IsNullOrWhiteSpace(SearchText) && CategoryId is null && !InStockOnly
                           && _minPrice is null && _maxPrice is null && _brands.Count == 0;

    public void SetPriceRange(decimal? min, decimal? max)
    {
        if (min < 0 || max < 0)
            throw new ArgumentException("Ціна не може бути від'ємною.");
        if (min is not null && max is not null && min > max)
            throw new ArgumentException("Мінімальна ціна не може перевищувати максимальну.");
        _minPrice = min;
        _maxPrice = max;
    }

    public void SetBrands(IEnumerable<string> brands)
    {
        _brands.Clear();
        _brands.UnionWith(brands);
    }

    public bool Matches(Product product)
    {
        if (CategoryId is { } categoryId && product.CategoryId != categoryId) return false;
        if (InStockOnly && !product.IsAvailable) return false;
        if (_minPrice is { } min && product.Price < min) return false;
        if (_maxPrice is { } max && product.Price > max) return false;
        if (_brands.Count > 0 && !_brands.Contains(product.Brand)) return false;

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var text = SearchText.Trim();
            return product.DisplayName.Contains(text, StringComparison.CurrentCultureIgnoreCase)
                   || product.Sku.Contains(text, StringComparison.OrdinalIgnoreCase)
                   || product.Description.Contains(text, StringComparison.CurrentCultureIgnoreCase);
        }

        return true;
    }

    public void Reset()
    {
        SearchText = null;
        CategoryId = null;
        InStockOnly = false;
        SortBy = ProductSortOption.Name;
        _minPrice = _maxPrice = null;
        _brands.Clear();
    }
}
