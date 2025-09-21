using Microsoft.EntityFrameworkCore;
using Url_Shortener.Data;
using Url_Shortener.Models;

namespace Url_Shortener.Services
{
    public class UrlShorteningService : IUrlShorteningService
    {
        private readonly UrlShortenerContext _context;
        private readonly Random _random = new();

        public UrlShorteningService(UrlShortenerContext context)
        {
            _context = context;
        }

        public async Task<ShortenedUrl> ShortenUrlAsync(string originalUrl, DateTime? expiresAt = null)
        {
            if (!Uri.TryCreate(originalUrl, UriKind.Absolute, out var uri))
            {
                throw new ArgumentException("Invalid URL format");
            }

      
            string shortCode;
            do
            {
                shortCode = GenerateShortCode();
            } while (await _context.ShortenedUrls.AnyAsync(u => u.ShortCode == shortCode));

            var shortenedUrl = new ShortenedUrl
            {
                OriginalUrl = originalUrl,
                ShortCode = shortCode,
                ExpiresAt = expiresAt
            };

            _context.ShortenedUrls.Add(shortenedUrl);
            await _context.SaveChangesAsync();

            return shortenedUrl;
        }

        public async Task<ShortenedUrl?> GetOriginalUrlAsync(string shortCode)
        {
            var url = await _context.ShortenedUrls
                .FirstOrDefaultAsync(u => u.ShortCode == shortCode);

            if (url != null && IsUrlExpired(url))
            {
                return null; 
            }

            return url;
        }

        public async Task<bool> DeleteUrlAsync(string shortCode)
        {
            var url = await _context.ShortenedUrls
                .FirstOrDefaultAsync(u => u.ShortCode == shortCode);

            if (url != null)
            {
                _context.ShortenedUrls.Remove(url);
                await _context.SaveChangesAsync();
                return true;
            }

            return false;
        }

        public async Task IncrementClickCountAsync(string shortCode)
        {
            var url = await _context.ShortenedUrls
                .FirstOrDefaultAsync(u => u.ShortCode == shortCode);

            if (url != null)
            {
                url.ClickCount++;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<ShortenedUrl>> GetAllUrlsAsync()
        {
            return await _context.ShortenedUrls
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();
        }

        public Task<bool> IsUrlExpiredAsync(ShortenedUrl url)
        {
            return Task.FromResult(IsUrlExpired(url));
        }

        private string GenerateShortCode()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            return new string(Enumerable.Repeat(chars, 7)
                .Select(s => s[_random.Next(s.Length)]).ToArray());
        }

        private static bool IsUrlExpired(ShortenedUrl url)
        {
            return url.ExpiresAt.HasValue && DateTime.UtcNow > url.ExpiresAt.Value;
        }
    }
}

