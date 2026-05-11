using System.ComponentModel.DataAnnotations;
using PuppetFestAPP.Web.Data; 

namespace PuppetFestAPP.Web.Models;

public class ProductFormModel
{
    [Required(ErrorMessage = "Name is required")]
    public string Name { get; set; } = string.Empty;

    [Required]
    public decimal Price { get; set; }

    [Required(ErrorMessage = "Please select a category")]
    public int CategoryId { get; set; }

    public string? Description { get; set; }
    public int? ParentProductId { get; set; }

    public string? ImageFileName { get; set; }
    
    // Only define these once!
    public ProductSize? Size { get; set; } = ProductSize.NA;
    public ProductColor? Color { get; set; } = ProductColor.NA;
}