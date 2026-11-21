using System.Text.Json.Serialization;

namespace ElectroShop.Core.Models;

/// <summary>Знімок товару на момент покупки — ціна в замовленні не змінюється разом із каталогом.</summary>
public class OrderItem
{
    private string _productName = string.Empty;
    private decimal _unitPrice;
    private int _quantity;

    public OrderItem() { }

    public OrderItem(Product product, int quantity)
    {
        ProductId = product.Id;
        CategoryId = product.CategoryId;
        ProductName = product.DisplayName;
        UnitPrice = product.Price;
        Quantity = quantity;
    }

    public Guid ProductId { get; init; }

    public Guid CategoryId { get; init; }

    public string ProductName
    {
        get => _productName;
        init => _productName = Guard.Text(value, "Назва товару", 1, 130);
    }

    public decimal UnitPrice
    {
        get => _unitPrice;
        init => _unitPrice = Guard.Range(value, "Ціна", 0.01m, Product.MaxPrice);
    }

    public int Quantity
    {
        get => _quantity;
        init => _quantity = Guard.Range(value, "Кількість", 1, CartItem.MaxQuantity);
    }

    [JsonIgnore]
    public decimal LineTotal => UnitPrice * Quantity;
}
