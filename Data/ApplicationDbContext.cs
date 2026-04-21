using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace PuppetFestAPP.Web.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    // Identity tables are registered by IdentityDbContext — do not add them here.

    public DbSet<Product> Products { get; set; }
    public DbSet<Category> Categories { get; set; }
    // Use the full path here to be 100% sure
    public DbSet<PuppetFestAPP.Web.Models.Image> Images { get; set; }
    public DbSet<Location> Locations { get; set; }
    public DbSet<ProductLocation> ProductLocations { get; set; }

    public DbSet<Inventory> Inventories { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure enum conversions

        modelBuilder.Entity<Product>()
            .Property(p => p.Color)
            .HasConversion<int>();

        modelBuilder.Entity<Product>()
            .Property(p => p.Size)
            .HasConversion<int>();

        // Configure decimal precision

        modelBuilder.Entity<Product>()
            .Property(p => p.Price)
            .HasPrecision(10, 2);

        // Configure default value for IsActive

        modelBuilder.Entity<Product>()
            .Property(p => p.IsActive)
            .HasDefaultValue(true);

        // Default check flags for existing and new product/location rows.
        modelBuilder.Entity<ProductLocation>()
            .Property(pl => pl.IsBoxChecked)
            .HasDefaultValue(false);

        modelBuilder.Entity<ProductLocation>()
            .Property(pl => pl.IsDeliveryChecked)
            .HasDefaultValue(false);
    }
}
