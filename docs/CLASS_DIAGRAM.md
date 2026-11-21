# Діаграма класів ElectroShop

Діаграми записано в [Mermaid](https://mermaid.js.org/) — вони відображаються прямо на GitHub і в VS Code (розширення *Markdown Preview Mermaid Support*). Для друку чи вставки в презентацію відкрийте [`class-diagram.html`](class-diagram.html) у браузері та збережіть сторінку як PDF (Ctrl+P). Вихідні файли діаграм — у теці [`diagrams/`](diagrams/).

## 0. Архітектура рішення

Три проєкти: бізнес-логіка винесена в окрему бібліотеку `ElectroShop.Core.dll`, консольний інтерфейс лише звертається до фасаду `Shop`.

```mermaid
flowchart LR
    subgraph UI["ElectroShop.UI (Console App)"]
        direction TB
        P[Program] --> A[App]
        A --> MS[MainScreen]
        MS --> CS[Екрани покупця]
        MS --> AS[Екрани адміністратора]
        IN[Input / Ui / Theme]
    end
    subgraph CORE["ElectroShop.Core (Class Library .dll)"]
        direction TB
        SH[Shop — фасад] --> SV[Сервіси:<br/>Catalog, Customer, Cart,<br/>Coupon, Order, Report]
        SV --> CTX[ShopContext<br/>Repository#60;T#62; × 5]
        SV --> PR[PriceCalculator<br/>IDiscountPolicy]
        CTX --> M[Моделі:<br/>Product, Category, Customer,<br/>Order, Coupon…]
        SH --> ST[IDataStorage#60;T#62;<br/>JsonDataStorage]
        EX[ShopException + 9 нащадків]
    end
    subgraph TESTS["ElectroShop.Tests (xUnit)"]
        T[71 тест]
    end
    FILE[(data/shop.json)]
    CS --> SH
    AS --> SH
    ST <--> FILE
    T --> SH
```

## 1. Модель предметної області та ціноутворення

Сутності магазину, ієрархія купонів (поліморфний розрахунок знижки) і калькулятор ціни.

```mermaid
classDiagram
    direction TB

    class IEntity {
        <<interface>>
        +Guid Id
    }
    class EntityBase {
        <<abstract>>
        +Guid Id
        +string DisplayName*
        +ToString() string
    }
    IEntity <|.. EntityBase

    class Category {
        -string _name
        -string _description
        +string Name
        +string Description
    }
    class Product {
        -string _sku
        -decimal _price
        -int _stock
        -Dictionary~string,string~ SpecificationsData
        +string Sku
        +string Name
        +string Brand
        +Guid CategoryId
        +decimal Price
        +int StockQuantity
        +int WarrantyMonths
        +IReadOnlyDictionary Specifications
        +bool IsAvailable
        +bool IsLowStock
        +ReserveStock(int qty)
        +Restock(int qty)
        +EnsureInStock(int qty)
        +SetSpecification(key, value)
        +CompareTo(Product) int
    }
    class Customer {
        -string _email
        -string _phone
        +string FullName
        +string Email
        +string Phone
        +string Address
        +DateTime RegisteredAt
        +decimal TotalSpent
        +Cart Cart
        +LoyaltyLevel LoyaltyLevel
        +NormalizePhone(string)$ string
    }
    class Cart {
        -List~CartItem~ ItemsData
        +IReadOnlyList~CartItem~ Items
        +bool IsEmpty
        +int TotalQuantity
        +Add(Guid productId, int qty)
        +SetQuantity(Guid productId, int qty)
        +Remove(Guid productId) bool
        +Clear()
    }
    class CartItem {
        +Guid ProductId
        +int Quantity
    }
    class Order {
        +int Number
        +Guid CustomerId
        +DateTime CreatedAt
        +OrderStatus Status
        +DeliveryMethod Delivery
        +string ShippingAddress
        +string? CouponCode
        +PriceBreakdown Pricing
        +IReadOnlyList~OrderItem~ Items
        +IReadOnlyList~OrderStatusChange~ History
        +bool CanBeCancelled
        +ChangeStatus(OrderStatus, DateTime)
        +CompareTo(Order) int
    }
    class OrderItem {
        +Guid ProductId
        +Guid CategoryId
        +string ProductName
        +decimal UnitPrice
        +int Quantity
        +decimal LineTotal
    }
    class OrderStatusChange {
        <<record>>
        +OrderStatus Status
        +DateTime ChangedAt
    }
    class OrderStatus {
        <<enumeration>>
        New
        Paid
        Shipped
        Delivered
        Cancelled
    }
    class DeliveryMethod {
        <<enumeration>>
        Pickup
        PostOffice
        Courier
    }
    class LoyaltyLevel {
        <<enumeration>>
        Bronze
        Silver
        Gold
        Platinum
    }

    EntityBase <|-- Category
    EntityBase <|-- Product
    EntityBase <|-- Customer
    EntityBase <|-- Order
    EntityBase <|-- Coupon

    class IComparable~T~ {
        <<interface>>
        +CompareTo(T other) int
    }
    IComparable~T~ <|.. Product
    IComparable~T~ <|.. Order

    Product "*" --> "1" Category : CategoryId
    Customer *-- "1" Cart
    Cart *-- "*" CartItem
    CartItem --> Product : ProductId
    Order *-- "1..*" OrderItem
    Order *-- "1..*" OrderStatusChange
    Order --> Customer : CustomerId
    Order --> OrderStatus
    Order --> DeliveryMethod
    Order *-- PriceBreakdown
    Customer --> LoyaltyLevel

    class IDiscountPolicy {
        <<interface>>
        +string Description
        +CalculateDiscount(PricingContext) decimal
    }
    class Coupon {
        <<abstract>>
        +string Code
        +DateTime ValidFrom
        +DateTime? ValidUntil
        +decimal MinOrderAmount
        +int? UsageLimit
        +int UsedCount
        +bool IsActive
        +string TypeName*
        +string ValueText*
        +EnsureApplicable(PricingContext, DateTime)
        +CalculateDiscount(PricingContext) decimal
        +RegisterUsage()
        +ReleaseUsage()
        #ValidateSpecific(PricingContext)
        #CalculateRawDiscount(PricingContext)* decimal
    }
    class PercentageCoupon {
        +decimal Percent
    }
    class FixedAmountCoupon {
        +decimal Amount
    }
    class CategoryCoupon {
        +Guid CategoryId
        +string CategoryName
        +decimal Percent
        #ValidateSpecific(PricingContext)
    }
    class LoyaltyDiscountPolicy {
        -LoyaltyLevel _level
        +decimal Percent
    }
    IDiscountPolicy <|.. Coupon
    IDiscountPolicy <|.. LoyaltyDiscountPolicy
    Coupon <|-- PercentageCoupon
    Coupon <|-- FixedAmountCoupon
    Coupon <|-- CategoryCoupon

    class PricingContext {
        +IReadOnlyList~OrderItem~ Items
        +decimal Subtotal
        +SubtotalForCategory(Guid) decimal
    }
    class PriceBreakdown {
        <<record>>
        +decimal Subtotal
        +decimal CouponDiscount
        +decimal LoyaltyDiscount
        +decimal DeliveryCost
        +decimal TotalDiscount
        +decimal Total
    }
    class PriceCalculator {
        +FreeDeliveryThreshold$ decimal
        +MaxDiscountShare$ decimal
        +Calculate(items, delivery, Coupon?, LoyaltyLevel) PriceBreakdown
        +GetDeliveryBaseCost(DeliveryMethod)$ decimal
    }
    class LoyaltyProgram {
        <<static>>
        +GetLevel(decimal totalSpent)$ LoyaltyLevel
        +GetPercent(LoyaltyLevel)$ decimal
        +GetNextLevel(LoyaltyLevel)$ LoyaltyLevel?
    }
    PriceCalculator ..> IDiscountPolicy : uses
    PriceCalculator ..> PricingContext : creates
    PriceCalculator ..> PriceBreakdown : creates
    PricingContext o-- OrderItem
    LoyaltyDiscountPolicy ..> LoyaltyProgram
```

## 2. Сервіси, репозиторії, збереження даних

Фасад `Shop`, узагальнений репозиторій `Repository<T>`, JSON-сховище та компаратори для сортування.

```mermaid
classDiagram
    direction TB

    class Shop {
        <<facade>>
        -IDataStorage~ShopSnapshot~ _storage
        -ShopContext _context
        -TimeProvider _time
        +CatalogService Catalog
        +CustomerService Customers
        +CartService Carts
        +CouponService Coupons
        +OrderService Orders
        +ReportService Reports
        +AdminAccess Admin
        +Load()
        +Save()
        +SeedDemoData()
        +BackupStorage() string
    }

    class IRepository~T~ {
        <<interface>>
        +int Count
        +this[Guid id] T
        +Add(T item)
        +Remove(Guid id) bool
        +GetById(Guid id) T
        +FindById(Guid id) T?
        +GetAll() IReadOnlyList~T~
        +Find(Predicate~T~) IReadOnlyList~T~
        +Exists(Predicate~T~) bool
    }
    class Repository~T~ {
        -List~T~ _items
        -string _entityName
    }
    class IEnumerable~T~ {
        <<interface>>
    }
    IEnumerable~T~ <|-- IRepository~T~
    IRepository~T~ <|.. Repository~T~

    class ShopContext {
        +IRepository~Category~ Categories
        +IRepository~Product~ Products
        +IRepository~Customer~ Customers
        +IRepository~Order~ Orders
        +IRepository~Coupon~ Coupons
        +ToSnapshot(DateTime) ShopSnapshot
        +LoadFrom(ShopSnapshot)
    }
    ShopContext *-- "5" Repository~T~

    class IDataStorage~T~ {
        <<interface>>
        +string Location
        +bool Exists
        +Save(T data)
        +Load() T
        +Backup() string
    }
    class JsonDataStorage~T~ {
        -JsonSerializerOptions Options$
        -string _path
    }
    class ShopSnapshot {
        +int Version
        +DateTime SavedAt
        +List~Category~ Categories
        +List~Product~ Products
        +List~Customer~ Customers
        +List~Order~ Orders
        +List~Coupon~ Coupons
    }
    IDataStorage~T~ <|.. JsonDataStorage~T~
    JsonDataStorage~T~ ..> ShopSnapshot : serializes
    ShopContext ..> ShopSnapshot

    class CatalogService {
        +Search(ProductFilter) IReadOnlyList~Product~
        +AddCategory(name, description) Category
        +RemoveCategory(Guid)
        +AddProduct(Product)
        +RemoveProduct(Guid)
        +Restock(Guid, int)
        +GetBrands() IReadOnlyList~string~
        +GetLowStock() IReadOnlyList~Product~
    }
    class CustomerService {
        +Register(name, email, phone, address) Customer
        +Login(email) Customer
        +UpdateProfile(Customer, name, phone, address)
        +GetAll() IReadOnlyList~Customer~
    }
    class CartService {
        +GetLines(Customer) IReadOnlyList~CartLine~
        +Add(Customer, Guid, int)
        +SetQuantity(Customer, Guid, int)
        +Remove(Customer, Guid) bool
    }
    class CouponService {
        +Add(Coupon)
        +Resolve(code, PricingContext) Coupon
        +GetAvailable() IReadOnlyList~Coupon~
        +SetActive(Coupon, bool)
    }
    class OrderService {
        +event LowStock
        +Preview(Customer, DeliveryMethod, code?) PriceBreakdown
        +PlaceOrder(Customer, DeliveryMethod, code?) Order
        +ChangeStatus(Order, OrderStatus)
        +Cancel(Order)
        +GetAll(OrderStatus?) IReadOnlyList~Order~
    }
    class ReportService {
        +GetSummary() SalesSummary
        +GetTopProducts(int) IReadOnlyList~ProductSales~
        +GetRevenueByCategory() IReadOnlyList~CategoryRevenue~
        +GetOrdersByStatus() IReadOnlyDictionary
    }
    class AdminAccess {
        -int _failedAttempts
        +bool IsLocked
        +int AttemptsLeft
        +Authenticate(string pin)
    }
    class ProductFilter {
        -HashSet~string~ _brands
        +string? SearchText
        +Guid? CategoryId
        +bool InStockOnly
        +ProductSortOption SortBy
        +SetPriceRange(min, max)
        +SetBrands(IEnumerable~string~)
        +Matches(Product) bool
    }
    class LowStockEventArgs {
        +Product Product
    }
    class CartLine {
        <<record>>
        +Product Product
        +int Quantity
        +decimal LineTotal
    }

    Shop o-- IDataStorage~T~
    Shop *-- ShopContext
    Shop *-- CatalogService
    Shop *-- CustomerService
    Shop *-- CartService
    Shop *-- CouponService
    Shop *-- OrderService
    Shop *-- ReportService
    Shop *-- AdminAccess
    OrderService --> CartService
    OrderService --> CouponService
    OrderService --> PriceCalculator
    OrderService ..> LowStockEventArgs : raises
    CatalogService ..> ProductFilter
    CartService ..> CartLine

    class IComparer~Product~ {
        <<interface>>
        +Compare(x, y) int
    }
    class ProductPriceComparer {
        -bool _descending
    }
    class ProductBrandComparer
    class ProductStockComparer
    class CustomerSpendingComparer
    class ProductComparers {
        <<static>>
        +For(ProductSortOption)$ IComparer~Product~
    }
    IComparer~Product~ <|.. ProductPriceComparer
    IComparer~Product~ <|.. ProductBrandComparer
    IComparer~Product~ <|.. ProductStockComparer
    ProductComparers ..> IComparer~Product~ : creates
    CatalogService ..> ProductComparers
    CustomerService ..> CustomerSpendingComparer
```

## 3. Власні винятки

Усі помилки бізнес-логіки успадковані від `ShopException`, тож UI може обробити їх одним `catch`.

```mermaid
classDiagram
    direction TB
    class Exception {
        <<.NET>>
        +string Message
        +Exception InnerException
    }
    class ShopException {
        +ShopException(message)
        +ShopException(message, inner)
    }
    class OutOfStockException {
        +string ProductName
        +int Requested
        +int Available
    }
    class InvalidDiscountException {
        +string? CouponCode
    }
    class EntityNotFoundException {
        +string EntityName
        +string Key
    }
    class DuplicateEntityException {
        +string EntityName
        +string Key
    }
    class EntityInUseException
    class EmptyCartException
    class InvalidOrderStateException {
        +OrderStatus From
        +OrderStatus To
    }
    class AccessDeniedException
    class DataStorageException {
        +string Path
    }
    Exception <|-- ShopException
    ShopException <|-- OutOfStockException
    ShopException <|-- InvalidDiscountException
    ShopException <|-- EntityNotFoundException
    ShopException <|-- DuplicateEntityException
    ShopException <|-- EntityInUseException
    ShopException <|-- EmptyCartException
    ShopException <|-- InvalidOrderStateException
    ShopException <|-- AccessDeniedException
    ShopException <|-- DataStorageException
```

## 4. Консольний інтерфейс

Екрани успадковують абстрактний `MenuScreen` (шаблонний метод), введення централізовано в `Input`.

```mermaid
classDiagram
    direction TB
    class App {
        -Shop _shop
        -NotificationCenter _notifications
        +Run()
        -LoadData() bool
        -SaveWithRetry()
        -OnCancelKeyPress(sender, e)
    }
    class MenuScreen {
        <<abstract>>
        #Shop Shop
        #string Title*
        #string BackLabel
        #RenderBody()
        #GetMenu()* IEnumerable~MenuItem~
        #Close()
        +Show()
    }
    class MenuItem {
        <<record>>
        +string Label
        +Action Action
        +bool ClosesScreen
    }
    class Input {
        <<static>>
        +Ask~T~(label, parse, current?) T
        +Int(label, min, max) int
        +Decimal(label, min, max) decimal
        +Date(label) DateTime
        +Confirm(question) bool
        +Select~T~(title, items, display) T?
    }
    class Ui {
        <<static>>
        +Header()
        +Run(Action action)
        +Error(Exception)
        +Success(message)
        +CardRow(Panel[])
    }
    class Theme {
        <<static>>
        +Money(decimal)$ string
        +StatusMarkup(OrderStatus)$ string
    }
    class NotificationCenter {
        +Items
        +OnLowStock(sender, LowStockEventArgs)
    }
    class InputCancelledException

    MenuScreen <|-- MainScreen
    MenuScreen <|-- CustomerScreen
    MenuScreen <|-- CatalogScreen
    MenuScreen <|-- CartScreen
    MenuScreen <|-- MyOrdersScreen
    MenuScreen <|-- ProfileScreen
    MenuScreen <|-- AdminScreen
    MenuScreen <|-- ProductsAdminScreen
    MenuScreen <|-- CategoriesAdminScreen
    MenuScreen <|-- OrdersAdminScreen
    MenuScreen <|-- CouponsAdminScreen
    MenuScreen <|-- CustomersAdminScreen

    class CheckoutFlow {
        <<static>>
        +Run(Shop, Customer) bool
    }
    class ReportsView {
        <<static>>
        +Show(Shop)
    }

    App --> MainScreen
    App --> NotificationCenter
    MenuScreen ..> MenuItem
    MenuScreen ..> Ui
    MenuScreen ..> Input
    MainScreen --> CustomerScreen
    MainScreen --> AdminScreen
    CustomerScreen --> CatalogScreen
    CustomerScreen --> CartScreen
    CustomerScreen --> MyOrdersScreen
    CustomerScreen --> ProfileScreen
    CartScreen --> CheckoutFlow
    AdminScreen --> ReportsView
    AdminScreen --> NotificationCenter
    Input ..> InputCancelledException : throws
```

## Умовні позначення

| Позначення | Значення |
|---|---|
| `<\|--` суцільна лінія з трикутником | успадкування (наслідування класу) |
| `<\|..` пунктир з трикутником | реалізація інтерфейсу |
| `*--` ромб зафарбований | композиція (частина не існує без цілого) |
| `o--` ромб порожній | агрегація |
| `-->` | асоціація (посилання) |
| `..>` | залежність (використовує / створює) |
| `+` / `-` / `#` | public / private / protected |
| `*` після члена | абстрактний член |
| `$` після члена | статичний член |
| `~T~` | узагальнений параметр (generic) |
