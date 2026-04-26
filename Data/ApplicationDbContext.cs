using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace PuppetFestAPP.Web.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    // Identity tables are registered by IdentityDbContext — do not add them here.

    public DbSet<Product> Products { get; set; }
    public DbSet<Category> Categories { get; set; }
<<<<<<< HEAD
    // Use the full path here to be 100% sure
    public DbSet<PuppetFestAPP.Web.Models.Image> Images { get; set; }
    public DbSet<Location> Locations { get; set; }
    public DbSet<ProductLocation> ProductLocations { get; set; }

=======
    public DbSet<PuppetFestAPP.Web.Models.Image> Images { get; set; }
    public DbSet<Location> Locations { get; set; }
    public DbSet<ProductLocation> ProductLocations { get; set; }
    public DbSet<StockTransferBox> StockTransferBoxes { get; set; }
    public DbSet<StockTransferBoxItem> StockTransferBoxItems { get; set; }
    
>>>>>>> main
    public DbSet<Inventory> Inventories { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Product>()
            .Property(p => p.Color)
            .HasConversion<int>();

        modelBuilder.Entity<Product>()
            .Property(p => p.Size)
            .HasConversion<int>();

        modelBuilder.Entity<Product>()
            .Property(p => p.Price)
            .HasPrecision(10, 2);

        modelBuilder.Entity<Product>()
            .Property(p => p.IsActive)
            .HasDefaultValue(true);

<<<<<<< HEAD
        // Default check flags for existing and new product/location rows.
        modelBuilder.Entity<ProductLocation>()
            .Property(pl => pl.IsBoxChecked)
            .HasDefaultValue(false);

        modelBuilder.Entity<ProductLocation>()
            .Property(pl => pl.IsDeliveryChecked)
            .HasDefaultValue(false);
=======
        modelBuilder.Entity<ProductLocation>()
            .HasIndex(pl => new { pl.ProductId, pl.LocationId })
            .IsUnique();
>>>>>>> main
    }
}
