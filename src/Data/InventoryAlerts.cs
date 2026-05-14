namespace PuppetFestAPP.Web.Data;

public static class InventoryAlerts
{
    public const int LowStockThreshold = 10;
    public const int CriticalStockThreshold = 5;

    public static bool IsLowStock(int quantity) => quantity < LowStockThreshold;
    public static bool IsCriticalStock(int quantity) => quantity <= CriticalStockThreshold;
}
