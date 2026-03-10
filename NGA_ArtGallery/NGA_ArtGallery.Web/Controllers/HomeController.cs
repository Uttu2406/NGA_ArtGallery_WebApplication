using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NGA_ArtGallery.Data.Context;
using NGA_ArtGallery.Web.Models;
using System.Diagnostics;

namespace NGA_ArtGallery.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var artworks = await _context.Artworks
                .Include(a => a.Artist)
                .ToListAsync();

            return View(artworks);
        }

        // --- NEW DETAILS ACTION ---
        public async Task<IActionResult> Details(int id)
        {
            // Fetch the specific artwork and include Artist details for the Bio
            var artwork = await _context.Artworks
                .Include(a => a.Artist)
                .FirstOrDefaultAsync(m => m.ArtworkID == id);

            if (artwork == null)
            {
                return NotFound();
            }

            return View(artwork);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}