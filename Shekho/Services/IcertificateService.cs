namespace Shekho.Services
{
    public interface ICertificateService
    {
        byte[] GenerateCertificate(string studentName, string courseTitle, DateTime completionDate);
    }
}
