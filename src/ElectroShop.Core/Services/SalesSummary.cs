namespace ElectroShop.Core.Services;

public sealed record SalesSummary(
    decimal Revenue,
    int OrdersCount,
    int CancelledCount,
    decimal AverageCheck,
    int ItemsSold,
    decimal TotalDiscounts,
    decimal InventoryValue,
    int CustomersCount,
    int ProductsCount);
