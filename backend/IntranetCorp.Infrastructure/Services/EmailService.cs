using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace IntranetCorp.Infrastructure.Services;

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string htmlBody);
}

public class EmailService : IEmailService
{
    private readonly string _smtpHost;
    private readonly int _smtpPort;
    private readonly string _smtpUser;
    private readonly string _smtpPassword;

    public EmailService(string smtpHost, int smtpPort, string smtpUser, string smtpPassword)
    {
        _smtpHost = smtpHost;
        _smtpPort = smtpPort;
        _smtpUser = smtpUser;
        _smtpPassword = smtpPassword;
    }

    public async Task SendEmailAsync(string to, string subject, string htmlBody)
    {
        try
        {
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(_smtpUser));
            email.To.Add(MailboxAddress.Parse(to));
            email.Subject = subject;

            var builder = new BodyBuilder { HtmlBody = htmlBody };
            email.Body = builder.ToMessageBody();

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(_smtpHost, _smtpPort, SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(_smtpUser, _smtpPassword);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }
        catch (Exception ex)
        {
            // Log error
            throw new InvalidOperationException($"Error al enviar email: {ex.Message}", ex);
        }
    }
}
