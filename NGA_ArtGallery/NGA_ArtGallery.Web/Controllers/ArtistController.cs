using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NGA_ArtGallery.Data.Context;
using NGA_ArtGallery.Data.Entities; // Adjust based on your actual namespace
// Add your DbContext namespace here!

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

        // 1. The "Dashboard" for the Artist to see their work
        public async Task<IActionResult> Index()
        {
            var userId = int.Parse(_userManager.GetUserId(User));

            // Find this user's Artist profile
            var artist = await _context.Artists
                .Include(a => a.Artworks)
                .FirstOrDefaultAsync(a => a.UserID == userId);

            if (artist == null)
            {
                return NotFound("Artist profile not found. Please contact Admin.");
            }

            return View(artist.Artworks);
        }

        // 2. GET: Show the Upload Form
        [HttpGet]
        public IActionResult Upload()
        {
            return View();
        }

        // 3. Post 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(Artwork artwork, IFormFile imageFile)
        {
            if (imageFile != null && imageFile.Length > 0)
            {
                // 1. Create a unique filename so people don't overwrite each other
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);

                // 2. Define the path to save the file
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/artworks", fileName);

                // 3. Save the file to the folder
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }

                // 4. Set the ImageURL property for the database
                artwork.ImageURL = "/images/artworks/" + fileName;

                // 5. Link the artwork to the logged-in Artist
                var userId = int.Parse(_userManager.GetUserId(User));
                var artist = await _context.Artists.FirstOrDefaultAsync(a => a.UserID == userId);

                if (artist != null)
                {
                    artwork.ArtistID = artist.ArtistID;
                    _context.Artworks.Add(artwork);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }

            return View(artwork);
        }

        // 4. Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var artwork = await _context.Artworks.FindAsync(id);
            if (artwork == null) return NotFound();

            // 1. Delete the physical file from the server
            var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", artwork.ImageURL.TrimStart('/'));
            if (System.IO.File.Exists(imagePath))
            {
                System.IO.File.Delete(imagePath);
            }

            // 2. Delete the record from the database
            _context.Artworks.Remove(artwork);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}