namespace ElectroShop.Core.Exceptions;

public class AccessDeniedException : ShopException
{
    public AccessDeniedException(string message) : base(message) { }
}
