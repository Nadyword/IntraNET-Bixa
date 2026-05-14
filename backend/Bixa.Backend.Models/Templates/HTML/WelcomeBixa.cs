namespace Bixa.Backend.Models.Templates.HTML;

public class WelcomeBixa(string host, string tempPassword)
{
    private readonly string bodyMail = """
        <!DOCTYPE html>
        <html lang="es">
        <head>
            <meta charset="UTF-8">
            <meta name="viewport" content="width=device-width, initial-scale=1.0">
            <title>Bienvenida a BIXA - Portal Corporativo</title>
        </head>
        <body style="margin: 0; padding: 0; font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; background-color: #f5f5f5;">
            <table role="presentation" style="width: 100%; border-collapse: collapse; background-color: #f5f5f5; padding: 40px 0;">
                <tr>
                    <td align="center">
                        <table role="presentation" style="width: 100%; max-width: 600px; border-collapse: collapse; background-color: #ffffff; border-radius: 12px; overflow: hidden; box-shadow: 0 4px 20px rgba(0, 0, 0, 0.1);">
                            <tr>
                                <td style="background-color: #D93A3A; padding: 40px 30px; text-align: center;">
                                    <h1 style="margin: 0; color: #ffffff; font-size: 28px; font-weight: bold; letter-spacing: 2px;">BIXA</h1>
                                    <p style="margin: 5px 0 0 0; color: rgba(255, 255, 255, 0.9); font-size: 12px; letter-spacing: 3px; text-transform: uppercase;">Portal Corporativo</p>
                                </td>
                            </tr>
                            <tr>
                                <td style="padding: 40px 30px;">
                                    <h2 style="margin: 0 0 20px 0; color: #333333; font-size: 24px; font-weight: 600; text-align: center;">¡Qué bueno tenerte con nosotros!</h2>

                                    <p style="margin: 0 0 20px 0; color: #666666; font-size: 16px; line-height: 1.6; text-align: center;">
                                       Tu cuenta en el Portal Corporativo BIXA ya está lista. Para comenzar, debes ingresar a la plataforma utilizando la siguiente clave temporal:
                                    </p>

                                    <!-- Bloque destacado para la clave temporal -->
                                    <div style="text-align: center; margin: 0 0 25px 0;">
                                        <span style="display: inline-block; background-color: #f0f0f0; padding: 12px 25px; border-radius: 6px; font-size: 18px; font-weight: bold; color: #333333; letter-spacing: 1px; border: 1px dashed #cccccc;">
                                            {{TEMP_PASSWORD}}
                                        </span>
                                    </div>

                                    <p style="margin: 0 0 30px 0; color: #666666; font-size: 16px; line-height: 1.6; text-align: center;">
                                        Por tu seguridad, el sistema te solicitará <strong>cambiar esta contraseña de forma obligatoria</strong> apenas realices tu primer inicio de sesión.
                                    </p>

                                    <table role="presentation" style="width: 100%; border-collapse: collapse;">
                                        <tr>
                                            <td align="center" style="padding: 10px 0 30px 0;">
                                                <a href="{{LOGIN_LINK}}" style="display: inline-block; background-color: #D93A3A; color: #ffffff; text-decoration: none; padding: 16px 40px; border-radius: 8px; font-size: 16px; font-weight: 600; letter-spacing: 0.5px;">
                                                    Acceder al Portal →
                                                </a>
                                            </td>
                                        </tr>
                                    </table>

                                    <div style="background-color: #f8f8f8; border-radius: 8px; padding: 20px; margin-bottom: 20px;">
                                        <p style="margin: 0 0 10px 0; color: #888888; font-size: 13px; text-align: center;">
                                            Si el botón no funciona, copia y pega el siguiente enlace en tu navegador:
                                        </p>
                                        <p style="margin: 0; color: #D93A3A; font-size: 12px; word-break: break-all; text-align: center;">
                                            {{LOGIN_LINK}}
                                        </p>
                                    </div>

                                    <p style="margin: 0; color: #999999; font-size: 13px; text-align: center; font-style: italic;">
                                        Este acceso es personal e intransferible. Si tienes problemas para ingresar, contacta al administrador.
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
        return bodyMail.Replace("{{LOGIN_LINK}}", host).Replace("{{TEMP_PASSWORD}}", tempPassword);
    }
}