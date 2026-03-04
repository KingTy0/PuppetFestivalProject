
using Microsoft.AspNetCore.Identity;

namespace PuppetFestAPP.Web.Data;

/// <summary>
/// Seeds the database with required roles and a default admin user.
/// Called once at startup from Program.cs. All operations are
/// idempotent (safe to re-run without creating duplicates).
/// </summary>
public static class SeedData
{
    /// <summary>
    /// Creates all application roles and the default admin user
    /// if they don't already exist.
    /// </summary>
    /// <param name="serviceProvider">
    /// The scoped service provider from the startup block in Program.cs.
    /// Used to resolve RoleManager and UserManager from dependency injection.
    /// </param>
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        // ── Resolve the Identity managers from DI ──
        // RoleManager<IdentityRole>: creates/queries roles in AspNetRoles table
        // UserManager<ApplicationUser>: creates/queries users in AspNetUsers table
        // Both of these are registered by .AddIdentityCore() in Program.cs.
        var roleManager = serviceProvider
            .GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider
            .GetRequiredService<UserManager<ApplicationUser>>();

        // ── Step 1: Create all roles ──
        // Iterates through every role name in AppRoles.AllRoles and
        // creates it in the database if it doesn't already exist.
        foreach (var roleName in AppRoles.AllRoles)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                var result = await roleManager.CreateAsync(
                    new IdentityRole(roleName));

                if (result.Succeeded)
                {
                    Console.WriteLine($"  ✓ Created role: {roleName}");
                }
                else
                {
                    // Identity returns structured errors (e.g., duplicate name).
                    // Join them into a readable string for the console.
                    Console.WriteLine($"  ✗ Failed to create role {roleName}: " +
                        string.Join(", ",
                            result.Errors.Select(e => e.Description)));
                }
            }
            else
            {
                Console.WriteLine($"  - Role already exists: {roleName}");
            }
        }

        // ── Step 2: Create a default Admin user ──
        // This gives you an account to log in with immediately.
        // In production, change this password after first login!
        var adminEmail = "admin@puppetfest.local";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser == null)
        {
            // Create the user object (not yet saved to DB)
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,   // Identity uses this for login
                Email = adminEmail,
                EmailConfirmed = true    // Skip email verification for seed user
            };

            // CreateAsync hashes the password and saves to AspNetUsers
            var createResult = await userManager.CreateAsync(
                adminUser, "Admin123!");

            if (createResult.Succeeded)
            {
                // Link the user to the Admin role in AspNetUserRoles table
                await userManager.AddToRoleAsync(adminUser, AppRoles.Admin);
                Console.WriteLine(
                    $"  ✓ Created admin user: {adminEmail} " +
                    $"(password: Admin123!)");
            }
            else
            {
                Console.WriteLine(
                    $"  ✗ Failed to create admin user: " +
                    string.Join(", ",
                        createResult.Errors.Select(e => e.Description)));
            }
        }
        else
        {
            Console.WriteLine(
                $"  - Admin user already exists: {adminEmail}");
        }
    }
}
