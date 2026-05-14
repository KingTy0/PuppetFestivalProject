// File: Data/SeedData.cs
//
// PURPOSE: Runs once at app startup to ensure the database has:
//   1. All roles defined in AppRoles.cs
//   2. A default Admin user so you can log in immediately
//
// HOW IT WORKS: Called from Program.cs after Database.Migrate().
// Each call is idempotent — it checks whether each role/user exists
// before creating it, so it's safe to run on every startup.
using PuppetFestAPP.Web.Models; // This finds your new Image class
using Image = PuppetFestAPP.Web.Models.Image;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace PuppetFestAPP.Web.Data;

/// <summary>
/// Seeds the database with required roles and a default admin user.
/// Called once at startup from Program.cs. All operations are
/// idempotent (safe to re-run without creating duplicates).
/// </summary>
/// 
#region "Seed Data Class" 
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

        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();








        // Seed 3 vans + warehouse
        var vans = new[]
        {
            new Location { Name = "Van 1", Address = "Mobile - Chicago Festival Route", Type = LocationType.Van },
            new Location { Name = "Van 2", Address = "Mobile - Chicago Festival Route", Type = LocationType.Van },
            new Location { Name = "Van 3", Address = "Mobile - Chicago Festival Route", Type = LocationType.Van },
            new Location { Name = "Warehouse", Address = "123 Storage St, Chicago IL", Type = LocationType.Warehouse }
        };

        foreach (var van in vans)
        {
            if (!context.Locations.Any(l => l.Name == van.Name))
                context.Locations.Add(van);
        }
        await context.SaveChangesAsync();




        // Seed theater locations
var theaters = new[]
{
    new Location { Name = "The Rhapsody Theater", Address = "1328 W. Morse Ave, Chicago, IL", Type = LocationType.Event },
    new Location { Name = "Music Box Theatre", Address = "3733 N. Southport Ave, Chicago, IL", Type = LocationType.Event },
    new Location { Name = "Constellation", Address = "3111 N. Western Ave, Chicago, IL", Type = LocationType.Event },
    new Location { Name = "The Biograph's Zacek McVay Theater", Address = "2433 N. Lincoln Ave, Chicago, IL", Type = LocationType.Event },
    new Location { Name = "Steppenwolf's Merle Reskin Space", Address = "1624 N. Halstead Ave, Chicago, IL", Type = LocationType.Event },
    new Location { Name = "Chopin Theatre", Address = "1543 W. Division St, Chicago, IL", Type = LocationType.Event },
    new Location { Name = "Wirtz Center Chicago Abbott Hall", Address = "710 N. Lake Shore Dr, Chicago, IL", Type = LocationType.Event },
    new Location { Name = "Warwick Allerton Hotel", Address = "701 N. Michigan Ave, Chicago, IL", Type = LocationType.Event },
    new Location { Name = "Studebaker Theater at the Fine Arts Building", Address = "410 S. Michigan Ave, Chicago, IL", Type = LocationType.Event },
    new Location { Name = "Festival Headquarters + Puppet Hub", Address = "410 S. Michigan Ave, Chicago, IL", Type = LocationType.Event },
    new Location { Name = "Depaul's Merle Reskin Theatre", Address = "60 E. Balbo Dr, Chicago, IL", Type = LocationType.Event },
    new Location { Name = "Dance Center Columbia College Chicago", Address = "1306 S. Michigan Ave, Chicago, IL", Type = LocationType.Event },
    new Location { Name = "Reva & David Logan Center for the Arts", Address = "915 E. 60th St, Chicago, IL", Type = LocationType.Event },
};

foreach (var theater in theaters)
{
    if (!context.Locations.Any(l => l.Name == theater.Name))
        context.Locations.Add(theater);
}
await context.SaveChangesAsync();










        SeedProducts(context);

    }
