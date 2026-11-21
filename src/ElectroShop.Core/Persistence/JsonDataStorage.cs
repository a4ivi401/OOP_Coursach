using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using ElectroShop.Core.Abstractions;
using ElectroShop.Core.Exceptions;

namespace ElectroShop.Core.Persistence;

public sealed class JsonDataStorage<T> : IDataStorage<T> where T : class
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        // кирилиця у файлі залишається читабельною, а не А...
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly string _path;

    public JsonDataStorage(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("Шлях до файлу даних не вказано.", nameof(path));
        _path = Path.GetFullPath(path);
    }

    public string Location => _path;

    public bool Exists => File.Exists(_path);

    public void Save(T data)
    {
        ArgumentNullException.ThrowIfNull(data);
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_path)!);

            // Пишемо в тимчасовий файл і лише потім підміняємо основний,
            // щоб збій посеред запису не зіпсував дані
            var tempPath = _path + ".tmp";
            File.WriteAllText(tempPath, JsonSerializer.Serialize(data, Options));
            File.Move(tempPath, _path, overwrite: true);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or NotSupportedException)
        {
            throw new DataStorageException($"Не вдалося зберегти дані: {ex.Message}", _path, ex);
        }
    }

    public T Load()
    {
        try
        {
            var json = File.ReadAllText(_path);
            return JsonSerializer.Deserialize<T>(json, Options)
                   ?? throw new JsonException("Файл не містить даних.");
        }
        catch (FileNotFoundException ex)
        {
            throw new DataStorageException("Файл даних не знайдено.", _path, ex);
        }
        catch (JsonException ex)
        {
            throw new DataStorageException($"Файл даних пошкоджено: {ex.Message}", _path, ex);
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidDiscountException)
        {
            // Спрацювала валідація у setter-і моделі — дані у файлі некоректні
            throw new DataStorageException($"Файл містить некоректні дані: {ex.Message}", _path, ex);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            throw new DataStorageException($"Не вдалося прочитати файл: {ex.Message}", _path, ex);
        }
    }

    public string Backup()
    {
        var backupPath = $"{_path}.{DateTime.Now:yyyyMMdd-HHmmss}.bak";
        File.Copy(_path, backupPath, overwrite: true);
        return backupPath;
    }
}
