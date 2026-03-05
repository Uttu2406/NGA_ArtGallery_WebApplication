using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NGA_ArtGallery.Web.Models.ViewModels;

namespace NGA_ArtGallery.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<IdentityUser<int>> _userManager;

        // Dependency Injection re, helps to add "tools" (userManager)...so we can use it later???
        public AdminController(UserManager<IdentityUser<int>> userManager)
        {
            _userManager = userManager;
        }


        // AdminController ko constructor. Was here by default hehe
        public IActionResult Index()
        {
            return View();
        }


        // Action method to manage users.
        public async Task<IActionResult> ManageUsers()
        {
            var users = await _userManager.Users.ToListAsync();
            var model = new List<UserManagementViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);

                model.Add(new UserManagementViewModel
                {
                    Id = user.Id,
                    Email = user.Email ?? "No Email",
                    Role = roles.FirstOrDefault() ?? "No Role"
                });
            }

            return View(model);
        }
    }
}
