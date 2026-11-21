namespace ElectroShop.Core.Models;

public static class DeliveryMethodExtensions
{
    public static string ToDisplayName(this DeliveryMethod method) => method switch
    {
        DeliveryMethod.Pickup => "Самовивіз з магазину",
        DeliveryMethod.PostOffice => "Доставка у відділення пошти",
        DeliveryMethod.Courier => "Кур'єрська доставка",
        _ => method.ToString()
    };
}
