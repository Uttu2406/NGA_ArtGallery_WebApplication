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

        // Updated Index: Accepts an optional artistId for Admin override
        public async Task<IActionResult> Index(int? artistId)
        {
            Artist? artist;

            if (User.IsInRole("Admin") && artistId.HasValue)
            {
                artist = await _context.Artists.Include(a => a.Artworks).FirstOrDefaultAsync(a => a.ArtistID == artistId.Value);
            }
            else
            {
                var userId = int.Parse(_userManager.GetUserId(User)!);
                artist = await _context.Artists.Include(a => a.Artworks).FirstOrDefaultAsync(a => a.UserID == userId);
            }

            if (artist == null)
            {
                if (User.IsInRole("Admin")) return NotFound(); // Admin shouldn't auto-create profiles for themselves

                var userId = int.Parse(_userManager.GetUserId(User)!);
                artist = new Artist { UserID = userId, Name = User.Identity?.Name ?? "New Artist" };
                _context.Artists.Add(artist);
                await _context.SaveChangesAsync();
            }

            return View(artist);
        }

        // Updated EditProfile: Accepts optional id for Admin to edit others
        [HttpGet]
        public async Task<IActionResult> EditProfile(int? id)
        {
            Artist? artist;
            if (User.IsInRole("Admin") && id.HasValue)
            {
                // Admin is editing a profile via User ID
                artist = await _context.Artists.FirstOrDefaultAsync(a => a.UserID == id.Value);
            }
            else
            {
                var userId = int.Parse(_userManager.GetUserId(User)!);
                artist = await _context.Artists.FirstOrDefaultAsync(a => a.UserID == userId);
            }

            if (artist == null) return NotFound();
            return View(artist);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(Artist updatedArtist)
        {
            var artistInDb = await _context.Artists.FirstOrDefaultAsync(a => a.ArtistID == updatedArtist.ArtistID);
            if (artistInDb == null) return NotFound();

            // Security: Only Admin or the owner can edit
            var userId = int.Parse(_userManager.GetUserId(User)!);
            if (!User.IsInRole("Admin") && artistInDb.UserID != userId) return Unauthorized();

            artistInDb.Name = updatedArtist.Name;
            artistInDb.Biography = updatedArtist.Biography;
            artistInDb.BirthDate = updatedArtist.BirthDate;
            artistInDb.Nationality = updatedArtist.Nationality;
            artistInDb.Website = updatedArtist.Website;
            artistInDb.ContactInformation = updatedArtist.ContactInformation;

            if (!ModelState.IsValid) return View(updatedArtist);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index), new { artistId = artistInDb.ArtistID });
        }

        [HttpGet]
        public async Task<IActionResult> EditArtwork(int id)
        {
            var artwork = await _context.Artworks.FindAsync(id);
            if (artwork == null) return NotFound();

            var userId = int.Parse(_userManager.GetUserId(User)!);
            var artist = await _context.Artists.FirstOrDefaultAsync(a => a.UserID == userId);

            // Allow if Admin OR owner
            if (!User.IsInRole("Admin") && artwork.ArtistID != artist?.ArtistID) return Unauthorized();

            return View(artwork);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditArtwork(int id, Artwork updatedArtwork, IFormFile? imageFile)
        {
            if (id != updatedArtwork.ArtworkID) return BadRequest();

            var artworkInDb = await _context.Artworks.AsNoTracking().FirstOrDefaultAsync(a => a.ArtworkID == id);
            if (artworkInDb == null) return NotFound();

            // Security check
            var userId = int.Parse(_userManager.GetUserId(User)!);
            var artist = await _context.Artists.FirstOrDefaultAsync(a => a.UserID == userId);
            if (!User.IsInRole("Admin") && artworkInDb.ArtistID != artist?.ArtistID) return Unauthorized();

            updatedArtwork.ArtistID = artworkInDb.ArtistID;

            if (imageFile != null && imageFile.Length > 0)
            {
                if (!string.IsNullOrEmpty(artworkInDb.ImageURL))
                {
                    var oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", artworkInDb.ImageURL.TrimStart('/'));
                    if (System.IO.File.Exists(oldImagePath)) System.IO.File.Delete(oldImagePath);
                }

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/artworks", fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }
                updatedArtwork.ImageURL = "/images/artworks/" + fileName;
            }
            else
            {
                updatedArtwork.ImageURL = artworkInDb.ImageURL;
            }

            if (!ModelState.IsValid) return View(updatedArtwork);

            _context.Update(updatedArtwork);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index), new { artistId = updatedArtwork.ArtistID });
        }

        [HttpGet]
        public IActionResult Upload() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(Artwork artwork, IFormFile imageFile)
        {
            if (imageFile == null || imageFile.Length == 0)
            {
                ModelState.AddModelError("imageFile", "Please select an image.");
            }
            else
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                var extension = Path.GetExtension(imageFile.FileName).ToLower();
                if (!allowedExtensions.Contains(extension))
                    ModelState.AddModelError("imageFile", "Invalid image type.");

                if (imageFile.Length > 5 * 1024 * 1024)
                    ModelState.AddModelError("imageFile", "Image size exceeds 5MB.");
            }

            if (!ModelState.IsValid) return View(artwork);

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/artworks", fileName);
            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(stream);
            }

            artwork.ImageURL = "/images/artworks/" + fileName;

            var userId = int.Parse(_userManager.GetUserId(User)!);
            var artist = await _context.Artists.FirstOrDefaultAsync(a => a.UserID == userId);
            artwork.ArtistID = artist!.ArtistID;

            _context.Artworks.Add(artwork);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var artwork = await _context.Artworks.FindAsync(id);
            if (artwork == null) return NotFound();

            // Security: Admin can delete anything, Artists only their own
            var userId = int.Parse(_userManager.GetUserId(User)!);
            var artist = await _context.Artists.FirstOrDefaultAsync(a => a.UserID == userId);
            if (!User.IsInRole("Admin") && artwork.ArtistID != artist?.ArtistID) return Unauthorized();

            if (!string.IsNullOrEmpty(artwork.ImageURL))
            {
                var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", artwork.ImageURL.TrimStart('/'));
                if (System.IO.File.Exists(imagePath)) System.IO.File.Delete(imagePath);
            }

            _context.Artworks.Remove(artwork);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { artistId = artwork.ArtistID });
        }
    }
}