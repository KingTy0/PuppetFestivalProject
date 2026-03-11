using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PuppetFestAPP.Web.Data;

public class Image
{
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string FileName { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? AltText { get; set; }

    // Navigation property
    public Product Product { get; set; } = null!;
}