#endregion



    #region "SeedProduct"
    private static void SeedProducts(ApplicationDbContext context)
{
    // 1. Categories — only Apparel and Miscellaneous
    if (!context.Categories.Any())
    {
        context.Categories.AddRange(
            new Category { Id = 1, Name = "Apparel" },
            new Category { Id = 2, Name = "Miscellaneous" }
        );
        context.SaveChanges();
    }

    if (!context.Images.Any())
    {
        context.Images.AddRange(
            new Image { FileName = "La-Liga-Teatro-Elastico-Deer-T-Shirt.png", AltText = "La Liga Teatro Elastico Deer T-Shirt" },
            new Image { FileName = "La-Liga-Teatro-Elastico-Wolf-Sweatshirt.png", AltText = "La Liga Teatro Elastico Wolf Sweatshirt" },
            new Image { FileName = "Puppet-Fest-Logo-Black-T-Shirt.png", AltText = "Puppet Fest Logo Black T-Shirt" },
            new Image { FileName = "Puppet-Fest-Logo-Orchid-T-Shirt.png", AltText = "Puppet Fest Logo Orchid T-Shirt" },
            new Image { FileName = "Puppet-Fest-Logo-Purple-T-Shirt.png", AltText = "Puppet Fest Logo Purple T-Shirt" },
            new Image { FileName = "Puppet-Fest-Logo-Tote-Bag.png", AltText = "Puppet Fest Logo Tote Bag" },
            new Image { FileName = "Stemless-Wine-Glass.png", AltText = "9oz Stemless Wine Glass" },
            new Image { FileName = "Galaxy-Of-Things-Paperback.png", AltText = "Book - A Galaxy of Things (Paperback)" },
            new Image { FileName = "Boy-Puppet-Button.png", AltText = "Boy Puppet Button" },
            new Image { FileName = "Die-Cut-Puppet-Stickers.png", AltText = "Die Cut Puppet Stickers (Pack of 4)" },
            new Image { FileName = "Toy-Binoculars.png", AltText = "DIY Toy Binoculars" },
            new Image { FileName = "Birch-Waffle-Cuffed-Beanie.png", AltText = "Puppet Fest Cuffed Waffle Beanie - Birch" },
            new Image { FileName = "Camel-Waffle-Cuffed-Beanie.png", AltText = "Puppet Fest Cuffed Waffle Beanie - Camel" },
            new Image { FileName = "Puppet-Fest-Notecards.png", AltText = "Puppet Fest Notecards (Pack of 8)" },
            new Image { FileName = "Bottle-Green-Cuffed-Beanie.png", AltText = "Puppet Fest Sustainable Rib Cuffed Beanie - Bottle Green" },
            new Image { FileName = "Burgundy-Cuffed-Beanie.png", AltText = "Puppet Fest Sustainable Rib Cuffed Beanie - Burgundy" },
            new Image { FileName = "Puppet-Head-Button-2023.png", AltText = "Puppet Head Button 2023" },
            new Image { FileName = "Puppet-Head-Button-2024.png", AltText = "Puppet Head Button 2024" },
            new Image { FileName = "Hare-Shadow-Puppet.png", AltText = "Shadow Puppet: The Hare" }
        );
        context.SaveChanges();
    }

    // 3. Locations — Hub fixed with proper name and address
    if (!context.Locations.Any(l => l.Type == LocationType.Store))
{
    context.Locations.Add(new Location 
    { 
        Name = "The Hub", 
        Address = "410 S Michigan Ave, Chicago, IL",
        Type = LocationType.Store
    });
    context.SaveChanges();
}
var hubLocation = context.Locations.First(l => l.Type == LocationType.Store);
int hubId = hubLocation.Id;

    // 4. Products
    if (!context.Products.Any())
{
    // --- PRODUCT 1: DEER T-SHIRT ---
    var deerParent = new Product
    {
        Name = "La Liga Teatro Elastico Deer T-Shirt",
        Description = "La Liga Teatro Elástico's majestic deer puppet...",
        Price = 20.00m, CategoryId = 1, ImageId = 1, IsActive = true
    };
    context.Products.Add(deerParent);
    context.SaveChanges();

    foreach (var size in new[] { ProductSize.XS, ProductSize.S, ProductSize.XL, ProductSize.XXL })
    {
        var v = new Product { Name = $"{deerParent.Name} ({size})", ParentProductId = deerParent.Id, Size = size, Price = 20.00m, Color = ProductColor.NA, CategoryId = 1, IsActive = true };
        context.Products.Add(v); context.SaveChanges();
        context.ProductLocations.Add(new ProductLocation { ProductId = v.Id, LocationId = hubId, Quantity = 25 });
    }
    context.SaveChanges();

    // --- PRODUCT 2: WOLF SWEATSHIRT ---
    var wolfParent = new Product
    {
        Name = "La Liga Teatro Elastico Wolf Sweatshirt",
        Description = "This light blue sweatshirt features the glorious Wolf Puppet...",
        Price = 40.00m, CategoryId = 1, ImageId = 2, IsActive = true
    };
    context.Products.Add(wolfParent);
    context.SaveChanges();

    foreach (var size in new[] { ProductSize.S, ProductSize.XL, ProductSize.XXL })
    {
        var v = new Product { Name = $"{wolfParent.Name} ({size})", ParentProductId = wolfParent.Id, Size = size, Price = 40.00m, Color = ProductColor.NA, CategoryId = 1, IsActive = true };
        context.Products.Add(v); context.SaveChanges();
        context.ProductLocations.Add(new ProductLocation { ProductId = v.Id, LocationId = hubId, Quantity = 33 });
    }
    context.SaveChanges();

    // --- PRODUCT 3: BLACK T-SHIRT ---
    var blackShirt = new Product
    {
        Name = "Puppet Fest Logo Black T-Shirt",
        Description = "Sport the Festival logo in classic puppeteer black!",
        Price = 20.00m, CategoryId = 1, ImageId = 3, IsActive = true
    };
    context.Products.Add(blackShirt);
    context.SaveChanges();

    foreach (var size in new[] { ProductSize.S, ProductSize.M, ProductSize.L, ProductSize.XL, ProductSize.XXL })
    {
        var v = new Product { Name = $"{blackShirt.Name} ({size})", ParentProductId = blackShirt.Id, Size = size, Price = 20.00m, Color = ProductColor.NA, CategoryId = 1, IsActive = true };
        context.Products.Add(v); context.SaveChanges();
        context.ProductLocations.Add(new ProductLocation { ProductId = v.Id, LocationId = hubId, Quantity = 20 });
    }
    context.SaveChanges();

    // --- PRODUCT 4: ORCHID T-SHIRT ---
    var orchidT = new Product
    {
        Name = "Puppet Fest Logo Orchid T-Shirt",
        Description = "A vibrant orchid-colored shirt featuring the Puppet Fest logo.",
        Price = 20.00m, CategoryId = 1, ImageId = 4, IsActive = true
    };
    context.Products.Add(orchidT);
    context.SaveChanges();

    foreach (var size in new[] { ProductSize.S, ProductSize.M, ProductSize.L, ProductSize.XL })
    {
        var v = new Product { Name = $"{orchidT.Name} ({size})", ParentProductId = orchidT.Id, Size = size, Price = 20.00m, Color = ProductColor.NA, CategoryId = 1, IsActive = true };
        context.Products.Add(v); context.SaveChanges();
        context.ProductLocations.Add(new ProductLocation { ProductId = v.Id, LocationId = hubId, Quantity = 15 });
    }
    context.SaveChanges();

    // --- PRODUCT 5: BEANIES ---
    foreach (var b in new[] {
        new { Name = "Puppet Fest Cuffed Waffle Beanie - Birch", ImgId = 12 },
        new { Name = "Puppet Fest Sustainable Rib Cuffed Beanie - Bottle Green", ImgId = 15 }
    })
    {
        var beanie = new Product { Name = b.Name, Description = "Stay warm with our official festival headwear.", Price = 25.00m, CategoryId = 1, ImageId = b.ImgId, IsActive = true };
        context.Products.Add(beanie); context.SaveChanges();
        context.ProductLocations.Add(new ProductLocation { ProductId = beanie.Id, LocationId = hubId, Quantity = 40 });
    }
    context.SaveChanges();

    // --- PRODUCT 6: TOTE BAG ---
    var tote = new Product { Name = "Puppet Fest Logo Tote Bag", Description = "Carry your stuff while repping your favorite Puppet Fest!", Price = 20.00m, CategoryId = 2, ImageId = 6, IsActive = true };
    context.Products.Add(tote); context.SaveChanges();
    context.ProductLocations.Add(new ProductLocation { ProductId = tote.Id, LocationId = hubId, Quantity = 100 });
    context.SaveChanges();

    // --- PRODUCT 7: WINE GLASS ---
    var wineGlass = new Product { Name = "9oz Stemless Wine Glass", Description = "Perfect for festival evenings. Features a crisp Puppet Fest logo.", Price = 12.00m, CategoryId = 2, ImageId = 7, IsActive = true };
    context.Products.Add(wineGlass); context.SaveChanges();
    context.ProductLocations.Add(new ProductLocation { ProductId = wineGlass.Id, LocationId = hubId, Quantity = 48 });
    context.SaveChanges();

    // --- PRODUCT 8: BOOK ---
    var book = new Product { Name = "A Galaxy of Things (Paperback)", Description = "A deep dive into the art and history of puppetry by various authors.", Price = 25.00m, CategoryId = 2, ImageId = 8, IsActive = true };
    context.Products.Add(book); context.SaveChanges();
    context.ProductLocations.Add(new ProductLocation { ProductId = book.Id, LocationId = hubId, Quantity = 30 });
    context.SaveChanges();

    // --- PRODUCT 9: BOY PUPPET BUTTON ---
    var boyButton = new Product { Name = "Boy Puppet Button", Description = "This button features a puppet made by Scout Tran.", Price = 2.00m, CategoryId = 2, ImageId = 9, IsActive = true, DateAdded = DateTime.UtcNow };
    context.Products.Add(boyButton); context.SaveChanges();
    context.ProductLocations.Add(new ProductLocation { ProductId = boyButton.Id, LocationId = hubId, Quantity = 100 });
    context.SaveChanges();

    // --- PRODUCT 10: STICKERS ---
    var stickers = new Product { Name = "Die Cut Puppet Stickers (Pack of 4)", Description = "High-quality vinyl stickers featuring our favorite puppet characters.", Price = 8.00m, CategoryId = 2, ImageId = 10, IsActive = true };
    context.Products.Add(stickers); context.SaveChanges();
    context.ProductLocations.Add(new ProductLocation { ProductId = stickers.Id, LocationId = hubId, Quantity = 200 });
    context.SaveChanges();

    // --- PRODUCT 11: BINOCULARS ---
    var binoculars = new Product { Name = "DIY Toy Binoculars", Description = "A fun, interactive kit for young puppeteers to explore the festival.", Price = 5.00m, CategoryId = 2, ImageId = 11, IsActive = true };
    context.Products.Add(binoculars); context.SaveChanges();
    context.ProductLocations.Add(new ProductLocation { ProductId = binoculars.Id, LocationId = hubId, Quantity = 40 });
    context.SaveChanges();

    // --- PRODUCT 12: NOTECARDS ---
    var notecards = new Product { Name = "Puppet Fest Notecards (Pack of 8)", Description = "A set of 8 assorted Puppet Fest notecards and envelopes.", Price = 15.00m, CategoryId = 2, ImageId = 14, IsActive = true, DateAdded = DateTime.UtcNow };
    context.Products.Add(notecards); context.SaveChanges();
    context.ProductLocations.Add(new ProductLocation { ProductId = notecards.Id, LocationId = hubId, Quantity = 100 });
    context.SaveChanges();

    // --- PRODUCT 13: BUTTON 2023 ---
    var button2023 = new Product { Name = "Puppet Head Button 2023", Description = "Official 2023 Festival souvenir button.", Price = 2.00m, CategoryId = 2, ImageId = 17, IsActive = true };
    context.Products.Add(button2023); context.SaveChanges();
    context.ProductLocations.Add(new ProductLocation { ProductId = button2023.Id, LocationId = hubId, Quantity = 50 });
    context.SaveChanges();

    // --- PRODUCT 14: BUTTON 2024 ---
    var button2024 = new Product { Name = "Puppet Head Button 2024", Description = "Official 2024 Festival souvenir button.", Price = 2.00m, CategoryId = 2, ImageId = 18, IsActive = true };
    context.Products.Add(button2024); context.SaveChanges();
    context.ProductLocations.Add(new ProductLocation { ProductId = button2024.Id, LocationId = hubId, Quantity = 75 });
    context.SaveChanges();

    // --- PRODUCT 15: SHADOW PUPPET ---
    var harePuppet = new Product { Name = "Shadow Puppet: The Hare", Description = "A laser-cut birch wood shadow puppet. Ready for your next performance!", Price = 15.00m, CategoryId = 2, ImageId = 19, IsActive = true };
    context.Products.Add(harePuppet); context.SaveChanges();
    context.ProductLocations.Add(new ProductLocation { ProductId = harePuppet.Id, LocationId = hubId, Quantity = 25 });
    context.SaveChanges();
}
}
}


    #endregion