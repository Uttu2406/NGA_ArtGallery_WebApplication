using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NGA_ArtGallery.Data.Context;
using NGA_ArtGallery.Data.Entities;

namespace NGA_ArtGallery.Web.Controllers
{
    [Authorize(Roles = "Artist, Admin")]
    public class ArtistController : Controller
    {
        private readonly UserManager<IdentityUser<int>> _userManager;
        private readonly ApplicationDbContext _context;

        public ArtistController(UserManager<IdentityUser<int>> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<IActionResult> Index(int? artistId)
        {
            Artist? artist;
            if (User.IsInRole("Admin") && artistId.HasValue)
                artist = await _context.Artists.Include(a => a.Artworks).FirstOrDefaultAsync(a => a.ArtistID == artistId.Value);
            else
            {
                var userId = int.Parse(_userManager.GetUserId(User)!);
                artist = await _context.Artists.Include(a => a.Artworks).FirstOrDefaultAsync(a => a.UserID == userId);
            }

            if (artist == null)
            {
                if (User.IsInRole("Admin")) return NotFound("Artist profile not found.");
                var userId = int.Parse(_userManager.GetUserId(User)!);
                artist = new Artist { UserID = userId, Name = User.Identity?.Name ?? "New Artist" };
                _context.Artists.Add(artist);
                await _context.SaveChangesAsync();
            }
            return View(artist);
        }

        // --- FIXED: EDIT PROFILE GET ---
        [HttpGet]
        public async Task<IActionResult> EditProfile(int? artistId)
        {
            Artist? artist;
            if (User.IsInRole("Admin") && artistId.HasValue)
                artist = await _context.Artists.FirstOrDefaultAsync(a => a.ArtistID == artistId.Value);
            else
            {
                var userId = int.Parse(_userManager.GetUserId(User)!);
                artist = await _context.Artists.FirstOrDefaultAsync(a => a.UserID == userId);
            }
            return artist == null ? NotFound() : View(artist);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(Artist updatedArtist)
        {
            var artistInDb = await _context.Artists.FirstOrDefaultAsync(a => a.ArtistID == updatedArtist.ArtistID);
            if (artistInDb == null) return NotFound();

            if (!User.IsInRole("Admin") && artistInDb.UserID != int.Parse(_userManager.GetUserId(User)!))
                return Unauthorized();

            artistInDb.Name = updatedArtist.Name;
            artistInDb.Biography = updatedArtist.Biography;
            artistInDb.Nationality = updatedArtist.Nationality;
            artistInDb.BirthDate = updatedArtist.BirthDate;
            artistInDb.Website = updatedArtist.Website;
            artistInDb.ContactInformation = updatedArtist.ContactInformation;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index), new { artistId = artistInDb.ArtistID });
        }

        // --- NEW: EDIT ARTWORK ACTIONS (The missing link) ---
        [HttpGet]
        public async Task<IActionResult> EditArtwork(int id)
        {
            var artwork = await _context.Artworks.FindAsync(id);
            if (artwork == null) return NotFound();

            // Authorization check
            if (!User.IsInRole("Admin"))
            {
                var userId = int.Parse(_userManager.GetUserId(User)!);
                var artist = await _context.Artists.FirstOrDefaultAsync(a => a.UserID == userId);
                if (artwork.ArtistID != artist?.ArtistID) return Unauthorized();
            }

            return View(artwork);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditArtwork(Artwork model, IFormFile? imageFile)
        {
            var artwork = await _context.Artworks.FindAsync(model.ArtworkID);
            if (artwork == null) return NotFound();

            artwork.Title = model.Title;
            artwork.Medium = model.Medium;
            artwork.Description = model.Description;
            artwork.CreationDate = model.CreationDate;

            if (imageFile != null && imageFile.Length > 0)
            {
                // Delete old image
                var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", artwork.ImageURL!.TrimStart('/'));
                if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);

                // Save new image
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/artworks", fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                    await imageFile.CopyToAsync(stream);

                artwork.ImageURL = "/images/artworks/" + fileName;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index), new { artistId = artwork.ArtistID });
        }

        [HttpGet]
        public async Task<IActionResult> Upload(int? artistId)
        {
            ViewBag.TargetArtistId = artistId;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(Artwork artwork, IFormFile imageFile, int? targetArtistId)
        {
            Artist? artist;
            if (User.IsInRole("Admin") && targetArtistId.HasValue)
                artist = await _context.Artists.FindAsync(targetArtistId.Value);
            else
                artist = await _context.Artists.FirstOrDefaultAsync(a => a.UserID == int.Parse(_userManager.GetUserId(User)!));

            if (artist == null) return BadRequest("Artist profile required.");

            if (imageFile != null && imageFile.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/artworks", fileName);
                Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
                using (var stream = new FileStream(filePath, FileMode.Create))
                    await imageFile.CopyToAsync(stream);
                artwork.ImageURL = "/images/artworks/" + fileName;
            }

            artwork.ArtistID = artist.ArtistID;
            _context.Artworks.Add(artwork);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { artistId = artist.ArtistID });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var artwork = await _context.Artworks.FindAsync(id);
            if (artwork == null) return NotFound();

            int returnId = artwork.ArtistID;

            if (!string.IsNullOrEmpty(artwork.ImageURL))
            {
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", artwork.ImageURL.TrimStart('/'));
                if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
            }

            _context.Artworks.Remove(artwork);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { artistId = returnId });
        }
    }
}