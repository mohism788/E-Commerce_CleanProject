namespace E_Commerce.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body, string replyTo = null);
    }
}
