using Bixa.Backend.Models.DTOs.KeyValuePairModelDTO;
using Bixa.Backend.DataAccess.Interfaces.Repositories;
using Bixa.Backend.Models.DTOs.UserModelDTO;
using Bixa.Backend.DataAccess.Wrappers;
using Bixa.Backend.DataAccess.Entities;
using Bixa.Backend.Services.Interfaces;
using Bixa.Backend.Models.Response;
using Bixa.Backend.Models.Utilities;
using Bixa.Backend.Models.Enums;
using Microsoft.Extensions.Logging;
using Bixa.Backend.Models.Query;
using AutoMapper;

namespace Bixa.Backend.Services.Services;

/// <summary>
/// Service for managing user-related business logic and operations.
/// Implements generic CRUD operations and specific user functionalities.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="UserService"/> class.
/// </remarks>
/// <param name="unitOfWork">The Unit of Work instance for managing database transactions.</param>
/// <param name="mapper">The AutoMapper instance for object mapping.</param>
/// <param name="userRepository">The user repository instance for data access.</param>
public class UserService(
    IUnitOfWork unitOfWork,
    IMapper mapper,
    IUserRepository userRepository,
    LoggerWrapper loggerWrapper,
    IReadOnlyUnitOfWork readOnlyUnitOfWork) : IUserService
{
    private readonly ILogger<UserService> _logger = loggerWrapper.CreateLogger<UserService>();
    private readonly IMapper _mapper = mapper;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IReadOnlyUnitOfWork _readOnlyUnitOfWork = readOnlyUnitOfWork;

    /// <summary>
    /// Adds a new user to the system.
    /// </summary>
    /// <param name="userDto">The user data transfer object containing information for the new user.</param>
    /// <returns>A <see cref="Result{T}"/> indicating success or failure, with the ID of the newly created user on success.</returns>
    public async Task<Result<int>> AddAsync(UserInsertDTO userDto)
    {
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            string ciNormalized = UtilityService.NormalizeCiFormat(userDto.Ci);
            var validationResult = await ValidateUserExistenceAsync(ciNormalized, null);

            if (!validationResult.IsSuccess)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return Result.Fail<int>(validationResult.Error!, ErrorTypeEnum.Conflict);
            }

            var snEmple = await _readOnlyUnitOfWork.SnEmple.GetFullInfoByCiAsync(userDto.Ci);

            if (snEmple == null)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return Result.Fail<int>("Empleado no encontrado en la BD secundaria.", ErrorTypeEnum.NotFound);
            }

            Users userCreate = _mapper.Map<UserInsertDTO, Users>(userDto);
            userCreate = _mapper.Map(snEmple, userCreate);
            userCreate.IsActive = true;
            userCreate.PasswordHash = CheckIfNewPassword(userCreate.PasswordHash!, string.Empty);

            await _userRepository.AddAsync(userCreate);
            var saveChangesSuccess = await _unitOfWork.SaveChangesAsync() > 0;

            if (saveChangesSuccess)
            {
                await _unitOfWork.CommitTransactionAsync();
                return Result.Success(userCreate.Id);
            }
            else
            {
                await _unitOfWork.RollbackTransactionAsync();
                return Result.Fail<int>("Error al insertar usuario (no se guardaron cambios)", ErrorTypeEnum.General);
            }
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            _logger?.LogError(ex, "Error al agregar usuario. Ci:{Ci}", userDto?.Ci);
            return Result.Fail<int>("Ocurrió un error al agregar el usuario.", ErrorTypeEnum.Database);
        }
    }

    /// <summary>
    /// Deletes a user by their unique identifier.
    /// </summary>
    /// <param name="Id">The ID of the user to delete.</param>
    /// <returns>A <see cref="Result{T}"/> indicating success or failure. True if deletion was successful, false otherwise.</returns>
    public async Task<Result<bool>> DeleteAsync(int Id)
    {
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var userResult = await ValidateUserExistsAsync(Id);
            if (!userResult.IsSuccess)
                return userResult;

            var userToDelete = (await _unitOfWork.Users.GetByIdAsync(Id)).FirstOrDefault();

            if (userToDelete == null)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return Result.Fail<bool>("Usuario no encontrado.", ErrorTypeEnum.NotFound);
            }

            if (userToDelete.IdUserRol == (int)UserRolEnum.SuperIntendente)
            {
                var isLastAdmin = await CheckIfLastAdmin(userToDelete.IdUserRol);

                if (isLastAdmin)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return Result.Fail<bool>("No se puede eliminar el usuario: Debe haber al menos un administrador activo en el sistema.", ErrorTypeEnum.Validation);
                }
            }

            //Si es Ejecutivo, eliminarlo de todos sus planes de gastos y solicitudes

            await _userRepository.DeleteNotificationsFromUserAsync(Id);
            var deletedSuccessfullyMarked = await _userRepository.DeleteAsync(Id);
            var saveChangesSuccess = await _unitOfWork.SaveChangesAsync() > 0;

            if (deletedSuccessfullyMarked && saveChangesSuccess)
            {
                await _unitOfWork.CommitTransactionAsync();
                return Result.Success(true);
            }
            else
            {
                await _unitOfWork.RollbackTransactionAsync();
                return Result.Fail<bool>("Error al eliminar el usuario", ErrorTypeEnum.General);
            }
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            _logger?.LogError(ex, "Error al eliminar usuario con id {Id}", Id);
            return Result.Fail<bool>("Ocurrió un error al eliminar el usuario.", ErrorTypeEnum.Database);
        }
    }

    /// <summary>
    /// Retrieves a paginated list of users based on specified filters.
    /// </summary>
    /// <param name="filters">The search query containing filtering and pagination parameters.</param>
    /// <returns>A <see cref="Result{T}"/> indicating success or failure, with a paginated list of <see cref="UserDTO"/> on success.</returns>
    public async Task<Result<PaginatedResult<UserDTO>>> GetAllAsync(SearchQuery<UserFilterDTO> filters)
    {
        var users = await _userRepository.GetAllAsync(filters.Filters ?? new object(), filters.Pagination);
        if (users.Data.Count == 0)
            return Result.Fail<PaginatedResult<UserDTO>>("No se encontraron usuarios", ErrorTypeEnum.NotFound);

        return Result.Success(_mapper.Map<PaginatedResult<Users>, PaginatedResult<UserDTO>>(users));
    }

    public async Task<Result<UserDTO>> GetByIdAsync(int id)
    {
        var userEntity = (await _userRepository.GetByIdAsync(id)).FirstOrDefault();
        if (userEntity == null)
            return Result.Fail<UserDTO>("Usuario no encontrado", ErrorTypeEnum.NotFound);

        var userDto = _mapper.Map<Users, UserDTO>(userEntity);
        return Result.Success(userDto);
    }

    public async Task<Result<IEnumerable<KeyValuePairDTO<object, object>>>> GetKeyValuePairsAsync(UserFilterDTO filters, KeyFieldConfigurationDTO config)
    {
        try
        {
            var result = await _unitOfWork.Users.GetKeyValuePairsAsync(filters, config);
            return Result.Success(result);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al recuperar pares clave-valor");
            return Result.Fail<IEnumerable<KeyValuePairDTO<object, object>>>("Ocurrió un error al recuperar los datos.", ErrorTypeEnum.Database);
        }
    }

    /// <summary>
    /// Retrieves a single user by their unique identifier.
    /// </summary>
    /// <param name="id">The ID of the user to retrieve.</param>
    /// <returns>A <see cref="Result{T}"/> indicating success or failure, with the <see cref="UserDTO"/> on success.</returns>
    public async Task<Result<UserIdDTO>> GetUserByIdAsync(int id)
    {
        var userEntity = (await _userRepository.GetByIdAsync(id)).FirstOrDefault();
        if (userEntity == null)
            return Result.Fail<UserIdDTO>("Usuario no encontrado", ErrorTypeEnum.NotFound);

        var userIdDto = _mapper.Map<UserIdDTO>(userEntity);
        return Result.Success(userIdDto);
    }

    /// <summary>
    /// Updates an existing user's information.
    /// </summary>
    /// <param name="dto">The DTO containing the updated user data.</param>
    /// <returns>A <see cref="Result{T}"/> indicating success or failure. True if update was successful, false otherwise.</returns>
    public async Task<Result<bool>> UpdateAsync(UserEditDTO dto)
    {
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var userResult = await ValidateUserExistsAsync(dto.Id);
            if (!userResult.IsSuccess)
                return userResult;

            var validationResult = await ValidateUserUpdateAsync(dto);
            if (!validationResult.IsSuccess)
                return validationResult;

            SetEnabledIfNotProvided(dto);

            var entity = (await _userRepository.GetByIdAsync(dto.Id)).FirstOrDefault();
            if (entity == null)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return Result.Fail<bool>("Usuario no encontrado", ErrorTypeEnum.NotFound);
            }

            if (dto.IdUserRol.HasValue && dto.IdUserRol != entity.IdUserRol)
            {
                var isLastAdmin = await CheckIfLastAdmin(entity.IdUserRol);
                if (isLastAdmin)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return Result.Fail<bool>("No se puede cambiar el rol: Debe haber al menos un administrador activo en el sistema.", ErrorTypeEnum.Validation);
                }
            }

            var effectiveRole = dto.IdUserRol ?? entity.IdUserRol;

            var updatedEntity = _mapper.Map(dto, entity);
            await _userRepository.UpdateAsync(updatedEntity);

            var saveChangesSuccess = await _unitOfWork.SaveChangesAsync() > 0;

            if (saveChangesSuccess)
            {
                await _unitOfWork.CommitTransactionAsync();
                return Result.Success(true);
            }
            else
            {
                await _unitOfWork.RollbackTransactionAsync();
                return Result.Fail<bool>("Error al intentar actualizar el usuario (no se guardaron cambios)", ErrorTypeEnum.General);
            }
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            _logger?.LogError(ex, "Error al actualizar usuario id {UserId}", dto?.Id);
            return Result.Fail<bool>("Ocurrió un error al actualizar el usuario.", ErrorTypeEnum.Database);
        }
    }

    /// <summary>
    /// Updates a user's password.
    /// </summary>
    /// <param name="userEdited">The DTO containing the user's ID and new password.</param>
    /// <returns>A <see cref="Result{T}"/> indicating success or failure. True if password update was successful, false otherwise.</returns>
    public async Task<Result<bool>> UpdateUserPassword(UserChangePasswordDTO userEdited)
    {
        try
        {
            var user = (await _userRepository.GetByIdAsync(userEdited.Id)).FirstOrDefault();

            if (user == null)
            {
                return Result.Fail<bool>("Usuario no encontrado", ErrorTypeEnum.NotFound);
            }

            // Validación de la nueva contraseña (usa IsValidPassword)
            var validationErrors = ValidationUtils.IsValidPassword(userEdited.Password);
            if (validationErrors.Count != 0)
            {
                // Concatenar todos los mensajes en uno solo para devolver en Message
                var combinedMessage = string.Join("; ", validationErrors);
                return Result.Fail<bool>(combinedMessage, ErrorTypeEnum.Validation);
            }

            // Si pasa validación, hasheamos y asignamos la nueva contraseña
            user.PasswordHash = CheckIfNewPassword(userEdited.Password ?? string.Empty, user.PasswordHash!);

            var successfullyMarked = await _userRepository.UpdateUserPasswordAsync(user);

            if (!successfullyMarked)
                return Result.Fail<bool>("Usuario no encontrado o no se pudo marcar para la actualización de contraseña.", ErrorTypeEnum.NotFound);

            var saveChangesSuccess = await _unitOfWork.SaveChangesAsync() > 0;

            if (saveChangesSuccess)
                return Result.Success(true);
            else
                return Result.Fail<bool>("Error al intentar cambiar la contraseña (no se guardaron cambios)", ErrorTypeEnum.General);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al cambiar contraseña para el usuario id {UserId}", userEdited?.Id);
            return Result.Fail<bool>("Ocurrió un error al cambiar la contraseña.", ErrorTypeEnum.Database);
        }
    }

    /// <summary>
    /// Hashes the new password if provided, otherwise returns the existing password.
    /// </summary>
    /// <param name="password">The new password string.</param>
    /// <param name="userpassword">The current user's password.</param>
    /// <returns>The hashed new password or the existing password.</returns>
    private static string CheckIfNewPassword(string password, string userpassword)
    {
        return !string.IsNullOrEmpty(password) ? Hasher.HashPassword(password) : userpassword;
    }

    private async Task<bool> CheckIfLastAdmin(int currentRoleId)
    {
        const int AdminRoleId = (int)UserRolEnum.SuperIntendente;

        if (currentRoleId == AdminRoleId)
            return await _unitOfWork.Users.CountAdminUsersAsync(AdminRoleId) <= 1;

        return false;
    }

    private void SetEnabledIfNotProvided(UserEditDTO dto)
    {
        if (!dto.Enabled.HasValue)
        {
            var entity = _userRepository.GetByIdAsync(dto.Id).Result.FirstOrDefault();
            dto.Enabled = entity?.IsActive;
        }
    }

    /// <summary>
    /// Validates if a user with the given email or Tax ID already exists,
    /// excluding a specific user ID for update scenarios.
    /// </summary>
    /// <param name="ci">The CI to check for existence.</param>
    /// <param name="currentUserId">The ID of the user being updated. If provided, this user will be excluded from the existence check.</param>
    /// <returns>A <see cref="Result"/> indicating success if validation passes, or failure with an appropriate error.</returns>
    private async Task<Result> ValidateUserExistenceAsync(string? ci, int? currentUserId)
    {
        if (!string.IsNullOrEmpty(ci))
        {
            var existingUserByCi = await _userRepository.GetUserByCiAsync(ci);
            if (existingUserByCi != null && existingUserByCi.Id != currentUserId)
                return Result.Fail($"Ya existe un usuario con el RUT {ci}", ErrorTypeEnum.Conflict);
        }

        return Result.Success();
    }

    private async Task<Result<bool>> ValidateUserExistsAsync(int id)
    {
        var existingUser = (await _userRepository.GetByIdAsync(id)).FirstOrDefault();
        if (existingUser == null)
        {
            await _unitOfWork.RollbackTransactionAsync();
            return Result.Fail<bool>("Usuario no encontrado", ErrorTypeEnum.NotFound);
        }
        return Result.Success(true);
    }

    private async Task<Result<bool>> ValidateUserUpdateAsync(UserEditDTO dto)
    {
        var validationResult = await ValidateUserExistenceAsync(dto.Ci, dto.Id);
        if (!validationResult.IsSuccess)
            return Result.Fail<bool>(validationResult.Error!);
        return Result.Success(true);
    }
}