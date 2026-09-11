using Bixa.Backend.DataAccess.Interfaces.Repositories;
using Bixa.Backend.Models.DTOs.UserModelDTO;
using Bixa.Backend.DataAccess.Entities.DbProfit;
using Bixa.Backend.DataAccess.Wrappers;
using Bixa.Backend.DataAccess.Entities;
using Bixa.Backend.Services.Interfaces;
using Bixa.Backend.Models.Response;
using Bixa.Backend.Models.Utilities;
using Bixa.Backend.Models.Enums;
using Microsoft.Extensions.Logging;
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
/// <param name="sendMail" >The email service instance for sending notifications.</param>
public class UserService(
    IUnitOfWork unitOfWork,
    IMapper mapper,
    IUserRepository userRepository,
    LoggerWrapper loggerWrapper,
    IReadOnlyUnitOfWork readOnlyUnitOfWork,
    ISendMailServices sendMail,
    IFirmaService firmaService) : IUserService
{
    private readonly ILogger<UserService> _logger = loggerWrapper.CreateLogger<UserService>();
    private readonly IMapper _mapper = mapper;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IReadOnlyUnitOfWork _readOnlyUnitOfWork = readOnlyUnitOfWork;
    private readonly ISendMailServices _sendMail = sendMail;
    private readonly IFirmaService _firmaService = firmaService;

    /// <summary>
    /// Adds a new user to the system.
    /// </summary>
    /// <param name="userDto">The user data transfer object containing information for the new user.</param>
    /// <returns>A <see cref="Result{T}"/> indicating success or failure, with the CI of the newly created user on success.</returns>
    public async Task<Result<string>> AddAsync(UserInsertDTO userDto)
    {
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            string ciNormalized = UtilityService.NormalizeCiFormat(userDto.Ci);
            var existingUser = await _userRepository.GetUserByCiAsync(ciNormalized);

            if (existingUser != null && existingUser.IsActive)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return Result.Fail<string>($"Ya existe un usuario con la cedula {ciNormalized}", ErrorTypeEnum.Conflict);
            }

            var snEmple = await _readOnlyUnitOfWork.SnEmple.GetFullInfoByCiAsync(ciNormalized);

            if (!snEmple.IsSuccess)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return Result.Fail<string>(snEmple.Error, snEmple.ErrorTypeEnum);
            }

            bool isReactivation = existingUser != null;
            Users userCreate = isReactivation ? existingUser! : _mapper.Map<SnEmple, Users>(snEmple.Value);

            if (isReactivation)
            {
                // No se usa el mapper sobre la entidad rastreada: sobrescribiría Ci (clave primaria) con el
                // valor crudo de Profit, que puede diferir en formato/espacios y hace fallar a EF.
                userCreate.FirstName = snEmple.Value.Nombres?.Trim();
                userCreate.LastName = snEmple.Value.Apellidos?.Trim();
                userCreate.RefreshToken = null;
                userCreate.RefreshTokenDate = null;
            }

            userCreate.IsActive = true;
            userCreate.IdUserRol = userDto.IdUserRol;
            userCreate.LastLogin = null;
            string clave = DateUtilities.GenerateSecureRandomPassword(10);
            userCreate.PasswordHash = clave;
            userCreate.PasswordHash = CheckIfNewPassword(userCreate.PasswordHash!, string.Empty);

            if (userDto.Firma != null)
            {
                var firmaResult = await _firmaService.ValidateAndSaveAsync(userDto.Firma, ciNormalized);
                if (!firmaResult.IsSuccess)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return Result.Fail<string>(firmaResult.Error!, ErrorTypeEnum.Validation);
                }
                userCreate.UrlFirma = firmaResult.Value!;
            }

            if (isReactivation)
                await _userRepository.UpdateAsync(userCreate);
            else
                await _userRepository.AddAsync(userCreate);

            var saveChangesSuccess = await _unitOfWork.SaveChangesAsync() > 0;

            if (saveChangesSuccess)
            {
                await _unitOfWork.CommitTransactionAsync();
                if (string.IsNullOrEmpty(snEmple.Value.CorreoE))
                    return Result.Success("Usuario creado exitosamente, pero no existe un correo empresarial asociado a este usuario. Es necesario registrar un correo realizar una recuperación de clave.");
                _ = _sendMail.SendMailNewUser(snEmple.Value.CorreoE, clave);
                return Result.Success(isReactivation ? "Usuario reactivado exitosamente." : "Usuario creado exitosamente.");
            }
            else
            {
                await _unitOfWork.RollbackTransactionAsync();
                return Result.Fail<string>("Error al insertar usuario (no se guardaron cambios)", ErrorTypeEnum.General);
            }
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            _logger?.LogError(ex, "Error al agregar usuario. Ci:{Ci}", userDto?.Ci);
            return Result.Fail<string>("Ocurrió un error al agregar el usuario.", ErrorTypeEnum.Database);
        }
    }

    /// <summary>
    /// Desactiva un usuario (baja lógica) por su identificador único.
    /// </summary>
    /// <param name="ci">The cedula of the user to deactivate.</param>
    /// <returns>A <see cref="Result{T}"/> indicating success or failure. True if deactivation was successful, false otherwise.</returns>
    public async Task<Result<bool>> DeleteAsync(string ci)
    {
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            ci = UtilityService.NormalizeCiFormat(ci);
            var userResult = await ValidateUserExistsAsync(ci);
            if (!userResult.IsSuccess)
                return userResult;

            var userToDelete = (await _unitOfWork.Users.GetByCiAsync(ci)).FirstOrDefault();

            if (userToDelete == null)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return Result.Fail<bool>("Usuario no encontrado.", ErrorTypeEnum.NotFound);
            }

            if (userToDelete.IdUserRol == (int)UserRolEnum.Administrador)
            {
                var isLastAdmin = await CheckIfLastAdmin(userToDelete.IdUserRol);

                if (isLastAdmin)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return Result.Fail<bool>("No se puede desactivar el usuario: Debe haber al menos un administrador activo en el sistema.", ErrorTypeEnum.Validation);
                }
            }

            userToDelete.IsActive = false;
            await _userRepository.UpdateAsync(userToDelete);
            var saveChangesSuccess = await _unitOfWork.SaveChangesAsync() > 0;

            if (saveChangesSuccess)
            {
                await _unitOfWork.CommitTransactionAsync();
                return Result.Success(true);
            }
            else
            {
                await _unitOfWork.RollbackTransactionAsync();
                return Result.Fail<bool>("Error al desactivar el usuario", ErrorTypeEnum.General);
            }
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            _logger?.LogError(ex, "Error al desactivar usuario con ci {Ci}", ci);
            return Result.Fail<bool>("Ocurrió un error al desactivar el usuario.", ErrorTypeEnum.Database);
        }
    }

    /// <summary>
    /// Retrieves a paginated list of users based on specified filters.
    /// </summary>
    /// <param name="pageNumber">The page number for pagination (default is 1).</param>
    /// <param name="pageSize">The number of items per page for pagination (default is 10).</param>
    /// <returns>A <see cref="Result{T}"/> indicating success or failure, with a paginated list of <see cref="UserDTO"/> on success.</returns>
    public async Task<Result<List<UserDTO>>> GetAllAsync(int pageNumber = 1, int pageSize = 10)
    {
        var users = await _userRepository.GetAllAsync(pageNumber, pageSize);

        return Result.Success(_mapper.Map<List<UserDTO>>(users));
    }

    /// <summary>
    /// Obtains a user by their CI.
    /// </summary>
    /// <param name="ci">The CI of the user to retrieve.</param>
    /// <returns>A <see cref="Result{T}"/> indicating success or failure, with the <see cref="UserDTO"/> on success.</returns>
    public async Task<Result<UserDTO>> GetByCiAsync(string ci)
    {
        var userEntity = (await _userRepository.GetByCiAsync(ci)).FirstOrDefault();
        if (userEntity == null)
            return Result.Fail<UserDTO>("Usuario no encontrado", ErrorTypeEnum.NotFound);

        var userDto = _mapper.Map<Users, UserDTO>(userEntity);
        return Result.Success(userDto);
    }

    /// <summary>
    /// Retrieves a single user by their unique identifier.
    /// </summary>
    /// <param name="ci">The CI of the user to retrieve.</param>
    /// <returns>A <see cref="Result{T}"/> indicating success or failure, with the <see cref="UserDTO"/> on success.</returns>
    public async Task<Result<UserIdDTO>> GetUserByCiAsync(string ci)
    {
        ci = UtilityService.NormalizeCiFormat(ci);
        var userEntity = (await _userRepository.GetByCiAsync(ci)).FirstOrDefault();
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
            dto.Ci = UtilityService.NormalizeCiFormat(dto.Ci);
            var userResult = await ValidateUserExistsAsync(dto.Ci);
            if (!userResult.IsSuccess)
                return userResult;

            SetEnabledIfNotProvided(dto);

            var entity = (await _userRepository.GetByCiAsync(dto.Ci)).FirstOrDefault();
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
                    return Result.Fail<bool>("No se puede cambiar el rol: Debe haber al menos un Administrador activo en el sistema.", ErrorTypeEnum.Validation);
                }
            }

            entity.IdUserRol = dto.IdUserRol ?? entity.IdUserRol;
            entity.IsActive = dto.Enabled ?? entity.IsActive;

            if (dto.Firma != null)
            {
                var firmaResult = await _firmaService.ValidateAndSaveAsync(dto.Firma, dto.Ci);
                if (!firmaResult.IsSuccess)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return Result.Fail<bool>(firmaResult.Error!, ErrorTypeEnum.Validation);
                }
                entity.UrlFirma = firmaResult.Value!;
            }

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();
            return Result.Success(true);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            _logger?.LogError(ex, "Error al actualizar usuario ci {UserCi}", dto?.Ci);
            return Result.Fail<bool>("Ocurrió un error al actualizar el usuario.", ErrorTypeEnum.Database);
        }
    }

    /// <summary>
    /// Updates a user's password.
    /// </summary>
    /// <param name="userEdited">The DTO containing the user's ID and new password.</param>
    /// <param name="newPassword">The new password string.</param>
    /// <returns>A <see cref="Result{T}"/> indicating success or failure. True if password update was successful, false otherwise.</returns>
    public async Task<Result<bool>> UpdateUserPassword(UserEditDTO userEdited, string newPassword)
    {
        try
        {
            var user = (await _userRepository.GetByCiAsync(userEdited.Ci)).FirstOrDefault();

            if (user == null)
            {
                return Result.Fail<bool>("Usuario no encontrado", ErrorTypeEnum.NotFound);
            }

            // Validación de la nueva contraseña (usa IsValidPassword)
            var validationErrors = ValidationUtils.IsValidPassword(newPassword);
            if (validationErrors.Count != 0)
            {
                // Concatenar todos los mensajes en uno solo para devolver en Message
                var combinedMessage = string.Join("; ", validationErrors);
                return Result.Fail<bool>(combinedMessage, ErrorTypeEnum.Validation);
            }

            if (!Hasher.VerifyPassword(userEdited.Password!, user.PasswordHash!)) return Result.Fail<bool>("Contraseña actual incorrecta", ErrorTypeEnum.Validation);

            // La nueva clave no puede coincidir con la clave actual
            if (Hasher.VerifyPassword(newPassword, user.PasswordHash!))
                return Result.Fail<bool>("La nueva clave no puede ser igual a la clave actual.", ErrorTypeEnum.Validation);

            // Si pasa validación, hasheamos y asignamos la nueva contraseña
            user.PasswordHash = CheckIfNewPassword(newPassword, user.PasswordHash!);

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
            _logger?.LogError(ex, "Error al cambiar contraseña para el usuario ci {UserCi}", userEdited?.Ci);
            return Result.Fail<bool>("Ocurrió un error al cambiar la contraseña.", ErrorTypeEnum.Database);
        }
    }

    public async Task<Result<bool>> ResendWelcomeEmail(string ci)
    {
        ci = UtilityService.NormalizeCiFormat(ci);
        var user = (await _userRepository.GetByCiAsync(ci)).FirstOrDefault();
        if (user == null)
            return Result.Fail<bool>("Usuario no encontrado", ErrorTypeEnum.NotFound);

        var snEmple = _readOnlyUnitOfWork.SnEmple.GetFullInfoByCiAsync(ci).Result;

        string clave = DateUtilities.GenerateSecureRandomPassword(10);
        user.PasswordHash = Hasher.HashPassword(clave);
        await _userRepository.UpdateUserPasswordAsync(user);
        var saveChangesSuccess = await _unitOfWork.SaveChangesAsync() > 0;

        try
        {
            _ = _sendMail.SendMailNewUser(snEmple.Value.CorreoE!, clave);
        }
        catch
        {
            return Result.Fail<bool>("Usuario actualizado pero no se pudo enviar el correo de bienvenida. Verifique que el correo electrónico esté registrado correctamente.", ErrorTypeEnum.General);
        }

        if (saveChangesSuccess)
            return Result.Success(true);
        else
            return Result.Fail<bool>("Error al intentar cambiar la contraseña (no se guardaron cambios)", ErrorTypeEnum.General);
    }

    /// <summary>
    /// Envía una solicitud de corrección de datos del empleado a todos los administradores activos por correo.
    /// </summary>
    /// <param name="ci">La CI del empleado que solicita la corrección.</param>
    /// <param name="comentario">El detalle de la corrección solicitada.</param>
    /// <returns>A <see cref="Result{T}"/> indicando si se pudo notificar al menos a un administrador.</returns>
    public async Task<Result<bool>> SolicitarCorreccion(string ci, string comentario)
    {
        ci = UtilityService.NormalizeCiFormat(ci);
        var solicitante = (await _userRepository.GetByCiAsync(ci)).FirstOrDefault();
        if (solicitante == null)
            return Result.Fail<bool>("Usuario no encontrado", ErrorTypeEnum.NotFound);

        var nombreSolicitante = $"{solicitante.FirstName} {solicitante.LastName}".Trim();

        var administradores = await _userRepository.GetActiveByRoleAsync((int)UserRolEnum.Administrador);
        if (administradores.Count == 0)
            return Result.Fail<bool>("No hay administradores activos registrados en el sistema.", ErrorTypeEnum.NotFound);

        var enviosExitosos = 0;
        foreach (var admin in administradores)
        {
            var correo = await _readOnlyUnitOfWork.SnEmple.GetEmailByCiAsync(admin.Ci);
            if (string.IsNullOrWhiteSpace(correo))
                continue;

            var enviado = await _sendMail.SendMailSolicitudCorreccion(correo, nombreSolicitante, ci, comentario);
            if (enviado)
                enviosExitosos++;
        }

        if (enviosExitosos == 0)
            return Result.Fail<bool>("No se pudo notificar a ningún administrador. Verifique que tengan un correo registrado.", ErrorTypeEnum.General);

        return Result.Success(true);
    }

    public async Task<List<SnEmple>> GetAllSnEmpleAsync(int pageNumber = 1, int pageSize = 10)
    {
        return await _readOnlyUnitOfWork.SnEmple.GetAllAsync(pageNumber, pageSize);
    }

    /// <summary>
    /// Obtiene de forma asíncrona una lista de empleados asociados a los usuarios especificados por grupo de CI.
    /// </summary>
    /// <param name="users">La lista de usuarios para los que se recuperarán los empleados asociados. No puede ser nula.</param>
    /// <returns>Un resultado que contiene una lista de objetos SnEmple asociados a los usuarios proporcionados. Si no se
    /// encuentran empleados, la lista estará vacía.</returns>
    public async Task<Result<List<SnEmple>>> GetAllByCiAsync(List<UserDTO> users)
    {
        return await _readOnlyUnitOfWork.SnEmple.GetAllByCiAsync(users);
    }

    /// <summary>
    /// Obtiene todo el personal a cargo (directo e indirecto) del supervisor con la CI dada.
    /// </summary>
    /// <param name="ci">La CI del supervisor autenticado.</param>
    public async Task<Result<List<EquipoSupervisor>>> GetEquipoSupervisorAsync(string ci)
    {
        ci = UtilityService.NormalizeCiFormat(ci);
        var equipo = await _readOnlyUnitOfWork.SnEmple.GetEquipoSupervisorAsync(ci);
        return Result.Success(equipo);
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
        const int AdminRoleId = (int)UserRolEnum.Administrador;

        if (currentRoleId == AdminRoleId)
            return await _unitOfWork.Users.CountAdminUsersAsync(AdminRoleId) <= 1;

        return false;
    }

    private void SetEnabledIfNotProvided(UserEditDTO dto)
    {
        if (!dto.Enabled.HasValue)
        {
            var entity = _userRepository.GetByCiAsync(dto.Ci).Result.FirstOrDefault();
            dto.Enabled = entity?.IsActive;
        }
    }

    private async Task<Result<bool>> ValidateUserExistsAsync(string ci)
    {
        var existingUser = (await _userRepository.GetByCiAsync(ci)).FirstOrDefault();
        if (existingUser == null)
        {
            await _unitOfWork.RollbackTransactionAsync();
            return Result.Fail<bool>("Usuario no encontrado", ErrorTypeEnum.NotFound);
        }
        return Result.Success(true);
    }
}