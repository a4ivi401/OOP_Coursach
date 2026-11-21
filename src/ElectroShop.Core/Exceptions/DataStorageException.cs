namespace ElectroShop.Core.Exceptions;

public class DataStorageException : ShopException
{
    public string Path { get; }

    public DataStorageException(string message, string path, Exception innerException)
        : base(message, innerException)
    {
        Path = path;
    }
}
