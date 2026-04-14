using QRCoder;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace QuranCentersSystem.Services
{
    /// <summary>
    /// خدمة رموز QR - توليد رموز QR فريدة لكل طالب واستخدامها في تسجيل الحضور
    /// يتم توليد رمز مشفر فريد لكل طالب ثم تحويله لصورة QR
    /// </summary>
    public class QrCodeService
    {
        /// <summary>
        /// توليد رمز QR فريد ومشفر لطالب معين
        /// يتكون الرمز من: معرف الطالب + طابع زمني + بصمة عشوائية
        /// </summary>
        /// <param name="studentId">معرف الطالب</param>
        /// <returns>رمز QR مشفر</returns>
        public string GenerateQrToken(int studentId)
        {
            // إنشاء رمز فريد: طالب + وقت + عشوائي
            var rawToken = $"ITQAN-STD-{studentId}-{DateTime.UtcNow.Ticks}-{Guid.NewGuid():N}";

            // تشفير الرمز بـ SHA256 للأمان
            using var sha256 = SHA256.Create();
            var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawToken));
            var hashString = BitConverter.ToString(hashBytes).Replace("-", "").Substring(0, 32);

            return $"ITQ{studentId:D4}{hashString}";
        }

        /// <summary>
        /// توليد صورة QR Code بصيغة PNG من رمز نصي
        /// </summary>
        /// <param name="token">الرمز النصي المراد تحويله لـ QR</param>
        /// <returns>مصفوفة بايتات تمثل صورة PNG</returns>
        public byte[] GenerateQrCodeImage(string token)
        {
            using var qrGenerator = new QRCodeGenerator();
            var qrCodeData = qrGenerator.CreateQrCode(token, QRCodeGenerator.ECCLevel.M);
            using var qrCode = new PngByteQRCode(qrCodeData);
            return qrCode.GetGraphic(10);
        }

        /// <summary>
        /// التحقق من صحة رمز QR (التحقق من التنسيق الأساسي)
        /// </summary>
        /// <param name="token">الرمز المراد التحقق منه</param>
        /// <returns>صحيح إذا كان التنسيق صالحاً</returns>
        public bool ValidateQrTokenFormat(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return false;

            // التحقق من أن الرمز يبدأ بـ ITQ ويحتوي على الحد الأدنى من الطول
            return token.StartsWith("ITQ") && token.Length >= 36;
        }
    }
}
