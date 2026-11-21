namespace ElectroShop.Core.Exceptions;

public class OutOfStockException : ShopException
{
    public string ProductName { get; }
    public int Requested { get; }
    public int Available { get; }

    public OutOfStockException(string productName, int requested, int available)
        : base(available == 0
            ? $"Товару «{productName}» немає на складі."
            : $"Недостатньо товару «{productName}» на складі: запитано {requested}, доступно {available}.")
    {
        ProductName = productName;
        Requested = requested;
        Available = available;
    }
}
