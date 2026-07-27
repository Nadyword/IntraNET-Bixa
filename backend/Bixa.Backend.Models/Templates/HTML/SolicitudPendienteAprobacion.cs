namespace Bixa.Backend.Models.Templates.HTML;

public class SolicitudPendienteAprobacion
{
    private readonly string bodyMail = """
        <!DOCTYPE html>
        <html lang="es">
        <head>
            <meta charset="UTF-8">
            <meta name="viewport" content="width=device-width, initial-scale=1.0">
            <title>Solicitud Pendiente de Aprobación - BIXA</title>
        </head>
        <body style="margin: 0; padding: 0; font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; background-color: #f5f5f5;">
            <table role="presentation" style="width: 100%; border-collapse: collapse; background-color: #f5f5f5; padding: 40px 0;">
                <tr>
                    <td align="center">
                        <table role="presentation" style="width: 100%; max-width: 600px; border-collapse: collapse; background-color: #ffffff; border-radius: 12px; overflow: hidden; box-shadow: 0 4px 20px rgba(0, 0, 0, 0.1);">
                            <tr>
                                <td style="background-color: #D93A3A; padding: 40px 30px; text-align: center;">
                                    <h1 style="margin: 0; color: #ffffff; font-size: 28px; font-weight: bold; letter-spacing: 2px;">Comunik2</h1>
                                    <p style="margin: 5px 0 0 0; color: rgba(255, 255, 255, 0.9); font-size: 12px; letter-spacing: 3px; text-transform: uppercase;">Portal Corporativo</p>
                                </td>
                            </tr>
                            <tr>
                                <td style="padding: 40px 30px;">
                                    <h2 style="margin: 0 0 20px 0; color: #333333; font-size: 24px; font-weight: 600; text-align: center;">Solicitud pendiente de tu aprobación</h2>

                                    <p style="margin: 0; color: #666666; font-size: 16px; line-height: 1.6; text-align: center;">
                                        Tienes una nueva solicitud pendiente de tu aprobación en el Portal Corporativo BIXA. Ingresa al portal para revisarla.
                                    </p>
                                </td>
                            </tr>
                            <tr>
                                <td style="background-color: #f8f8f8; padding: 25px 30px; border-top: 1px solid #eeeeee;">
                                    <p style="margin: 0 0 10px 0; color: #888888; font-size: 13px; text-align: center;">
                                        Tu espacio de trabajo digital
                                    </p>
                                    <p style="margin: 0; color: #aaaaaa; font-size: 11px; text-align: center;">
                                        © 2026 BIXA Portal Corporativo. Todos los derechos reservados.
                                    </p>
                                    <p style="margin: 15px 0 0 0; color: #aaaaaa; font-size: 11px; text-align: center;">
                                        Este es un correo automático, por favor no respondas a este mensaje.
                                    </p>
                                </td>
                            </tr>
                        </table>
                    </td>
                </tr>
            </table>
        </body>
        </html>
        """;

    public string GetBodyMail()
    {
        return bodyMail;
    }
}