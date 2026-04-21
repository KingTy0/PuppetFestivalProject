using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PuppetFestAPP.Web.Components;
using PuppetFestAPP.Web.Components.Account;
using PuppetFestAPP.Web.Data;
using PuppetFestAPP.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = IdentityConstants.ApplicationScheme;
    options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
})
    .AddIdentityCookies();

// Role-based authorization policies. These keep page access rules and
// navigation visibility aligned with the Admin > SM > FOH > Driver hierarchy.
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(AppPolicies.CanViewProducts, policy =>
        policy.RequireRole(AppRoles.ViewProductRoles));

    options.AddPolicy(AppPolicies.CanEditProducts, policy =>
        policy.RequireRole(AppRoles.EditProductRoles));

    options.AddPolicy(AppPolicies.CanEnterSales, policy =>
        policy.RequireRole(AppRoles.SalesInputRoles));

    options.AddPolicy(AppPolicies.CanManageDeliveryChecks, policy =>
        policy.RequireRole(AppRoles.DeliveryCheckRoles));

    options.AddPolicy(AppPolicies.CanManageUsers, policy =>
        policy.RequireRole(AppRoles.UserManagementRoles));
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.")));

builder.Services.AddQuickGridEntityFrameworkAdapter();
builder.Services.AddDatabaseDeveloperPageExceptionFilter();



// ── Identity Configuration ──
// AddIdentityCore registers the user management services.
// .AddRoles<IdentityRole>() is CRITICAL — without it, the RoleManager
// service won't be available in DI and the seeder will crash at runtime
// with: "No service for type 'RoleManager<IdentityRole>' has been registered"
builder.Services.AddIdentityCore<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddSingleton<IEmailSender<ApplicationUser>, BrevoEmailSender>();

// ── Backup Services ──
// SqliteBackupService: Scoped — one instance per DI scope (request).
//   Contains the actual backup/restore logic.
builder.Services.AddScoped<SqliteBackupService>();

// BackupQueueService: Singleton — one instance for the entire app.
//   Holds the SemaphoreSlim that serializes all backup operations.
//   Must be Singleton so the semaphore is shared across all callers.
builder.Services.AddSingleton<BackupQueueService>();

// TimedBackupService: Hosted Service — started/stopped by ASP.NET Core.
//   Runs the backup timer loop in the background.
builder.Services.AddHostedService<TimedBackupService>();



builder.Services.AddScoped<ProductService>();



var app = builder.Build();


// ── Startup: Migrate database and seed initial data ──
// This block runs once when the app starts. It ensures the database
// schema is up to date (Migrate) and that roles + admin user exist (Seed).
// We wrap it in a "scope" because EF Core DbContext is a scoped service —
// it needs a defined lifetime boundary, which the scope provides.
/*
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    // GetRequiredService<T> fetches the service from DI, or throws if
    // it's not registered. ApplicationDbContext was registered by
    // AddDbContext<ApplicationDbContext>() earlier in Program.cs.
    var context = services.GetRequiredService<ApplicationDbContext>();

    // Migrate() applies any pending EF Core migrations to the database.
    // On first run, this creates all the Identity tables (AspNetUsers,
    // AspNetRoles, etc.). On subsequent runs, it applies new migrations.
    context.Database.Migrate();

    // Seed roles and admin user (idempotent — safe to run every startup)
    await SeedData.InitializeAsync(services);
}
*/
// ── End startup block ──



// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// STARTUP SEQUENCE: Restore → Migrate → Seed
//
// The order of operations here is CRITICAL:
//   1. Check if the database FILE exists on disk (plain filesystem check)
//   2. If missing, restore from Azure Blob backup (before any EF Core usage)
//   3. NOW create the DbContext and run migrations
//   4. Seed roles and admin user
//
// WHY THIS ORDER? EF Core caches database connections internally. If we
// create a DbContext pointing at an empty/missing file, then overwrite
// that file with a backup, EF Core's cached connection state becomes
// desynced from the actual file on disk, causing "unable to open
// database" errors. By restoring FIRST, EF Core only ever sees the
// correct database file.
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var config = services.GetRequiredService<IConfiguration>();
    var logger = services.GetRequiredService<ILogger<Program>>();

    // ── Step 1: Resolve the database file path ──
    // Parse it from the connection string, handling both "DataSource="
    // (no space, used by EF Core convention) and "Data Source="
    // (with space, used by Microsoft.Data.Sqlite convention).
    var connStr = config.GetConnectionString("DefaultConnection")
        ?? "DataSource=app.db;Cache=Shared";
    var dbPath = connStr
        .Split(';')
        .Select(s => s.Trim())
        .FirstOrDefault(s =>
            s.StartsWith("DataSource=",
                StringComparison.OrdinalIgnoreCase) ||
            s.StartsWith("Data Source=",
                StringComparison.OrdinalIgnoreCase))
        ?.Split('=', 2)[1]?.Trim()
        ?? "app.db";

    // ── Step 2: Restore from backup if database file is missing ──
    // This is the typical scenario when a new container starts in Azure
    // (fresh filesystem, no existing database).
    if (!File.Exists(dbPath))
    {
        logger.LogInformation(
            "Database file not found at {DbPath}. " +
            "Attempting restore from backup...", dbPath);

        var useStorage = config.GetValue<bool>(
            "Backup:UseStorage", false);
        var blobConnStr = config["Backup:BlobConnectionString"];

        if (useStorage && !string.IsNullOrWhiteSpace(blobConnStr))
        {
            try
            {
                var backupService = services
                    .GetRequiredService<SqliteBackupService>();
                var blobName = await backupService
                    .RestoreLatestToPathAsync(dbPath);
                logger.LogInformation(
                    "Auto-restored database from: {BlobName}", blobName);
            }
            catch (Exception ex)
            {
                // Not fatal — if no backup exists, Migrate() will create
                // a fresh database in step 3.
                logger.LogWarning(
                    "No backup to restore: {Message}", ex.Message);
            }
        }
        else
        {
            logger.LogInformation(
                "Backup not configured. " +
                "A fresh database will be created by Migrate().");
        }
    }

    // ── Step 3: Create/update the database schema ──
    // NOW it's safe to use DbContext. Either:
    //   (a) The file already existed (normal local development), or
    //   (b) We just restored it from backup, or
    //   (c) No backup existed, so Migrate() will create a new file
    var context = services.GetRequiredService<ApplicationDbContext>();
    context.Database.Migrate();

    // ── Step 4: Seed roles and the default admin user ──
    await SeedData.InitializeAsync(services);
}
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━





// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
    app.UseMigrationsEndPoint();
}

app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();

app.Run();
