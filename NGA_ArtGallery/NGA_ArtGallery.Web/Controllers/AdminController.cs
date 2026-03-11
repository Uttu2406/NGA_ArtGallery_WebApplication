using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NGA_ArtGallery.Data.Context;
using NGA_ArtGallery.Data.Entities;
using NGA_ArtGallery.Web.Models.ViewModels;

namespace NGA_ArtGallery.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<IdentityUser<int>> _userManager;
        private readonly RoleManager<IdentityRole<int>> _roleManager;
        private readonly ApplicationDbContext _context;

        public AdminController(UserManager<IdentityUser<int>> userManager, RoleManager<IdentityRole<int>> roleManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        public IActionResult Index() => View();

        public async Task<IActionResult> ManageUsers()
        {
            var users = await _userManager.Users.ToListAsync();
            var userList = new List<UserManagementViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userList.Add(new UserManagementViewModel
                {
                    Id = user.Id,
                    Email = user.Email ?? "No Email",
                    Role = roles.FirstOrDefault() ?? "User"
                });
            }

            // Grouping the list by Role
            var groupedModel = userList.GroupBy(u => u.Role).ToList();

            return View(groupedModel);
        }

        [HttpGet]
        public async Task<IActionResult> UserDetails(int userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) return NotFound();

            var roles = await _userManager.GetRolesAsync(user);
            var artist = await _context.Artists.FirstOrDefaultAsync(a => a.UserID == userId);

            ViewBag.Role = roles.FirstOrDefault() ?? "User";
            ViewBag.ArtistId = artist?.ArtistID;

            return View(user);
        }

        [HttpGet]
        public async Task<IActionResult> EditRole(int userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) return NotFound();

            var allRoles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
            var userRoles = await _userManager.GetRolesAsync(user);

            ViewBag.UserId = user.Id;
            ViewBag.UserEmail = user.Email;
            ViewBag.AllRoles = allRoles;
            ViewBag.CurrentRole = userRoles.FirstOrDefault();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateRole(int userId, string newRole)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) return NotFound();

            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            await _userManager.AddToRoleAsync(user, newRole);

            return RedirectToAction(nameof(ManageUsers));
        }

        [HttpGet]
        public IActionResult CreateUser() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(string email, string password, string role)
        {
            var user = new IdentityUser<int> { UserName = email, Email = email };
            var result = await _userManager.CreateAsync(user, password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, role);
                if (role == "Artist")
                {
                    _context.Artists.Add(new Artist { UserID = user.Id, Name = email });
                    await _context.SaveChangesAsync();
                }
                return RedirectToAction(nameof(ManageUsers));
            }

            foreach (var error in result.Errors) ModelState.AddModelError("", error.Description);
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(int id)
        {
            // 1. Get current logged-in Admin's ID
            var currentUserId = int.Parse(_userManager.GetUserId(User)!);

            // 2. Prevent self-deletion
            if (id == currentUserId)
            {
                TempData["Error"] = "You cannot delete your own account while you are logged in.";
                return RedirectToAction(nameof(ManageUsers));
            }

            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null) return NotFound();

            // 3. Clean up Artist data and images
            var artist = await _context.Artists.Include(a => a.Artworks).FirstOrDefaultAsync(a => a.UserID == id);
            if (artist != null)
            {
                foreach (var art in artist.Artworks)
                {
                    if (!string.IsNullOrEmpty(art.ImageURL))
                    {
                        var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", art.ImageURL.TrimStart('/'));
                        if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
                    }
                }
                _context.Artists.Remove(artist);
            }

            // 4. Delete the User
            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                await _context.SaveChangesAsync();
                TempData["Success"] = "User and associated data deleted successfully.";
            }
            else
            {
                TempData["Error"] = "Failed to delete user.";
            }

            return RedirectToAction(nameof(ManageUsers));
        }

        public async Task<IActionResult> ViewArtistAccount(int userId)
        {
            var artist = await _context.Artists.FirstOrDefaultAsync(a => a.UserID == userId);
            if (artist == null) return NotFound();

            return RedirectToAction("Index", "Artist", new { artistId = artist.ArtistID });
        }
    }
}