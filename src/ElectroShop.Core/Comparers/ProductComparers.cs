using ElectroShop.Core.Models;

namespace ElectroShop.Core.Comparers;

public static class ProductComparers
{
    public static IComparer<Product> For(ProductSortOption option) => option switch
    {
        ProductSortOption.PriceAscending => new ProductPriceComparer(),
        ProductSortOption.PriceDescending => new ProductPriceComparer(descending: true),
        ProductSortOption.Brand => new ProductBrandComparer(),
        ProductSortOption.Stock => new ProductStockComparer(),
        _ => Comparer<Product>.Default // використовує Product.CompareTo
    };

    public static string ToDisplayName(this ProductSortOption option) => option switch
    {
        ProductSortOption.Name => "За назвою (А–Я)",
        ProductSortOption.PriceAscending => "Спочатку дешевші",
        ProductSortOption.PriceDescending => "Спочатку дорожчі",
        ProductSortOption.Brand => "За брендом",
        ProductSortOption.Stock => "За наявністю",
        _ => option.ToString()
    };
}
