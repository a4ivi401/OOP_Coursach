using ElectroShop.Core.Models;

namespace ElectroShop.Core.Comparers;

public sealed class ProductPriceComparer : IComparer<Product>
{
    private readonly bool _descending;

    public ProductPriceComparer(bool descending = false) => _descending = descending;

    public int Compare(Product? x, Product? y)
    {
        if (ReferenceEquals(x, y)) return 0;
        if (x is null) return -1;
        if (y is null) return 1;

        var result = x.Price.CompareTo(y.Price);
        if (result == 0) result = x.CompareTo(y);
        return _descending ? -result : result;
    }
}
