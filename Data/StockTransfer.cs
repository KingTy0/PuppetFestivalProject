using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation; 

namespace PuppetFestAPP.Web.Data;

public class StockTransfer
{
    public int Id { get; set; }

    [Required]
    public int ProductId { get; set; }

    [Required]
    public int FromLocationId { get; set; }

    [Required]
    public int ToLocationId { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }

    public DateTime TransferDate { get; set; } = DateTime.UtcNow;

    [MaxLength(250)]
    public string? Notes { get; set; }

    public Product Product { get; set; } = null!;
    public Location FromLocation { get; set; } = null!;
    public Location ToLocation { get; set; } = null!;
}