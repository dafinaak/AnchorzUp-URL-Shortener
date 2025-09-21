using Url_Shortener.Models;

namespace Url_Shortener.Services
{
    public interface IUrlShorteningService
    {
        Task<ShortenedUrl> ShortenUrlAsync(string originalUrl, DateTime? expiresAt = null);
        Task<ShortenedUrl?> GetOriginalUrlAsync(string shortCode);
        Task<bool> DeleteUrlAsync(string shortCode);
        Task IncrementClickCountAsync(string shortCode);
        Task<List<ShortenedUrl>> GetAllUrlsAsync();
        Task<bool> IsUrlExpiredAsync(ShortenedUrl url);
    }
}

