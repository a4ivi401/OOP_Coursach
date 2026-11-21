using ElectroShop.Core.Models;

namespace ElectroShop.Core.Comparers;

/// <summary>Спочатку товари з найбільшим залишком.</summary>
public sealed class ProductStockComparer : IComparer<Product>
{
    public int Compare(Product? x, Product? y)
    {
        if (ReferenceEquals(x, y)) return 0;
        if (x is null) return 1;
        if (y is null) return -1;

        var result = y.StockQuantity.CompareTo(x.StockQuantity);
        return result != 0 ? result : x.CompareTo(y);
    }
}
