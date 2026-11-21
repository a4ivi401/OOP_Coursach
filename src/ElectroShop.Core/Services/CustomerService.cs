using ElectroShop.Core.Comparers;
using ElectroShop.Core.Exceptions;
using ElectroShop.Core.Models;
using ElectroShop.Core.Persistence;

namespace ElectroShop.Core.Services;

public sealed class CustomerService
{
    private readonly ShopContext _context;
    private readonly TimeProvider _time;

    public CustomerService(ShopContext context, TimeProvider time)
    {
        _context = context;
        _time = time;
    }

    public IReadOnlyList<Customer> GetAll()
    {
        var list = _context.Customers.ToList();
        list.Sort(new CustomerSpendingComparer());
        return list;
    }

    public Customer GetById(Guid id) => _context.Customers.GetById(id);

    public bool IsEmailTaken(string email) => FindByEmail(email) is not null;

    public Customer Register(string fullName, string email, string phone, string address)
    {
        if (IsEmailTaken(email))
            throw new DuplicateEntityException("Клієнт з email", email.Trim().ToLowerInvariant());

        var customer = new Customer(fullName, email, phone, address)
        {
            RegisteredAt = _time.GetLocalNow().DateTime
        };
        _context.Customers.Add(customer);
        return customer;
    }

    public Customer Login(string email) =>
        FindByEmail(email) ?? throw new EntityNotFoundException("Клієнта з email", email.Trim());

    public void UpdateProfile(Customer customer, string fullName, string phone, string address)
    {
        // Спершу валідуємо все на тимчасовому об'єкті, щоб не змінити профіль частково
        _ = new Customer(fullName, customer.Email, phone, address);
        customer.FullName = fullName;
        customer.Phone = phone;
        customer.Address = address;
    }

    public int CountOrders(Guid customerId) => _context.Orders.Find(o => o.CustomerId == customerId).Count;

    private Customer? FindByEmail(string email)
    {
        var normalized = email.Trim().ToLowerInvariant();
        return _context.Customers.FirstOrDefault(c => c.Email == normalized);
    }
}
