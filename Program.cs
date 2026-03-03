using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PuppetFestAPP.Web.Components;
using PuppetFestAPP.Web.Components.Account;
using PuppetFestAPP.Web.Data;

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

builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

var app = builder.Build();


// ── Startup: Migrate database and seed initial data ──
// This block runs once when the app starts. It ensures the database
// schema is up to date (Migrate) and that roles + admin user exist (Seed).
// We wrap it in a "scope" because EF Core DbContext is a scoped service —
// it needs a defined lifetime boundary, which the scope provides.
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
// ── End startup block ──




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
