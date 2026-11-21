namespace ElectroShop.Core.Abstractions;

public interface IDataStorage<T> where T : class
{
    string Location { get; }
    bool Exists { get; }

    void Save(T data);
    T Load();

    /// <returns>Шлях до створеної резервної копії.</returns>
    string Backup();
}
