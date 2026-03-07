using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NGA_ArtGallery.Data.Context;
using NGA_ArtGallery.Data.Entities;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace NGA_ArtGallery.Web.Controllers
{
    [Authorize(Roles = "Artist")]
    public class ArtistController : Controller
    {
        private readonly UserManager<IdentityUser<int>> _userManager;
        private readonly ApplicationDbContext _context;

        public ArtistController(UserManager<IdentityUser<int>> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        // 1. Dashboard: Shows artworks or creates profile if missing
        public async Task<IActionResult> Index()
        {
            var userIdString = _userManager.GetUserId(User);
            if (userIdString == null) return Challenge();

            var userId = int.Parse(userIdString);

            var artist = await _context.Artists
                .Include(a => a.Artworks)
                .FirstOrDefaultAsync(a => a.UserID == userId);

            // AUTO-CREATE PROFILE: If user has Artist role but no record in Gallery.Artists
            if (artist == null)
            {
                artist = new Artist
                {
                    UserID = userId,
                    Name = User.Identity?.Name ?? "New Artist"
                };
                _context.Artists.Add(artist);
                await _context.SaveChangesAsync();
            }

            return View(artist.Artworks ?? new List<Artwork>());
        }

        // 2. GET: Upload Form
        [HttpGet]
        public IActionResult Upload()
        {
            return View();
        }

        // 3. POST: Handle File Upload and Database entry
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(Artwork artwork, IFormFile imageFile)
        {
            if (imageFile != null && imageFile.Length > 0)
            {
                // Create unique filename
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/artworks", fileName);

                // Ensure directory exists
                Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }

                artwork.ImageURL = "/images/artworks/" + fileName;

                var userId = int.Parse(_userManager.GetUserId(User)!);
                var artist = await _context.Artists.FirstOrDefaultAsync(a => a.UserID == userId);

                // Double check artist exists before linking artwork
                if (artist == null)
                {
                    artist = new Artist { UserID = userId, Name = User.Identity?.Name ?? "New Artist" };
                    _context.Artists.Add(artist);
                    await _context.SaveChangesAsync();
                }

                artwork.ArtistID = artist.ArtistID;
                _context.Artworks.Add(artwork);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(artwork);
        }

        // 4. POST: Delete Artwork and its image file
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var artwork = await _context.Artworks.FindAsync(id);
            if (artwork == null) return NotFound();

            // Delete physical file
            var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", artwork.ImageURL.TrimStart('/'));
            if (System.IO.File.Exists(imagePath))
            {
                System.IO.File.Delete(imagePath);
            }

            _context.Artworks.Remove(artwork);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}