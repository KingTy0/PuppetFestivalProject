using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace PuppetFestAPP.Web.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    // Identity tables are registered by IdentityDbContext — do not add them here.

    /// <summary>
    /// Exposes the Products table for querying and persistence.
    /// EF Core maps this to the "Products" table (pluralised from "Product").
    /// </summary>
    public DbSet<Product> Products { get; set; }

    /// <summary>
    /// Exposes the Categories table.
    /// </summary>
    public DbSet<Category> Categories { get; set; }

}
