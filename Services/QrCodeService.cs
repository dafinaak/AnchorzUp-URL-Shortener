using QRCoder;
using System.Drawing;
using System.Drawing.Imaging;

namespace Url_Shortener.Services
{
    public class QrCodeService : IQrCodeService
    {
        public string GenerateQrCode(string url, int size = 200)
        {
            var qrCodeBytes = GenerateQrCodeBytes(url, size);
            return Convert.ToBase64String(qrCodeBytes);
        }

        public byte[] GenerateQrCodeBytes(string url, int size = 200)
        {
            using var qrGenerator = new QRCodeGenerator();
            using var qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new PngByteQRCode(qrCodeData);
            return qrCode.GetGraphic(20);
        }
    }
}