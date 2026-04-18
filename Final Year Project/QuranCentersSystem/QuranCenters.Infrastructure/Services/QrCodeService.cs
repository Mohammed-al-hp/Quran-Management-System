using QRCoder;
using QuranCenters.Application.Interfaces;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace QuranCenters.Infrastructure.Services
{
    public class QrCodeService : IQrCodeService
    {
        public string GenerateQrToken(int studentId)
        {
            var rawToken = $"ITQAN-STD-{studentId}-{DateTime.UtcNow.Ticks}-{Guid.NewGuid():N}";

            using var sha256 = SHA256.Create();
            var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawToken));
            var hashString = BitConverter.ToString(hashBytes).Replace("-", "").Substring(0, 32);

            return $"ITQ{studentId:D4}{hashString}";
        }

        public byte[] GenerateQrCodeImage(string token)
        {
            using var qrGenerator = new QRCodeGenerator();
            var qrCodeData = qrGenerator.CreateQrCode(token, QRCodeGenerator.ECCLevel.M);
            using var qrCode = new PngByteQRCode(qrCodeData);
            return qrCode.GetGraphic(10);
        }

        public bool ValidateQrTokenFormat(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return false;

            return token.StartsWith("ITQ") && token.Length >= 36;
        }
    }
}
