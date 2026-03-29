using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using PuppetFestAPP.Web.Data;

namespace PuppetFestAPP.Web.Models
{
    public class Image
    {
        [Key]
        public int Id { get; set; }

        // Change 'FilePath' to 'FileName' to match SeedData
        [Required]
        public string FileName { get; set; } = string.Empty;

        // Add 'AltText' to match SeedData
        public string? AltText { get; set; }

        public DateTime UploadDate { get; set; } = DateTime.Now;

        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    }
}