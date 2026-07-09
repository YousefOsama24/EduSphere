using EduSphere.Services.Interfaces;
using System.Net;
using System.Net.Mail;

namespace EduSphere.Services.Implementations
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendConfirmationEmailAsync(
            string email,
            string fullName,
            string confirmationLink)
        {
            string subject = "Verify Your EduSphere Account";

            string body = $@"
<!DOCTYPE html>

<html>

<head>

<meta charset='UTF-8'>

</head>

<body style='font-family:Arial;background:#f5f5f5;padding:30px;'>

<div style='max-width:650px;background:white;margin:auto;padding:40px;border-radius:12px;'>

<h2 style='color:#0d6efd;'>Welcome to EduSphere 🎓</h2>

<p>Hello <b>{fullName}</b>,</p>

<p>
Thank you for creating your account.
</p>

<p>
Please click the button below to verify your email address.
</p>

<br/>

<a
href='{confirmationLink}'

style='background:#0d6efd;
padding:14px 30px;
color:white;
text-decoration:none;
border-radius:8px;'>

Verify Email

</a>

<br/><br/>

<p>
If you didn't create this account, you can safely ignore this email.
</p>

<hr/>

<small>

EduSphere Educational Management Platform

</small>

</div>

</body>

</html>";

            await SendCustomEmailAsync(
                email,
                subject,
                body);
        }

        public async Task SendResetPasswordEmailAsync(
            string email,
            string fullName,
            string resetLink)
        {
            string subject = "Reset Your EduSphere Password";

            string body = $@"
<!DOCTYPE html>

<html>

<head>

<meta charset='UTF-8'>

</head>

<body style='font-family:Arial;background:#f5f5f5;padding:30px;'>

<div style='max-width:650px;background:white;margin:auto;padding:40px;border-radius:12px;'>

<h2 style='color:#dc3545;'>Password Reset</h2>

<p>Hello <b>{fullName}</b>,</p>

<p>
We received a request to reset your password.
</p>

<p>
Click the button below.
</p>

<br/>

<a
href='{resetLink}'

style='background:#dc3545;
padding:14px 30px;
color:white;
text-decoration:none;
border-radius:8px;'>

Reset Password

</a>

<br/><br/>

<p>
If you didn't request this, ignore this email.
</p>

<hr/>

<small>

EduSphere Educational Management Platform

</small>

</div>

</body>

</html>";

            await SendCustomEmailAsync(
                email,
                subject,
                body);
        }

        public async Task SendCustomEmailAsync(
            string email,
            string subject,
            string htmlBody)
        {
            var senderEmail =
                _configuration["EmailSettings:Email"];

            var senderPassword =
                _configuration["EmailSettings:Password"];

            using var smtp = new SmtpClient(
                "smtp.gmail.com",
                587);

            smtp.EnableSsl = true;

            smtp.UseDefaultCredentials = false;

            smtp.Credentials =
                new NetworkCredential(
                    senderEmail,
                    senderPassword);

            MailMessage message = new();

            message.From =
                new MailAddress(senderEmail!);

            message.To.Add(email);

            message.Subject = subject;

            message.Body = htmlBody;

            message.IsBodyHtml = true;

            await smtp.SendMailAsync(message);
        }
    }
}