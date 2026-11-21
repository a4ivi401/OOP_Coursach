using ElectroShop.Core.Models;

namespace ElectroShop.Core.Services;

public sealed class LowStockEventArgs : EventArgs
{
    public LowStockEventArgs(Product product) => Product = product;

    public Product Product { get; }
}
