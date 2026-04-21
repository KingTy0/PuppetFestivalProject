using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace PuppetFestAPP.Web.Data;

public class ProductLocation
{
    public int Id { get; set; }

    [Required]
    public int ProductId { get; set; }

    [ValidateNever]
    public Product Product { get; set; } = null!;

    [Required]
    [Range(0, int.MaxValue, ErrorMessage = "Stock cannot be negative.")]
    public int Quantity { get; set; } = 0;

    [Required]
    public int LocationId { get; set; }

    [ValidateNever]
    public Location Location { get; set; } = null!;

    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    // Box/delivery check status is stored on the existing ProductLocation row
    // so the app can track checks without introducing a separate workflow table.
    public bool IsBoxChecked { get; set; }
    public DateTime? BoxCheckedAt { get; set; }
    public bool IsDeliveryChecked { get; set; }
    public DateTime? DeliveryCheckedAt { get; set; }
}
