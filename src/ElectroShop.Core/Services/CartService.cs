using ElectroShop.Core.Models;
using ElectroShop.Core.Persistence;

namespace ElectroShop.Core.Services;

public sealed class CartService
{
    private readonly ShopContext _context;

    public CartService(ShopContext context) => _context = context;

    public IReadOnlyList<CartLine> GetLines(Customer customer)
    {
        var lines = new List<CartLine>();
        foreach (var item in customer.Cart.Items.ToList())
        {
            var product = _context.Products.FindById(item.ProductId);
            if (product is null)
                customer.Cart.Remove(item.ProductId);
            else
                lines.Add(new CartLine(product, item.Quantity));
        }
        return lines;
    }

    public decimal GetSubtotal(Customer customer) => GetLines(customer).Sum(l => l.LineTotal);

    public void Add(Customer customer, Guid productId, int quantity)
    {
        var product = _context.Products.GetById(productId);
        var newQuantity = customer.Cart.QuantityOf(productId) + quantity;

        if (newQuantity > CartItem.MaxQuantity)
            throw new ArgumentOutOfRangeException(nameof(quantity),
                $"В одному замовленні можна придбати не більше {CartItem.MaxQuantity} шт. одного товару.");

        product.EnsureInStock(newQuantity);
        customer.Cart.Add(productId, quantity);
    }

    public void SetQuantity(Customer customer, Guid productId, int quantity)
    {
        _context.Products.GetById(productId).EnsureInStock(quantity);
        customer.Cart.SetQuantity(productId, quantity);
    }

    public bool Remove(Customer customer, Guid productId) => customer.Cart.Remove(productId);

    public void Clear(Customer customer) => customer.Cart.Clear();
}
