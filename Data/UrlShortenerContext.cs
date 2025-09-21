using Microsoft.EntityFrameworkCore;
using Url_Shortener.Models;

namespace Url_Shortener.Data
{
    public class UrlShortenerContext : DbContext
    {
        public UrlShortenerContext(DbContextOptions<UrlShortenerContext> options) : base(options)
        {
        }

        public DbSet<ShortenedUrl> ShortenedUrls { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ShortenedUrl>()
                .HasIndex(u => u.ShortCode)
                .IsUnique();

            base.OnModelCreating(modelBuilder);
        }
    }
}


