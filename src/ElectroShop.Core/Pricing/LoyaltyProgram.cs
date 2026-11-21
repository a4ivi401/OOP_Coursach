using ElectroShop.Core.Models;

namespace ElectroShop.Core.Pricing;

public static class LoyaltyProgram
{
    private static readonly (LoyaltyLevel Level, decimal Threshold, decimal Percent)[] Tiers =
    [
        (LoyaltyLevel.Platinum, 100_000m, 7m),
        (LoyaltyLevel.Gold, 50_000m, 5m),
        (LoyaltyLevel.Silver, 20_000m, 3m),
        (LoyaltyLevel.Bronze, 0m, 0m)
    ];

    public static LoyaltyLevel GetLevel(decimal totalSpent) =>
        Tiers.First(t => totalSpent >= t.Threshold).Level;

    public static decimal GetPercent(LoyaltyLevel level) =>
        Tiers.First(t => t.Level == level).Percent;

    public static decimal GetThreshold(LoyaltyLevel level) =>
        Tiers.First(t => t.Level == level).Threshold;

    public static LoyaltyLevel? GetNextLevel(LoyaltyLevel level) =>
        level == LoyaltyLevel.Platinum ? null : level + 1;

    public static string ToDisplayName(this LoyaltyLevel level) => level switch
    {
        LoyaltyLevel.Bronze => "Бронзовий",
        LoyaltyLevel.Silver => "Срібний",
        LoyaltyLevel.Gold => "Золотий",
        LoyaltyLevel.Platinum => "Платиновий",
        _ => level.ToString()
    };
}
