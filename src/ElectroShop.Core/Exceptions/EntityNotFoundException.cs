namespace ElectroShop.Core.Exceptions;

public class EntityNotFoundException : ShopException
{
    public string EntityName { get; }
    public string Key { get; }

    public EntityNotFoundException(string entityName, object key)
        : base($"{entityName} «{key}» не знайдено.")
    {
        EntityName = entityName;
        Key = key.ToString() ?? string.Empty;
    }
}
