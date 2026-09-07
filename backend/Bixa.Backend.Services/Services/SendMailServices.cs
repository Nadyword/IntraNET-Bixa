using Bixa.Backend.Models.Templates.HTML;
using Microsoft.Extensions.Configuration;
using Bixa.Backend.Services.Interfaces;
using MailKit.Net.Smtp;
using MimeKit;

namespace Bixa.Backend.Services.Services;

public class SendMailServices(IConfiguration configuration) : ISendMailServices
{
    private readonly string remitente = configuration["EmailSettings:Remitente"]!;
    private readonly string smtpUser = configuration["EmailSettings:SmtpUser"]!;
    private readonly string password = configuration["EmailSettings:Password"]!;
    private readonly string smtpHost = configuration["EmailSettings:SmtpHost"]!;
    private readonly int smtpPort = int.Parse(configuration["EmailSettings:SmtpPort"]!);
    private readonly bool enableSsl = bool.Parse(configuration["EmailSettings:EnableSsl"]!);
    private readonly string host = configuration["EmailSettings:Host"]!;

    public async Task<bool> SendMailRetrievePassword(string destinatario, string Tokken, string Ci)
    {
        const string asunto = "Recuperación de contraseña";
        var cuerpo = new RetrievePassword(host, Tokken, Ci).GetBodyMail();
        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Productos Bixa", remitente));
            message.To.Add(new MailboxAddress("Usuario", destinatario));
            message.Subject = asunto;
            message.Body = new TextPart("html") { Text = cuerpo };

            using var client = new SmtpClient();
            await client.ConnectAsync(smtpHost, smtpPort, enableSsl);
            await client.AuthenticateAsync(smtpUser, password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> SendMailNewUser(string destinatario, string tempPassword)
    {
        const string asunto = "¡Hola! Te damos la bienvenida al equipo de Bixa";
        var cuerpo = new WelcomeBixa(host, tempPassword).GetBodyMail();
        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Productos Bixa", remitente));
            message.To.Add(new MailboxAddress("Nuevo Usuario", destinatario));
            message.Subject = asunto;
            message.Body = new TextPart("html") { Text = cuerpo };

            using var client = new SmtpClient();
            await client.ConnectAsync(smtpHost, smtpPort, enableSsl);
            await client.AuthenticateAsync(smtpUser, password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> SendMailSolicitudPendienteAprobacion(string destinatario)
    {
        const string asunto = "Solicitud pendiente de tu aprobación";
        var cuerpo = new SolicitudPendienteAprobacion().GetBodyMail();
        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Productos Bixa", remitente));
            message.To.Add(new MailboxAddress("Aprobador", destinatario));
            message.Subject = asunto;
            message.Body = new TextPart("html") { Text = cuerpo };

            using var client = new SmtpClient();
            await client.ConnectAsync(smtpHost, smtpPort, enableSsl);
            await client.AuthenticateAsync(smtpUser, password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> SendMailSolicitudFirmadaCompleta(string destinatario)
    {
        const string asunto = "Solicitud lista para aprobación final";
        var cuerpo = new SolicitudFirmadaCompleta().GetBodyMail();
        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Productos Bixa", remitente));
            message.To.Add(new MailboxAddress("Administrador", destinatario));
            message.Subject = asunto;
            message.Body = new TextPart("html") { Text = cuerpo };

            using var client = new SmtpClient();
            await client.ConnectAsync(smtpHost, smtpPort, enableSsl);
            await client.AuthenticateAsync(smtpUser, password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> SendMailHcActualizado(string destinatario, string empleadoNombre, string empleadoCi)
    {
        const string asunto = "Registro de HC actualizado";
        var cuerpo = new HcRegistroActualizado(empleadoNombre, empleadoCi).GetBodyMail();
        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Productos Bixa", remitente));
            message.To.Add(new MailboxAddress("Administrador", destinatario));
            message.Subject = asunto;
            message.Body = new TextPart("html") { Text = cuerpo };

            using var client = new SmtpClient();
            await client.ConnectAsync(smtpHost, smtpPort, enableSsl);
            await client.AuthenticateAsync(smtpUser, password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> SendMailNuevoMensajeChat(string destinatario, string empleadoNombre, string empleadoCi)
    {
        const string asunto = "Nuevo mensaje en el chat de soporte";
        var cuerpo = new NuevoMensajeChat(empleadoNombre, empleadoCi).GetBodyMail();
        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Productos Bixa", remitente));
            message.To.Add(new MailboxAddress("Administrador", destinatario));
            message.Subject = asunto;
            message.Body = new TextPart("html") { Text = cuerpo };

            using var client = new SmtpClient();
            await client.ConnectAsync(smtpHost, smtpPort, enableSsl);
            await client.AuthenticateAsync(smtpUser, password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> SendMailSolicitudCorreccion(string destinatario, string empleadoNombre, string empleadoCi, string comentario)
    {
        const string asunto = "Solicitud de corrección de datos";
        var cuerpo = new CorreccionDatos(empleadoNombre, empleadoCi, comentario).GetBodyMail();
        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Productos Bixa", remitente));
            message.To.Add(new MailboxAddress("Administrador", destinatario));
            message.Subject = asunto;
            message.Body = new TextPart("html") { Text = cuerpo };

            using var client = new SmtpClient();
            await client.ConnectAsync(smtpHost, smtpPort, enableSsl);
            await client.AuthenticateAsync(smtpUser, password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> SendMailAriPlanilla(
        string destinatario,
        string empleadoNombre,
        string empleadoCi,
        string mes,
        int anoGravable,
        byte[] adjunto,
        string nombreArchivo)
    {
        var asunto = $"Planilla AR-I de {empleadoNombre} ({mes} {anoGravable})";
        var cuerpo = new AriPlanillaRecibida(empleadoNombre, empleadoCi, mes, anoGravable).GetBodyMail();
        try
        {
            var builder = new BodyBuilder { HtmlBody = cuerpo };
            builder.Attachments.Add(nombreArchivo, adjunto, ContentType.Parse("application/vnd.ms-excel"));

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Productos Bixa", remitente));
            message.To.Add(new MailboxAddress("Administrador", destinatario));
            message.Subject = asunto;
            message.Body = builder.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(smtpHost, smtpPort, enableSsl);
            await client.AuthenticateAsync(smtpUser, password);
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
