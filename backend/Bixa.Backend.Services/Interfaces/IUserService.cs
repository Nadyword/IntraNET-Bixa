using Bixa.Backend.Models.DTOs.UserModelDTO;
using Bixa.Backend.DataAccess.Entities.DbProfit;
using Bixa.Backend.Models.Response;

namespace Bixa.Backend.Services.Interfaces;

public interface IUserService : IService<UserDTO, UserInsertDTO, UserEditDTO, string>
{
    Task<Result<UserIdDTO>> GetUserByCiAsync(string ci);

    Task<Result<bool>> ResendWelcomeEmail(string ci);

    Task<Result<bool>> UpdateUserPassword(UserEditDTO userEdited, string newPassword);

    Task<Result<List<SnEmple>>> GetAllByCiAsync(List<UserDTO> users);

    Task<Result<bool>> SolicitarCorreccion(string ci, string comentario);

    /// <summary>
    /// Obtiene todo el personal a cargo (directo e indirecto) del supervisor con la CI dada.
    /// </summary>
    /// <param name="ci">La CI del supervisor autenticado.</param>
    Task<Result<List<EquipoSupervisor>>> GetEquipoSupervisorAsync(string ci);
}