namespace ElectroShop.Core.Exceptions;

public class DuplicateEntityException : ShopException
{
    public string EntityName { get; }
    public string Key { get; }

    public DuplicateEntityException(string entityName, object key)
        : base($"{entityName} «{key}» вже існує.")
    {
        EntityName = entityName;
        Key = key.ToString() ?? string.Empty;
    }
}
