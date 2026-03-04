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

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentityCore<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>() 
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();



// ── Backup Services ──
builder.Services.AddScoped<SqliteBackupService>();
builder.Services.AddSingleton<BackupQueueService>();
builder.Services.AddHostedService<TimedBackupService>();


var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var config = services.GetRequiredService<IConfiguration>();
    var logger = services.GetRequiredService<ILogger<Program>>();

    // Step 1: Resolve DB path
    var connStr = config.GetConnectionString("DefaultConnection")
        ?? "DataSource=app.db;Cache=Shared";

    var dbPath = connStr
        .Split(';')
        .Select(s => s.Trim())
        .FirstOrDefault(s =>
            s.StartsWith("DataSource=", StringComparison.OrdinalIgnoreCase) ||
            s.StartsWith("Data Source=", StringComparison.OrdinalIgnoreCase))
        ?.Split('=', 2)[1]?.Trim()
        ?? "app.db";

    // Step 2: Restore if missing
    if (!File.Exists(dbPath))
    {
        logger.LogInformation("Database file not found at {DbPath}. Attempting restore from backup...", dbPath);

        var useStorage = config.GetValue<bool>("Backup:UseStorage", false);
        var blobConnStr = config["Backup:BlobConnectionString"];

        if (useStorage && !string.IsNullOrWhiteSpace(blobConnStr))
        {
            try
            {
                var backupService = services.GetRequiredService<SqliteBackupService>();
                var blobName = await backupService.RestoreLatestToPathAsync(dbPath);
                logger.LogInformation("Auto-restored database from: {BlobName}", blobName);
            }
            catch (Exception ex)
            {
                logger.LogWarning("No backup to restore: {Message}", ex.Message);
            }
        }
        else
        {
            logger.LogInformation("Backup not configured. A fresh database will be created by Migrate().");
        }
    }

    // Step 3: Migrate
    var context = services.GetRequiredService<ApplicationDbContext>();
    context.Database.Migrate();

    // Step 4: Seed
    await SeedData.InitializeAsync(services);
}



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
}

app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();

app.Run();
