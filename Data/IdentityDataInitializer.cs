using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using myapp.Models;
using System.Threading.Tasks;

namespace myapp.Data
{
    public static class IdentityDataInitializer
    {
        public static async Task SeedData(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext context)
        {
            // *** THIS IS THE FIX ***
            // Ensures that the database is created and all migrations are applied.
            // This must run BEFORE any data seeding attempts.
            await context.Database.MigrateAsync();

            await SeedRoles(roleManager);
            await SeedUsers(userManager, context);
        }

        private static async Task SeedRoles(RoleManager<IdentityRole> roleManager)
        {
            // Seed Roles: Admin and User
            if (!await roleManager.RoleExistsAsync("Admin"))
            {
                await roleManager.CreateAsync(new IdentityRole("Admin"));
            }
            if (!await roleManager.RoleExistsAsync("User"))
            {
                await roleManager.CreateAsync(new IdentityRole("User"));
            }
        }

        private static async Task SeedUsers(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            // Check if the admin user already exists
            if (await userManager.FindByNameAsync("ituser@example.com") == null)
            {
                // Create the Admin User
                var adminUser = new ApplicationUser
                {
                    UserName = "ituser@example.com",
                    Email = "ituser@example.com",
                    FirstName = "Admin",
                    LastName = "User",
                    Department = "IT",
                    Section = "Support",
                    Plant = "Headquarters",
                    IsActive = true,
                    IsIT = true,
                    EmailConfirmed = true // Bypassing email confirmation for the seed user
                };

                var result = await userManager.CreateAsync(adminUser, "Password123!");

                if (result.Succeeded)
                {
                    // Assign the 'Admin' role
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }
        }
    }
}
