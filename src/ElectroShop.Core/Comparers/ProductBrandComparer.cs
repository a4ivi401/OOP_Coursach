using ElectroShop.Core.Models;

namespace ElectroShop.Core.Comparers;

public sealed class ProductBrandComparer : IComparer<Product>
{
    public int Compare(Product? x, Product? y)
    {
        if (ReferenceEquals(x, y)) return 0;
        if (x is null) return -1;
        if (y is null) return 1;

        var result = string.Compare(x.Brand, y.Brand, StringComparison.CurrentCultureIgnoreCase);
        return result != 0 ? result : x.Price.CompareTo(y.Price);
    }
}
