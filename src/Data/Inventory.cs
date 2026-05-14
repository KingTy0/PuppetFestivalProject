using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PuppetFestAPP.Web.Data
{
    public class Inventory
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ProductId { get; set; }

        // The actual count of items in stock
        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Quantity cannot be negative")]
        public int Quantity { get; set; }

        [MaxLength(50)]
        public string? Location { get; set; } // e.g., "Booth A", "Warehouse", "Box 4"

        public DateTime LastUpdated { get; set; } = DateTime.Now;

        // Navigation property back to the Product
        [ForeignKey("ProductId")]
        public virtual Product? Product { get; set; }
    }
}