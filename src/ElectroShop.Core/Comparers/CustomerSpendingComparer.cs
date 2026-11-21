using ElectroShop.Core.Models;

namespace ElectroShop.Core.Comparers;

public sealed class CustomerSpendingComparer : IComparer<Customer>
{
    public int Compare(Customer? x, Customer? y)
    {
        if (ReferenceEquals(x, y)) return 0;
        if (x is null) return 1;
        if (y is null) return -1;

        var result = y.TotalSpent.CompareTo(x.TotalSpent);
        return result != 0 ? result : string.Compare(x.FullName, y.FullName, StringComparison.CurrentCulture);
    }
}
