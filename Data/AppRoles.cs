namespace PuppetFestAPP.Web.Data;

/// <summary>
/// Central registry of all role names used in the application.
/// Roles control what pages a user can access (via the [Authorize]
/// attribute on Razor components) and what navigation links they see
/// (via &lt;AuthorizeView&gt; in NavMenu.razor).
/// </summary>
public static class AppRoles
{
    /// <summary>Full system access: user management, storage admin, all features.</summary>
    public const string Admin = "Admin";

    /// <summary>Can view and edit inventory counts, add/remove products.</summary>
    public const string InventoryManager = "InventoryManager";

    /// <summary>Can process sales at a merch stand during the festival.</summary>
    public const string SalesStaff = "SalesStaff";

    /// <summary>Can view delivery schedules and confirm stock deliveries.</summary>
    public const string DeliveryStaff = "DeliveryStaff";

    /// <summary>Public-facing role for online customers (future use).</summary>
    public const string Customer = "Customer";

    /// <summary>
    /// Array of all role names. Used by the database seeder to create
    /// roles on first run, and by UI components to populate dropdowns.
    /// </summary>
    public static readonly string[] AllRoles = new[]
    {
        Admin,
        InventoryManager,
        SalesStaff,
        DeliveryStaff,
        Customer
    };
}