using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PuppetFestAPP.Web.Data;

public class Category
{
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    // Navigation property
    public ICollection<Product> Products { get; set; } = null!;
}