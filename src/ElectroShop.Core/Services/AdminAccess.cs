using System.Security.Cryptography;
using System.Text;
using ElectroShop.Core.Exceptions;

namespace ElectroShop.Core.Services;

public sealed class AdminAccess
{
    public static int MaxAttempts => 3;

    // SHA-256 від PIN-коду "1234": сам PIN у коді не зберігається
    private static readonly byte[] PinHash =
        Convert.FromHexString("03AC674216F3E15C761EE1A5E255F067953623C8B388B4459E13F978D7C846F4");

    private int _failedAttempts;

    public int AttemptsLeft => MaxAttempts - _failedAttempts;

    public bool IsLocked => _failedAttempts >= MaxAttempts;

    public void Authenticate(string pin)
    {
        if (IsLocked)
            throw new AccessDeniedException("Адмін-панель заблоковано до перезапуску програми.");

        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(pin.Trim()));
        if (CryptographicOperations.FixedTimeEquals(hash, PinHash))
        {
            _failedAttempts = 0;
            return;
        }

        _failedAttempts++;
        throw new AccessDeniedException(IsLocked
            ? "Невірний PIN. Спроби вичерпано — адмін-панель заблоковано."
            : $"Невірний PIN. Залишилось спроб: {AttemptsLeft}.");
    }
}
