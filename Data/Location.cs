using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PuppetFestAPP.Web.Data;

public class Location
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Address { get; set; } = string.Empty;

    // Navigation property

    public ICollection<ProductLocation> ProductLocations { get; set; } = null!;
}