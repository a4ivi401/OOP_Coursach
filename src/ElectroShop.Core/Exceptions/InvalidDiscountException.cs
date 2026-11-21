namespace ElectroShop.Core.Exceptions;

public class InvalidDiscountException : ShopException
{
    public string? CouponCode { get; }

    public InvalidDiscountException(string message, string? couponCode = null)
        : base(couponCode is null ? char.ToUpper(message[0]) + message[1..] : $"Купон {couponCode}: {message}")
    {
        CouponCode = couponCode;
    }
}
