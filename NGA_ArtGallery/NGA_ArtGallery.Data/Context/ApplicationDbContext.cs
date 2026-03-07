using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NGA_ArtGallery.Data.Entities;

namespace NGA_ArtGallery.Data.Context
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser<int>, IdentityRole<int>, int>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Artist> Artists { get; set; }
        public DbSet<Artwork> Artworks { get; set; }
        public DbSet<Gallery> Galleries { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            // 1. Initialize Identity defaults
            base.OnModelCreating(builder);

            // 2. Keep Identity tables in dbo (the default)
            builder.HasDefaultSchema("dbo");

            // 3. Move only your custom Gallery models to the "Gallery" schema
            builder.Entity<Artist>().ToTable("Artists", "Gallery");
            builder.Entity<Artwork>().ToTable("Artworks", "Gallery");
            builder.Entity<Gallery>().ToTable("Galleries", "Gallery");
        }
    }
}