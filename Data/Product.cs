using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using PuppetFestAPP.Web.Models;

namespace PuppetFestAPP.Web.Data;

public enum ProductColor
{
    NA = 0,
    Red = 1, Orange = 2, Yellow = 3, Green = 4,
    Blue = 5, Purple = 6, Black = 7, White = 8,
    Brown = 9, Pink = 10, Multi = 11, Clear = 12
}

public enum ProductSize
{
    NA = 0,
    XS = 1, S = 2, M = 3, L = 4, XL = 5, XXL = 6, XXXL = 7
}

public class Product
{
    public int Id { get; set; }

    // --- PARENT vs VARIANT discriminator ---
    public int? ParentProductId { get; set; }

    [ValidateNever] // Added here to prevent recursion
    public Product? ParentProduct { get; set; }

    [ValidateNever] // Added here to prevent recursion
    public ICollection<Product> Variants { get; set; } = new List<Product>();

    // --- SHARED fields ---
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    // --- VARIANT-LEVEL fields ---
    [Required]
    [Column(TypeName = "decimal(10,2)")]
    public decimal Price { get; set; }

    public ProductSize? Size { get; set; }

    public ProductColor? Color { get; set; }

    [MaxLength(100)]
    public string? Material { get; set; }

    [Required]
    public bool IsActive { get; set; } = true;

    [Required]
    public DateTime DateAdded { get; set; } = DateTime.UtcNow;

    // --- NAVIGATION ---
    public int? ImageId { get; set; }
    
    [ValidateNever]
    public Image? Image { get; set; }

    [Required]
    public int CategoryId { get; set; }

    [ValidateNever] // Added here so the form doesn't try to validate the whole Category object
    public Category Category { get; set; } = null!;

    [ValidateNever] // This solves your "64 recursion depth" error!
    public ICollection<ProductLocation> ProductLocations { get; set; } = new List<ProductLocation>();

    public virtual ICollection<Inventory> Inventories { get; set; } = new List<Inventory>();

}