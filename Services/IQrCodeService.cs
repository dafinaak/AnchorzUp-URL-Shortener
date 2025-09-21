using System.Drawing;

namespace Url_Shortener.Services
{
    public interface IQrCodeService
    {
        /// <summary>
        /// Generates a QR code as a base64 encoded PNG image for the given URL
        /// </summary>
        /// <param name="url">The URL to encode in the QR code</param>
        /// <param name="size">The size of the QR code in pixels (default: 200)</param>
        /// <returns>Base64 encoded PNG image string</returns>
        string GenerateQrCode(string url, int size = 200);
        
        /// <summary>
        /// Generates a QR code as a byte array for the given URL
        /// </summary>
        /// <param name="url">The URL to encode in the QR code</param>
        /// <param name="size">The size of the QR code in pixels (default: 200)</param>
        /// <returns>Byte array representing the PNG image</returns>
        byte[] GenerateQrCodeBytes(string url, int size = 200);
    }
}

