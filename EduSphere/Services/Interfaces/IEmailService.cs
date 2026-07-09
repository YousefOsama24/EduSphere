namespace EduSphere.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendConfirmationEmailAsync(
            string email,
            string fullName,
            string confirmationLink);

        Task SendResetPasswordEmailAsync(
            string email,
            string fullName,
            string resetLink);

        Task SendCustomEmailAsync(
            string email,
            string subject,
            string htmlBody);
    }
}