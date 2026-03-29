using System.ComponentModel.DataAnnotations;
// Make sure to add the using for your Data folder where Enums live
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
    
    // Add these back as the actual Enum types
    public ProductSize? Size { get; set; }
    public ProductColor? Color { get; set; }
}