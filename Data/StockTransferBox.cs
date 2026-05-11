
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation; 

namespace PuppetFestAPP.Web.Data;

public class StockTransferBox
{
    public int Id { get; set; }
    public int FromLocationId { get; set; }
    public int ToLocationId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    [MaxLength(500)]
    public string? Notes { get; set; }

    public bool IsPickedUp { get; set; } = false;  // ADD THIS LINE
    
    [ValidateNever]
    public Location FromLocation { get; set; } = null!;
    [ValidateNever]
    public Location ToLocation { get; set; } = null!;

    public bool IsDelivered { get; set; } = false;
    public string Status => IsDelivered ? "Delivered" : IsPickedUp ? "In Transit" : "Pending";

    public ICollection<StockTransferBoxItem> Items { get; set; } = new List<StockTransferBoxItem>();
}

public class StockTransferBoxItem
{
    public int Id { get; set; }
    public int StockTransferBoxId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }

    public StockTransferBox StockTransferBox { get; set; } = null!;
    public Product Product { get; set; } = null!;
}