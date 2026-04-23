using Bixa.Backend.Models.DTOs.KeyValuePairModelDTO;
using Bixa.Backend.Models.DTOs.UserModelDTO;
using Bixa.Backend.Models.Response;

namespace Bixa.Backend.Services.Interfaces;

public interface IProfitService : IService<UserDTO, UserInsertDTO, UserEditDTO, UserFilterDTO, int>
{
    Task<Result<IEnumerable<KeyValuePairDTO<object, object>>>> GetKeyValuePairsAsync(UserFilterDTO filters, KeyFieldConfigurationDTO config);

    Task<Result<UserIdDTO>> GetUserByIdAsync(int id);

    Task<Result<bool>> UpdateUserPassword(UserChangePasswordDTO userEdited);
}