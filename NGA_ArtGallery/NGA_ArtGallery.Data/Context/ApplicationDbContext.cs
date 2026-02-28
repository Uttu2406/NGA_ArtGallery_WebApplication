using Microsoft.AspNetCore.Identity.EntityFrameworkCore; // Must have .Identity
using Microsoft.EntityFrameworkCore;
using NGA_ArtGallery.Data.Entities;

namespace NGA_ArtGallery.Data.Context
{
    // Must inherit from IdentityDbContext to handle logins
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Artist> Artists { get; set; }
        public DbSet<Artwork> Artworks { get; set; }
        public DbSet<Gallery> Galleries { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.HasDefaultSchema("Gallery");
        }
    }
}