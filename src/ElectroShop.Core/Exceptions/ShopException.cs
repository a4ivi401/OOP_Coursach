namespace ElectroShop.Core.Exceptions;

/// <summary>Базовий тип для всіх помилок бізнес-логіки магазину.</summary>
public class ShopException : Exception
{
    public ShopException(string message) : base(message) { }

    public ShopException(string message, Exception innerException) : base(message, innerException) { }
}
