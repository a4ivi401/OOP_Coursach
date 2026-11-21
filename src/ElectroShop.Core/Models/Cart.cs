using System.Text.Json.Serialization;

namespace ElectroShop.Core.Models;

public class Cart
{
    [JsonInclude, JsonPropertyName("Items")]
    private List<CartItem> ItemsData { get; set; } = new();

    [JsonIgnore]
    public IReadOnlyList<CartItem> Items => ItemsData;

    [JsonIgnore]
    public bool IsEmpty => ItemsData.Count == 0;

    [JsonIgnore]
    public int TotalQuantity => ItemsData.Sum(i => i.Quantity);

    public int QuantityOf(Guid productId) => Find(productId)?.Quantity ?? 0;

    public void Add(Guid productId, int quantity)
    {
        var existing = Find(productId);
        if (existing is null)
            ItemsData.Add(new CartItem(productId, quantity));
        else
            existing.Quantity += quantity;
    }

    public void SetQuantity(Guid productId, int quantity)
    {
        var item = Find(productId) ?? throw new ArgumentException("Товару немає в кошику.");
        item.Quantity = quantity;
    }

    public bool Remove(Guid productId) => ItemsData.RemoveAll(i => i.ProductId == productId) > 0;

    public void Clear() => ItemsData.Clear();

    private CartItem? Find(Guid productId) => ItemsData.Find(i => i.ProductId == productId);
}
