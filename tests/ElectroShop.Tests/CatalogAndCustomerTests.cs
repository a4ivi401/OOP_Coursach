using ElectroShop.Core.Comparers;
using ElectroShop.Core.Exceptions;
using ElectroShop.Core.Models;
using ElectroShop.Core.Repositories;
using ElectroShop.Core.Services;

namespace ElectroShop.Tests;

public class CatalogAndCustomerTests
{
    private readonly ShopFixture _f = new();

    [Fact]
    public void Filter_ByPriceRangeAndBrand()
    {
        var filter = new ProductFilter();
        filter.SetPriceRange(250, 5_000);
        filter.SetBrands(["acme"]);

        var result = _f.Shop.Catalog.Search(filter);

        Assert.Equal([_f.Cable], result);
    }

    [Fact]
    public void Filter_InStockOnly_HidesZeroStock()
    {
        var filter = new ProductFilter { InStockOnly = true };
        Assert.DoesNotContain(_f.Cable, _f.Shop.Catalog.Search(filter));
    }

    [Fact]
    public void Filter_RejectsInvertedPriceRange() =>
        Assert.Throws<ArgumentException>(() => new ProductFilter().SetPriceRange(100, 10));

    [Fact]
    public void Filter_SearchMatchesNameAndSku()
    {
        var filter = new ProductFilter { SearchText = "aud-0001" };
        Assert.Equal([_f.Headphones], _f.Shop.Catalog.Search(filter));
    }

    [Theory]
    [InlineData(ProductSortOption.PriceAscending, "Cable", "Phone X")]
    [InlineData(ProductSortOption.PriceDescending, "Phone X", "Cable")]
    [InlineData(ProductSortOption.Stock, "Phone X", "Cable")]
    public void Sorting_UsesComparers(ProductSortOption option, string first, string last)
    {
        var result = _f.Shop.Catalog.Search(new ProductFilter { SortBy = option });
        Assert.Equal(first, result[0].Name);
        Assert.Equal(last, result[^1].Name);
    }

    [Fact]
    public void DuplicateSku_Throws() =>
        Assert.Throws<DuplicateEntityException>(() =>
            _f.Shop.Catalog.AddProduct(new Product("phn-0001", "Other", "Acme", _f.Phones.Id, 1m, 1)));

    [Fact]
    public void RemoveCategoryWithProducts_Throws() =>
        Assert.Throws<EntityInUseException>(() => _f.Shop.Catalog.RemoveCategory(_f.Phones.Id));

    [Fact]
    public void RemoveProduct_AlsoRemovesItFromCarts()
    {
        _f.Shop.Carts.Add(_f.Customer, _f.Phone.Id, 1);
        _f.Shop.Catalog.RemoveProduct(_f.Phone.Id);
        Assert.True(_f.Customer.Cart.IsEmpty);
    }

    [Fact]
    public void Register_DuplicateEmail_IgnoringCase_Throws() =>
        Assert.Throws<DuplicateEntityException>(() =>
            _f.Shop.Customers.Register("Інший Клієнт", "TEST@mail.com", "+380501111111", "м. Київ, вул. 2"));

    [Fact]
    public void Login_UnknownEmail_Throws() =>
        Assert.Throws<EntityNotFoundException>(() => _f.Shop.Customers.Login("nobody@mail.com"));

    [Fact]
    public void UpdateProfile_WithInvalidPhone_LeavesProfileUnchanged()
    {
        Assert.Throws<ArgumentException>(() =>
            _f.Shop.Customers.UpdateProfile(_f.Customer, "Нове Ім'я", "123", "Нова адреса 1"));
        Assert.Equal("Тест Тестович", _f.Customer.FullName);
    }

    [Fact]
    public void Repository_GetMissing_ThrowsNotFound()
    {
        var repo = new Repository<Category>("Категорію");
        Assert.Throws<EntityNotFoundException>(() => repo[Guid.NewGuid()]);
    }

    [Fact]
    public void Repository_AddSameEntityTwice_Throws()
    {
        var repo = new Repository<Category>("Категорію");
        var category = new Category("Тест");
        repo.Add(category);
        Assert.Throws<DuplicateEntityException>(() => repo.Add(category));
    }

    [Fact]
    public void AdminAccess_LocksAfterThreeWrongPins()
    {
        var access = _f.Shop.Admin;
        for (var i = 0; i < AdminAccess.MaxAttempts; i++)
            Assert.Throws<AccessDeniedException>(() => access.Authenticate("0000"));

        Assert.True(access.IsLocked);
        Assert.Throws<AccessDeniedException>(() => access.Authenticate("1234"));
    }

    [Fact]
    public void AdminAccess_AcceptsCorrectPin() => _f.Shop.Admin.Authenticate("1234");
}
