namespace IntranetCorp.Infrastructure.Services;

public interface IChatbotService
{
    Task<string> GetResponseAsync(string message);
    Task<Dictionary<string, string>> GetFaqsAsync();
}

public class ChatbotFaqService : IChatbotService
{
    private readonly Dictionary<string, string> _faqs;

    public ChatbotFaqService()
    {
        _faqs = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "vacaciones", "Para solicitar vacaciones, ve a Solicitudes > Vacaciones, completa las fechas y envía. Tu líder recibirá la solicitud." },
            { "finiquito", "El finiquito es la liquidación de prestaciones. Requiere aprobación de RRHH y es válido solo al cierre de relación." },
            { "constancia", "Una constancia de trabajo acredita tu empleo en la empresa. Solicítala en Solicitudes > Constancia de Trabajo." },
            { "permiso", "Los permisos especiales son días no pagos para asuntos personales. Solicita en Solicitudes > Permiso Especial." },
            { "prestamo", "Puedes solicitar un préstamo sobre tus prestaciones. Ve a Solicitudes > Préstamo de Utilidades. Máximo 50% de lo acumulado." },
            { "dias vacaciones", "Tienes derecho a 15 días hábiles de vacaciones por año, según la LOTTT." },
            { "prestaciones", "Las prestaciones se calculan por antigüedad: 15 días x año. Se acumulan incluso si no las usas." },
            { "sueldo", "Consulta tu nómina en Mis Datos. El sueldo se deposita el último día hábil del mes." },
            { "beneficios", "Ve a Cultura > Beneficios para conocer el plan de salud, bonificaciones y otros beneficios." },
            { "documentos", "Carga documentos desde Solicitudes. Todos los archivos son protegidos y solo accesibles para ti y RRHH." },
            { "antecedentes", "Ver tu historial de solicitudes en Mis Trámites. Puedes descargar PDFs de solicitudes finalizadas." },
            { "onboarding", "Completa tu checklist de incorporación en la sección Onboarding. Son 8 pasos para tu integración." },
            { "contraseña", "Puedes cambiar tu contraseña desde tu perfil. Usa una clave segura con mayúsculas, números y caracteres especiales." },
            { "ayuda", "Para más ayuda, contacta a RRHH o usa este chat. Estoy aquí para responder tus preguntas." },
            { "hola", "¡Hola! Soy el asistente de BIXA Intranet. ¿Cómo puedo ayudarte hoy?" }
        };
    }

    public Task<string> GetResponseAsync(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return Task.FromResult("Por favor, escribe una pregunta válida.");

        var keyword = FindBestMatch(message);

        if (_faqs.TryGetValue(keyword, out var response))
            return Task.FromResult(response);

        return Task.FromResult("No encontré una respuesta a tu pregunta. Intenta con: vacaciones, finiquito, constancia, permiso, prestamo, beneficios, etc.");
    }

    public Task<Dictionary<string, string>> GetFaqsAsync()
    {
        return Task.FromResult(_faqs);
    }

    private string FindBestMatch(string message)
    {
        var words = message.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);

        foreach (var word in words)
        {
            if (_faqs.Keys.Any(k => k.StartsWith(word) || word.Contains(k)))
                return _faqs.Keys.First(k => k.StartsWith(word) || word.Contains(k));
        }

        return _faqs.Keys.FirstOrDefault() ?? "hola";
    }
}
