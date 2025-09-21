using System.ComponentModel.DataAnnotations;

namespace Url_Shortener.Models
{
    public class ShortenedUrl
    {
        public int Id { get; set; }
        
        [Required]
        public string OriginalUrl { get; set; } = string.Empty;
        
        [Required]
        public string ShortCode { get; set; } = string.Empty;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? ExpiresAt { get; set; }
        
        public int ClickCount { get; set; } = 0;
        
        public string ShortUrl => $"https://short.link/{ShortCode}";
    }
}

