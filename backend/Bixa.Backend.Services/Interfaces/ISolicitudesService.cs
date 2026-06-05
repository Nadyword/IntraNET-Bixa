namespace Bixa.Backend.Services.Interfaces;

public interface ISolicitudesService
{
    /// <summary>
    /// Lista de las personas que tienen que aprobar la solicitud de permiso de un usuario, identificada por su CI.
    /// </summary>
    /// <param name="ci">La cédula de identidad del usuario.</param>
    /// <returns>Una lista de nombres de los aprobadores.</returns>
    Task<List<string>> GetAprovadoresPermisosByCi(string ci);
}