namespace PuppetFestAPP.Web.Data;

public class Sale
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public int LocationId { get; set; }
    public Location Location { get; set; } = null!;
    
    public int QuantitySold { get; set; }
    public decimal UnitPriceAtSale { get; set; }
    public DateTime SaleDate { get; set; } = DateTime.UtcNow;
    
    // This helps with your "Over/Under" - was this a live sale or a manual adjustment?
    public bool IsManualAdjustment { get; set; } 
}