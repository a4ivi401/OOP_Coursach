namespace ElectroShop.Core.Exceptions;

public class EmptyCartException : ShopException
{
    public EmptyCartException() : base("Кошик порожній — додайте товари перед оформленням замовлення.") { }
}
