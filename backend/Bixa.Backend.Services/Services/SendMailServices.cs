using Bixa.Backend.Models.Templates.HTML;
using Microsoft.Extensions.Configuration;
using Bixa.Backend.Services.Interfaces;
using MailKit.Net.Smtp;
using MimeKit;

namespace Bixa.Backend.Services.Services;

public class SendMailServices(IConfiguration configuration) : ISendMailServices
{
    private readonly string remitente = configuration["EmailSettings:Remitente"]!;
    private readonly string password = configuration["EmailSettings:Password"]!;
    private readonly string smtpHost = configuration["EmailSettings:SmtpHost"]!;
    private readonly int smtpPort = int.Parse(configuration["EmailSettings:SmtpPort"]!);
    private readonly bool enableSsl = bool.Parse(configuration["EmailSettings:EnableSsl"]!);
    private readonly string host = configuration["EmailSettings:Host"]!;

    public async Task<bool> SendMailRetrievePassword(string destinatario, string Tokken)
    {
        const string asunto = "Recuperación de contraseña";
        var cuerpo = new RetrievePassword(host, Tokken).GetBodyMail();
        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Remitente", remitente));
            message.To.Add(new MailboxAddress("", destinatario));
            message.Subject = asunto;
            message.Body = new TextPart("html") { Text = cuerpo };

            using var client = new SmtpClient();
            await client.ConnectAsync(smtpHost, smtpPort, enableSsl);
            await client.AuthenticateAsync(remitente, password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
            return true;
        }
        catch
        {
            return false;
        }
    }
}