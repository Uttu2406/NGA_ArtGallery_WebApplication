using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Threading.Tasks;

public static class DbSeeder
{
    public static async Task SeedRolesAndAdminAsync(IServiceProvider service)
    {
        var roleManager = service.GetRequiredService<RoleManager<IdentityRole<int>>>();
        var userManager = service.GetRequiredService<UserManager<IdentityUser<int>>>();

        // 1. Define the Roles
        string[] roles = { "Admin", "Artist", "User" };
        foreach (var roleName in roles)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole<int>(roleName));
            }
        }

        // 2. Define the User Lists by Role
        var admins = new List<string> { 
            "bhandari.krish.kb@gmail.com", 
            "admin@gmail.com" 
        };

        var artists = new List<string> { 
            "test_artist@gmail.com" 
        };

        var normalUsers = new List<string> { 
            "test_user@gmail.com" 
        };

        // 3. Seed Admins
        foreach (var email in admins)
        {
            await CreateUserWithRole(userManager, email, "Admin123!", "Admin");
        }

        // 4. Seed Artists
        foreach (var email in artists)
        {
            await CreateUserWithRole(userManager, email, "Artist123!", "Artist");
        }

        // 5. Seed Normal Users
        foreach (var email in normalUsers)
        {
            await CreateUserWithRole(userManager, email, "User123!", "User");
        }
    }

    // Helper method to keep the code clean and handle multiple users easily
    private static async Task CreateUserWithRole(UserManager<IdentityUser<int>> userManager, string email, string password, string role)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user == null)
        {
            var newUser = new IdentityUser<int>
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(newUser, password);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(newUser, role);
            }
        }
    }
}
