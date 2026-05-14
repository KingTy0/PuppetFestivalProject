using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation; // Add this using!

namespace PuppetFestAPP.Web.Data;

public class ProductLocation
{
    public int Id { get; set; }

    [Required]
    public int ProductId { get; set; }

    [ValidateNever] // 1. Add this to stop the loop back to Product
    public Product Product { get; set; } = null!;
    
    [Required]
    [Range(0, int.MaxValue, ErrorMessage = "Stock cannot be negative.")]
    public int Quantity { get; set; } = 0;

    [Required]
    public int LocationId { get; set; }

    [ValidateNever] // 2. Add this to stop the loop into Location
    public Location Location { get; set; } = null!;

    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}