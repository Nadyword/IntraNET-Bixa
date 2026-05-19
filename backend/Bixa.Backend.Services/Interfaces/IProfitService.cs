using Bixa.Backend.Models.DTOs.UserModelDTO;
using Bixa.Backend.Models.Response;

namespace Bixa.Backend.Services.Interfaces;

public interface IProfitService : IService<UserDTO, UserInsertDTO, UserEditDTO, int>
{
    Task<Result<UserIdDTO>> GetUserByCiAsync(string ci);

    Task<Result<bool>> UpdateUserPassword(UserChangePasswordDTO userEdited);
}