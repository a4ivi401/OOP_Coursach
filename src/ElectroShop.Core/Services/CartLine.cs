using ElectroShop.Core.Models;

namespace ElectroShop.Core.Services;

public sealed record CartLine(Product Product, int Quantity)
{
    public decimal LineTotal => Product.Price * Quantity;

    public bool HasEnoughStock => Quantity <= Product.StockQuantity;
}
