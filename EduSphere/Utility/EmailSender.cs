using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Net;
using System.Net.Mail;

namespace EduSphere.Utility
{
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var client = new SmtpClient("smtp.gmail.com", 587)
            {
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential("mohammedmahfouz890@gmail.com", "cgyb xwar sqre otlj")
            };

            return client.SendMailAsync(
                new MailMessage(from: "mohammedmahfouz890@gmail.com",
                                to: email,
                                subject,
                                htmlMessage
                                )
                {
                        IsBodyHtml = true
                });
        }
    }
}
