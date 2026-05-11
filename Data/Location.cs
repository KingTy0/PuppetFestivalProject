using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace PuppetFestAPP.Web.Data;

public enum LocationType
{
    Store = 0,
    Van = 1,
    Warehouse = 2,
    Event = 3
}

public class Location
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Address { get; set; } = string.Empty;

    [Required]
    public LocationType Type { get; set; }

    public bool IsActive { get; set; } = true;

    [ValidateNever]
    public ICollection<ProductLocation> ProductLocations { get; set; } = new List<ProductLocation>();
}