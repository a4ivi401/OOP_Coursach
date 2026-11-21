using ElectroShop.Core.Models;

namespace ElectroShop.Core.Pricing;

public sealed class PricingContext
{
    public PricingContext(IReadOnlyList<OrderItem> items)
    {
        Items = items;
        Subtotal = items.Sum(i => i.LineTotal);
    }

    public IReadOnlyList<OrderItem> Items { get; }

    public decimal Subtotal { get; }

    public decimal SubtotalForCategory(Guid categoryId) =>
        Items.Where(i => i.CategoryId == categoryId).Sum(i => i.LineTotal);
}
