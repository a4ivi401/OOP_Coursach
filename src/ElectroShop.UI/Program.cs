using System.Globalization;
using System.Text;
using ElectroShop.Core;
using ElectroShop.Core.Persistence;
using ElectroShop.UI;

Console.OutputEncoding = Encoding.UTF8;
Console.InputEncoding = Encoding.UTF8;
CultureInfo.DefaultThreadCurrentCulture = CultureInfo.CurrentCulture = new CultureInfo("uk-UA");

// Шлях до файлу даних можна передати першим аргументом
var dataPath = args.Length > 0 ? args[0] : Path.Combine("data", "shop.json");

var shop = new Shop(new JsonDataStorage<ShopSnapshot>(dataPath));
new App(shop).Run();
