using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PuppetFestAPP.Web.Data;

public class ProductLocation
{
    public int Id { get; set; }

    // Navigation properties

    [Required]
    public int ProductId { get; set; }
    public Product Product { get; set; }

    [Required]
    public int LocationId { get; set; }
    public Location Location { get; set; }
}