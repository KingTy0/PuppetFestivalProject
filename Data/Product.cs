using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PuppetFestAPP.Web.Data;

/// <summary>
/// Represents a single merchandise item available for sale at festival stands.
/// Maps to the Products table in the SQLite database.
/// </summary>
public class Product
{
    /// <summary>
    /// Primary key. EF Core recognises the name "Id" and configures
    /// auto-increment automatically for SQLite INTEGER columns.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// The display name shown to customers and staff.
    /// MaxLength matches the NVARCHAR(100) defined in the ERD.
    /// Required enforces a NOT NULL constraint in the database.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Optional longer description of the product.
    /// Nullable string maps to a NULL-able column — no [Required] attribute.
    /// </summary>
    [MaxLength(500)]
    public string? Description { get; set; }

    /// <summary>
    /// Unit price in USD.
    /// Column attribute sets precision and scale to match DECIMAL(10,2) in ERD.
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(10,2)")]
    public decimal Price { get; set; }

    /// <summary>
    /// Current total inventory count across all festival locations.
    /// </summary>
    [Required]
    public int StockQuantity { get; set; }

    /// <summary>
    /// Optional relative path or URL to the product's image asset.
    /// </summary>
    [MaxLength(500)]
    public string? ImageUrl { get; set; }

    /// <summary>
    /// When false, the item is hidden from sales staff and customers
    /// even if StockQuantity is greater than zero.
    /// </summary>
    [Required]
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// UTC timestamp set automatically when the record is first created.
    /// See ApplicationDbContext.SaveChangesAsync() for the auto-set logic.
    /// </summary>
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // -------------------------------------------------------------------------
    // Navigation properties — EF Core uses these to resolve relationships.
    // They correspond to the FK line drawn in the ERD diagram.
    // -------------------------------------------------------------------------

    /// <summary>
    /// Foreign key column. Stores the Id of the related Category record.
    /// Matches the CategoryId FK column defined in the ERD.
    /// </summary>
    public int CategoryId { get; set; }

    /// <summary>
    /// Navigation property. EF Core populates this with the full Category
    /// object when the query uses .Include(p => p.Category).
    /// The ERD relationship line (N:1 from Products to Categories) is
    /// expressed in code as this property paired with CategoryId above.
    /// </summary>
    public Category Category { get; set; } = null!;
}