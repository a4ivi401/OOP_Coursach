using ElectroShop.Core.Services;

namespace ElectroShop.UI.Infrastructure;

/// <summary>Збирає сповіщення від бізнес-логіки (через події) для показу в адмін-панелі.</summary>
internal sealed class NotificationCenter
{
    private readonly List<(DateTime At, string Message)> _items = new();

    public IReadOnlyList<(DateTime At, string Message)> Items => _items;

    public void OnLowStock(object? sender, LowStockEventArgs e)
    {
        var product = e.Product;
        _items.Add((DateTime.Now, product.IsAvailable
            ? $"Закінчується «{product.DisplayName}»: залишилось {product.StockQuantity} шт."
            : $"«{product.DisplayName}» розпродано повністю."));
    }

    public void Clear() => _items.Clear();
}
