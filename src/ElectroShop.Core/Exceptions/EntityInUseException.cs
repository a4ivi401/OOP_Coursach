namespace ElectroShop.Core.Exceptions;

public class EntityInUseException : ShopException
{
    public EntityInUseException(string message) : base(message) { }
}
