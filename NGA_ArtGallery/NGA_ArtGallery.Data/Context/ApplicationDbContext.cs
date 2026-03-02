using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NGA_ArtGallery.Data.Entities;

namespace NGA_ArtGallery.Data.Context
{
    // Explicitly tell Identity to use IdentityUser<int>, IdentityRole<int>, and int for the PK
    public class ApplicationDbContext : IdentityDbContext<IdentityUser<int>, IdentityRole<int>, int>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Artist> Artists { get; set; }
        public DbSet<Artwork> Artworks { get; set; }
        public DbSet<Gallery> Galleries { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            // This MUST be called first so Identity tables are configured
            base.OnModelCreating(builder);

            // Sets your custom schema
            builder.HasDefaultSchema("Gallery");
        }
    }
}