namespace ElectroShop.Core.Models;

public class CartItem
{
    public static int MaxQuantity => 20;

    private Guid _productId;
    private int _quantity;

    public CartItem() { }

    public CartItem(Guid productId, int quantity)
    {
        ProductId = productId;
        Quantity = quantity;
    }

    public Guid ProductId
    {
        get => _productId;
        set => _productId = Guard.NotEmpty(value, "Товар");
    }

    public int Quantity
    {
        get => _quantity;
        set => _quantity = Guard.Range(value, "Кількість у кошику", 1, MaxQuantity);
    }
}
