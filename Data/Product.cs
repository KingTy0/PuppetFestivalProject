using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PuppetFestAPP.Web.Data;

public enum ProductColor
{
    Red = 1,
    Orange = 2,
    Yellow = 3,
    Green = 4,
    Blue = 5,
    Purple = 6,
    Black = 7,
    White = 8,
    Brown = 9,
    Pink = 10,
    Multi = 11,
    Clear = 12
}

public enum ProductSize
{
    XS = 1,
    S = 2,
    M = 3,
    L = 4,
    XL = 5,
    XXL = 6,
    XXXL = 7
}

public class Product
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [Required]
    public int Quantity { get; set; }

    [Required]
    [Column(TypeName = "decimal(10,2)")]
    public decimal Price { get; set; }

    /// UTC timestamp set automatically when the record is first created
    [Required]
    public DateTime DateAdded { get; set; } = DateTime.UtcNow;

    public ProductSize? Size { get; set; }

    public ProductColor? Color { get; set; }

    [Required]
    public bool IsActive { get; set; } = true;

    // Navigation properties

    [Required]
    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    [Required]
    public int ImageId { get; set; }
    public Image Image { get; set; } = null!;

    public ICollection<ProductLocation> ProductLocations { get; set; } = null!;
}