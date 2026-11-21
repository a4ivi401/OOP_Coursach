using ElectroShop.Core.Models;
using ElectroShop.Core.Persistence;

namespace ElectroShop.Core.Services;

public sealed class ReportService
{
    private readonly ShopContext _context;

    public ReportService(ShopContext context) => _context = context;

    private IEnumerable<Order> ActiveOrders => _context.Orders.Where(o => o.IsActive);

    public SalesSummary GetSummary()
    {
        var active = ActiveOrders.ToList();
        var revenue = active.Sum(o => o.Total);

        return new SalesSummary(
            Revenue: revenue,
            OrdersCount: active.Count,
            CancelledCount: _context.Orders.Count - active.Count,
            AverageCheck: active.Count == 0 ? 0 : decimal.Round(revenue / active.Count, 2),
            ItemsSold: active.SelectMany(o => o.Items).Sum(i => i.Quantity),
            TotalDiscounts: active.Sum(o => o.Pricing.TotalDiscount),
            InventoryValue: _context.Products.Sum(p => p.Price * p.StockQuantity),
            CustomersCount: _context.Customers.Count,
            ProductsCount: _context.Products.Count);
    }

    public IReadOnlyList<ProductSales> GetTopProducts(int count) => ActiveOrders
        .SelectMany(o => o.Items)
        .GroupBy(i => i.ProductId)
        .Select(g => new ProductSales(
            _context.Products.FindById(g.Key)?.DisplayName ?? g.First().ProductName,
            g.Sum(i => i.Quantity),
            g.Sum(i => i.LineTotal)))
        .OrderByDescending(s => s.Quantity)
        .ThenByDescending(s => s.Revenue)
        .Take(count)
        .ToList();

    public IReadOnlyList<CategoryRevenue> GetRevenueByCategory() => ActiveOrders
        .SelectMany(o => o.Items)
        .GroupBy(i => i.CategoryId)
        .Select(g => new CategoryRevenue(
            _context.Categories.FindById(g.Key)?.Name ?? "Видалена категорія",
            g.Sum(i => i.LineTotal)))
        .OrderByDescending(c => c.Revenue)
        .ToList();

    public IReadOnlyDictionary<OrderStatus, int> GetOrdersByStatus() => Enum.GetValues<OrderStatus>()
        .ToDictionary(s => s, s => _context.Orders.Find(o => o.Status == s).Count);
}
