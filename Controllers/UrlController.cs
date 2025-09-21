using Microsoft.AspNetCore.Mvc;
using Url_Shortener.Models;
using Url_Shortener.Services;

namespace Url_Shortener.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UrlController : ControllerBase
    {
        private readonly IUrlShorteningService _urlShorteningService;
        private readonly IQrCodeService _qrCodeService;
        private readonly ILogger<UrlController> _logger;

        public UrlController(IUrlShorteningService urlShorteningService, IQrCodeService qrCodeService, ILogger<UrlController> logger)
        {
            _urlShorteningService = urlShorteningService;
            _qrCodeService = qrCodeService;
            _logger = logger;
        }

        [HttpPost("shorten")]
        public async Task<IActionResult> ShortenUrl([FromBody] ShortenUrlRequest request)
        {
            try
            {
                DateTime? expiresAt = null;
                
                if (request.ExpirationMinutes.HasValue)
                {
                    expiresAt = DateTime.UtcNow.AddMinutes(request.ExpirationMinutes.Value);
                }

                var shortenedUrl = await _urlShorteningService.ShortenUrlAsync(request.Url, expiresAt);
                
                return Ok(new
                {
                    shortUrl = shortenedUrl.ShortUrl,
                    shortCode = shortenedUrl.ShortCode,
                    originalUrl = shortenedUrl.OriginalUrl,
                    expiresAt = shortenedUrl.ExpiresAt,
                    clickCount = shortenedUrl.ClickCount
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error shortening URL");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpGet("{shortCode}")]
        public async Task<IActionResult> RedirectUrl(string shortCode)
        {
            try
            {
                var url = await _urlShorteningService.GetOriginalUrlAsync(shortCode);
                
                if (url == null)
                {
                    return NotFound(new { error = "URL not found or expired" });
                }

                await _urlShorteningService.IncrementClickCountAsync(shortCode);
                
                return Redirect(url.OriginalUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error redirecting URL");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllUrls()
        {
            try
            {
                var urls = await _urlShorteningService.GetAllUrlsAsync();
                
                var result = urls.Select(u => new
                {
                    shortUrl = u.ShortUrl,
                    shortCode = u.ShortCode,
                    originalUrl = u.OriginalUrl,
                    createdAt = u.CreatedAt,
                    expiresAt = u.ExpiresAt,
                    clickCount = u.ClickCount,
                    isExpired = u.ExpiresAt.HasValue && DateTime.UtcNow > u.ExpiresAt.Value
                }).ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all URLs");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpDelete("{shortCode}")]
        public async Task<IActionResult> DeleteUrl(string shortCode)
        {
            try
            {
                var deleted = await _urlShorteningService.DeleteUrlAsync(shortCode);
                
                if (!deleted)
                {
                    return NotFound(new { error = "URL not found" });
                }

                return Ok(new { message = "URL deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting URL");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpGet("{shortCode}/qr")]
        public async Task<IActionResult> GetQrCode(string shortCode, [FromQuery] int size = 200)
        {
            try
            {
                var url = await _urlShorteningService.GetOriginalUrlAsync(shortCode);
                
                if (url == null)
                {
                    return NotFound(new { error = "URL not found or expired" });
                }

                if (size < 50 || size > 1000)
                {
                    return BadRequest(new { error = "Size must be between 50 and 1000 pixels" });
                }

                var qrCodeBase64 = _qrCodeService.GenerateQrCode(url.OriginalUrl, size);
                
                return Ok(new
                {
                    qrCode = qrCodeBase64,
                    shortUrl = url.ShortUrl,
                    originalUrl = url.OriginalUrl,
                    size = size
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating QR code");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpGet("{shortCode}/qr/image")]
        public async Task<IActionResult> GetQrCodeImage(string shortCode, [FromQuery] int size = 200)
        {
            try
            {
                var url = await _urlShorteningService.GetOriginalUrlAsync(shortCode);
                
                if (url == null)
                {
                    return NotFound(new { error = "URL not found or expired" });
                }

                if (size < 50 || size > 1000)
                {
                    return BadRequest(new { error = "Size must be between 50 and 1000 pixels" });
                }

                var qrCodeBytes = _qrCodeService.GenerateQrCodeBytes(url.OriginalUrl, size);
                
                return File(qrCodeBytes, "image/png", $"qr-code-{shortCode}.png");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating QR code image");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }
    }

    public class ShortenUrlRequest
    {
        public string Url { get; set; } = string.Empty;
        public int? ExpirationMinutes { get; set; }
    }
}


