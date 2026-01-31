using MakeupStudio_Api.Data;
using MakeupStudio_Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MakeupStudio_Api.Seed;

//this class is responsible for inserting initial data into the database
//it runs when the application starts and makes sure required data exists
public static class DataSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        //create a scoped service provider so we can resolve db and identity services
        using var scope = services.CreateScope();

        //get database context
        var db =
            scope.ServiceProvider
                .GetRequiredService<ApplicationDbContext>();

        //get identity user manager (handles users)
        var userManager =
            scope.ServiceProvider
                .GetRequiredService<UserManager<ApplicationUser>>();

        //get identity role manager (handles roles)
        var roleManager =
            scope.ServiceProvider
                .GetRequiredService<RoleManager<IdentityRole>>();

        //make sure database exists and migrations are applied
        await db.Database.MigrateAsync();

        //=========================
        //1.roles
        //=========================

        //ensure both admin and client roles exist
        await EnsureRole(roleManager, "Admin");
        await EnsureRole(roleManager, "Client");

        //=========================
        //2.admin user
        //=========================

        //default admin credentials (used only for development/demo)
        var adminEmail = "admin@makeupstudio.com";
        var adminPassword = "Admin123!";

        //check if admin user already exists
        var admin = await userManager.FindByEmailAsync(adminEmail);

        if (admin is null)
        {
            //create admin user if it does not exist
            admin = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail
            };

            var created = await userManager.CreateAsync(
                admin,
                adminPassword
            );

            //assign admin role after successful creation
            if (created.Succeeded)
            {
                await userManager.AddToRoleAsync(admin, "Admin");
            }
        }
        else
        {
            //if admin exists but is missing the admin role, add it
            var isAdmin = await userManager.IsInRoleAsync(
                admin,
                "Admin"
            );

            if (!isAdmin)
            {
                await userManager.AddToRoleAsync(admin, "Admin");
            }
        }

        //=========================
        //3.default services
        //=========================

        //add basic services only if they do not already exist
        await AddServiceIfMissing(db, "Bridal Makeup", 120, 3500);
        await AddServiceIfMissing(db, "Evening Makeup", 90, 2500);
        await AddServiceIfMissing(db, "Soft Makeup", 60, 1500);

        //save any changes made during seeding
        await db.SaveChangesAsync();
    }

    //creates a role only if it does not already exist
    private static async Task EnsureRole(
        RoleManager<IdentityRole> roleManager,
        string roleName
    )
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(
                new IdentityRole(roleName)
            );
        }
    }

    //adds a service only if it is not already stored in the database
    //prevents duplicate services on every app restart
    private static async Task AddServiceIfMissing(
        ApplicationDbContext db,
        string name,
        int durationMinutes,
        decimal price
    )
    {
        var exists = await db.Services
            .AnyAsync(s => s.Name == name);

        if (!exists)
        {
            db.Services.Add(new Service
            {
                Name = name,
                DurationMinutes = durationMinutes,
                Price = price
            });
        }
    }
}
