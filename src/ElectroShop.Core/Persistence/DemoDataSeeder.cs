using ElectroShop.Core.Models;
using ElectroShop.Core.Models.Coupons;
using ElectroShop.Core.Pricing;
using ElectroShop.Core.Services;

namespace ElectroShop.Core.Persistence;

internal static class DemoDataSeeder
{
    public static void Seed(ShopContext context, TimeProvider time)
    {
        var today = time.GetLocalNow().Date;
        var catalog = new CatalogService(context);

        var phones = catalog.AddCategory("Смартфони", "Мобільні телефони та смартфони");
        var laptops = catalog.AddCategory("Ноутбуки", "Ноутбуки для роботи, навчання та ігор");
        var tablets = catalog.AddCategory("Планшети", "Планшети та електронні книги");
        var audio = catalog.AddCategory("Аудіо", "Навушники, колонки та саундбари");
        var wearables = catalog.AddCategory("Смарт-годинники", "Розумні годинники та фітнес-браслети");
        var accessories = catalog.AddCategory("Аксесуари", "Зарядки, павербанки, кабелі, миші");

        Product Add(string sku, string name, string brand, Category category, decimal price, int stock,
            int warranty, string description, params (string Key, string Value)[] specs)
        {
            var product = new Product(sku, name, brand, category.Id, price, stock, warranty) { Description = description };
            foreach (var (key, value) in specs)
                product.SetSpecification(key, value);
            catalog.AddProduct(product);
            return product;
        }

        var iphone = Add("PHN-1001", "iPhone 16 128GB", "Apple", phones, 41_999m, 12, 12,
            "Флагманський смартфон з чипом A18 та камерою 48 Мп.",
            ("Екран", "6.1\" OLED"), ("Пам'ять", "128 ГБ"), ("Камера", "48 Мп"));
        var galaxy = Add("PHN-1002", "Galaxy S25 256GB", "Samsung", phones, 38_499m, 9, 24,
            "Компактний флагман з Galaxy AI.",
            ("Екран", "6.2\" AMOLED 120 Гц"), ("Пам'ять", "256 ГБ"), ("Акумулятор", "4000 мАг"));
        Add("PHN-1003", "Redmi Note 14 Pro", "Xiaomi", phones, 13_999m, 25, 12,
            "Найкраще співвідношення ціни та можливостей.",
            ("Екран", "6.67\" AMOLED"), ("Камера", "200 Мп"), ("Акумулятор", "5110 мАг"));
        Add("PHN-1004", "Pixel 9", "Google", phones, 32_999m, 2, 12,
            "Чистий Android та найкращий нічний режим.",
            ("Екран", "6.3\" OLED"), ("Пам'ять", "128 ГБ"));
        Add("PHN-1005", "Galaxy A56", "Samsung", phones, 17_499m, 0, 24,
            "Надійний середньобюджетник з 6 роками оновлень.",
            ("Екран", "6.7\" AMOLED"), ("Пам'ять", "256 ГБ"));

        var macbook = Add("LAP-2001", "MacBook Air 13 M4", "Apple", laptops, 52_999m, 6, 12,
            "Тонкий і тихий ноутбук без вентилятора.",
            ("Процесор", "Apple M4"), ("ОЗП", "16 ГБ"), ("SSD", "256 ГБ"));
        Add("LAP-2002", "ThinkPad E14 Gen 6", "Lenovo", laptops, 36_999m, 8, 36,
            "Бізнес-ноутбук із класичною клавіатурою.",
            ("Процесор", "Intel Core Ultra 5"), ("ОЗП", "16 ГБ"), ("SSD", "512 ГБ"));
        Add("LAP-2003", "ROG Strix G16", "ASUS", laptops, 64_999m, 3, 24,
            "Ігровий ноутбук з RTX 4070.",
            ("Процесор", "Intel Core i9"), ("Відеокарта", "RTX 4070"), ("Екран", "16\" 240 Гц"));
        Add("LAP-2004", "IdeaPad Slim 3", "Lenovo", laptops, 21_499m, 14, 12,
            "Доступний ноутбук для навчання.",
            ("Процесор", "AMD Ryzen 5"), ("ОЗП", "8 ГБ"));
        Add("LAP-2005", "Swift Go 14", "Acer", laptops, 33_999m, 5, 24,
            "Легкий ноутбук з OLED-екраном.",
            ("Екран", "14\" OLED"), ("Вага", "1.3 кг"));

        Add("TAB-3001", "iPad Air 11 M2", "Apple", tablets, 29_999m, 7, 12,
            "Універсальний планшет для творчості.", ("Екран", "11\" Liquid Retina"));
        Add("TAB-3002", "Galaxy Tab S10 FE", "Samsung", tablets, 19_999m, 10, 24,
            "Планшет зі стилусом S Pen у комплекті.", ("Екран", "10.9\""), ("Стилус", "S Pen"));
        Add("TAB-3003", "Pad 7", "Xiaomi", tablets, 12_499m, 3, 12,
            "Планшет з екраном 144 Гц.", ("Екран", "11.2\" 144 Гц"));
        Add("TAB-3004", "Kindle Paperwhite", "Amazon", tablets, 7_499m, 9, 12,
            "Електронна книга з підсвіткою.", ("Екран", "7\" E-Ink"));

        var airpods = Add("AUD-4001", "AirPods Pro 2", "Apple", audio, 9_999m, 15, 12,
            "Активне шумозаглушення та просторове аудіо.", ("Тип", "TWS"), ("ANC", "Так"));
        var sony = Add("AUD-4002", "WH-1000XM5", "Sony", audio, 13_499m, 6, 24,
            "Еталонні накладні навушники з ANC.", ("Тип", "Накладні"), ("Автономність", "30 год"));
        Add("AUD-4003", "Charge 5", "JBL", audio, 6_299m, 11, 12,
            "Захищена портативна колонка.", ("Захист", "IP67"), ("Автономність", "20 год"));
        Add("AUD-4004", "Galaxy Buds3 Pro", "Samsung", audio, 8_499m, 1, 12,
            "Навушники з адаптивним шумозаглушенням.", ("Тип", "TWS"));
        Add("AUD-4005", "Redmi Buds 6", "Xiaomi", audio, 1_499m, 40, 12,
            "Бюджетні TWS-навушники.", ("Тип", "TWS"), ("Автономність", "38 год"));

        Add("WTC-5001", "Watch Series 10", "Apple", wearables, 18_999m, 5, 12,
            "Найтонший Apple Watch.", ("Корпус", "42 мм"));
        Add("WTC-5002", "Galaxy Watch 7", "Samsung", wearables, 10_999m, 8, 24,
            "Розумний годинник з аналізом сну.", ("Корпус", "44 мм"));
        Add("WTC-5003", "Smart Band 9", "Xiaomi", wearables, 1_799m, 30, 12,
            "Фітнес-браслет з автономністю до 21 дня.", ("Автономність", "21 день"));

        var charger = Add("ACC-6001", "Зарядний пристрій 65W GaN", "Anker", accessories, 1_599m, 35, 18,
            "Компактна зарядка на 3 порти.", ("Потужність", "65 Вт"));
        Add("ACC-6002", "Павербанк 20000 mAh", "Baseus", accessories, 1_299m, 20, 12,
            "Павербанк зі швидкою зарядкою 22.5 Вт.", ("Ємність", "20000 мАг"));
        var mouse = Add("ACC-6003", "MX Master 3S", "Logitech", accessories, 4_299m, 12, 24,
            "Ергономічна миша для продуктивної роботи.", ("Підключення", "Bluetooth / USB"));
        Add("ACC-6004", "Кабель USB-C 2 м", "Ugreen", accessories, 349m, 60, 12,
            "Плетений кабель 100 Вт.", ("Довжина", "2 м"));

        context.Coupons.Add(new PercentageCoupon("WELCOME10", 10m) { ValidFrom = today.AddMonths(-3) });
        context.Coupons.Add(new FixedAmountCoupon("MINUS1000", 1_000m)
        {
            ValidFrom = today.AddMonths(-1), ValidUntil = today.AddMonths(2), MinOrderAmount = 15_000m, UsageLimit = 50
        });
        context.Coupons.Add(new CategoryCoupon("AUDIO15", audio.Id, audio.Name, 15m)
        {
            ValidFrom = today.AddDays(-14), ValidUntil = today.AddDays(30)
        });
        context.Coupons.Add(new PercentageCoupon("BLACKFRIDAY", 20m)
        {
            ValidFrom = today.AddDays(5), ValidUntil = today.AddDays(12), UsageLimit = 100
        });
        context.Coupons.Add(new PercentageCoupon("SUMMER25", 25m)
        {
            ValidFrom = today.AddMonths(-6), ValidUntil = today.AddMonths(-3)
        });
        context.Coupons.Add(new FixedAmountCoupon("ONCE300", 300m) { UsageLimit = 1, ValidFrom = today.AddMonths(-2) });

        // Історію замовлень формуємо через справжній OrderService з "минулим" годинником,
        // тож склад, купони і суми покупок клієнтів залишаються узгодженими
        var clock = new SeedClock();
        var carts = new CartService(context);
        var coupons = new CouponService(context, clock);
        var customers = new CustomerService(context, clock);
        var orders = new OrderService(context, carts, coupons, new PriceCalculator(), clock);

        clock.Now = today.AddDays(-60).AddHours(10);
        var olena = customers.Register("Олена Коваленко", "olena@gmail.com", "+380671234567", "м. Київ, вул. Хрещатик, 15, кв. 8");
        var andrii = customers.Register("Андрій Шевчук", "andrii.shevchuk@ukr.net", "0501112233", "м. Львів, пр. Свободи, 27");
        var maria = customers.Register("Марія Бондар", "maria.bondar@gmail.com", "+380939876543", "м. Одеса, вул. Дерибасівська, 4");

        void Buy(Customer customer, int daysAgo, DeliveryMethod delivery, string? coupon,
            OrderStatus finalStatus, params (Product Product, int Qty)[] lines)
        {
            clock.Now = today.AddDays(-daysAgo).AddHours(9 + daysAgo % 9).AddMinutes(daysAgo * 7 % 60);
            foreach (var (product, qty) in lines)
                carts.Add(customer, product.Id, qty);
            var order = orders.PlaceOrder(customer, delivery, coupon);

            clock.Now = clock.Now.AddHours(20);
            if (finalStatus == OrderStatus.Cancelled)
            {
                orders.Cancel(order);
                return;
            }

            foreach (var status in new[] { OrderStatus.Paid, OrderStatus.Shipped, OrderStatus.Delivered })
            {
                if (order.Status == finalStatus) break;
                orders.ChangeStatus(order, status);
                clock.Now = clock.Now.AddHours(20);
            }
        }

        Buy(olena, 45, DeliveryMethod.Courier, "WELCOME10", OrderStatus.Delivered, (macbook, 1), (mouse, 1));
        Buy(olena, 20, DeliveryMethod.PostOffice, null, OrderStatus.Delivered, (airpods, 1), (charger, 2));
        Buy(andrii, 12, DeliveryMethod.Pickup, "MINUS1000", OrderStatus.Delivered, (galaxy, 1));
        Buy(maria, 7, DeliveryMethod.PostOffice, "AUDIO15", OrderStatus.Shipped, (sony, 1));
        Buy(andrii, 4, DeliveryMethod.Courier, null, OrderStatus.Cancelled, (iphone, 1));
        Buy(olena, 2, DeliveryMethod.Courier, null, OrderStatus.Paid, (iphone, 1));
        Buy(maria, 0, DeliveryMethod.PostOffice, null, OrderStatus.New, (charger, 1), (airpods, 1));

        carts.Add(andrii, airpods.Id, 1);
    }

    private sealed class SeedClock : TimeProvider
    {
        public DateTime Now { get; set; }

        public override DateTimeOffset GetUtcNow() => new DateTimeOffset(Now).ToUniversalTime();

        public override TimeZoneInfo LocalTimeZone => TimeZoneInfo.Local;
    }
}
