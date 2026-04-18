namespace QuranCenters.Application.Interfaces
{
    public interface IQrCodeService
    {
        string GenerateQrToken(int studentId);
        byte[] GenerateQrCodeImage(string token);
        bool ValidateQrTokenFormat(string token);
    }
}
