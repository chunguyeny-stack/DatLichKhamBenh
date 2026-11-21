namespace DatLichKhamBenh.Services.Interfaces
{
    public interface IEmailService
    {
        Task<bool> SendAsync(string toEmail, string subject, string body);
    }
}
