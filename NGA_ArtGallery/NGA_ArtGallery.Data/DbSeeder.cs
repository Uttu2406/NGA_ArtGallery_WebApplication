using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

public static class DbSeeder
{
    public static async Task SeedRolesAndAdminAsync(IServiceProvider service)
    {
        // 1. Get the Managers with the <int> and <IdentityRole<int>> types
        var roleManager = service.GetRequiredService<RoleManager<IdentityRole<int>>>();
        var userManager = service.GetRequiredService<UserManager<IdentityUser<int>>>();

        // 2. Define the Roles
        string[] roles = { "Admin", "Artist", "User" };
        foreach (var roleName in roles)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                // CRITICAL: Must use IdentityRole<int> here
                await roleManager.CreateAsync(new IdentityRole<int>(roleName));
            }
        }

        // 3. Define your Admin List
        var adminEmails = new List<string> {
            "bhandari.krish.kb@gmail.com",
            "admin@gmail.com"
        };

        foreach (var email in adminEmails)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                // CRITICAL: Must use IdentityUser<int> here
                var newAdmin = new IdentityUser<int>
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true
                };

                // Use a secure temporary password
                var result = await userManager.CreateAsync(newAdmin, "Admin123!");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(newAdmin, "Admin");
                }
            }
        }
    }
}