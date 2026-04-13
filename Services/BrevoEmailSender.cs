using System.Net;
using System.Net.Mail;
using Microsoft.AspNetCore.Identity;
using PuppetFestAPP.Web.Data;

namespace PuppetFestAPP.Web.Services;
public class BrevoEmailSender : IEmailSender<ApplicationUser>
{
   public async Task SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink)
{
    var html = EmailTemplates.ConfirmationEmailClean(confirmationLink);

    await SendEmailAsync(email, "Confirm your account", html);
}

    public async Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink)
    {
        await SendEmailAsync(email, "Reset your password",
            $"Click here: <a href='{resetLink}'>Reset Password</a>");
    }
    public async Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode)
{
    await SendEmailAsync(email, "Reset your password",
        $"Your password reset code is: <strong>{resetCode}</strong>");
}

    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        var smtpClient = new SmtpClient("smtp-relay.brevo.com")
        {
            Port = 587,
            Credentials = new NetworkCredential(
                "a8015d001@smtp-brevo.com",
                "PSCmUjIKEZFQVyxg"
            ),
            EnableSsl = true,
        };

        var mailMessage = new MailMessage
        {
            From = new MailAddress("graham03@colum.edu"),
            Subject = subject,
            Body = htmlMessage,
            IsBodyHtml = true,
        };

        mailMessage.To.Add(email);

        await smtpClient.SendMailAsync(mailMessage);
    }

    
}
