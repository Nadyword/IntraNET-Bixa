using AutoMapper;
using Bixa.Backend.DataAccess.Entities;
using Bixa.Backend.DataAccess.Interfaces.Repositories;
using Bixa.Backend.Models.DTOs.UserRolDTO;
using Bixa.Backend.Models.Enums;
using Bixa.Backend.Models.Response;
using Bixa.Backend.Services.Interfaces;

namespace Bixa.Backend.Services.Services;

/// <summary>
/// Provides business logic for managing roles, interacting with the role repository layer.
/// </summary>
public class RolService : IUserRolService
{
    private readonly IUserRolRepository _rolRepository;
    private readonly IMapper _mapper;

    /// <summary>
    /// Initializes a new instance of the RolService.
    /// </summary>
    /// <param name="rolRepository">The role repository instance for data access.</param>
    /// <param name="mapper">The AutoMapper instance for DTO conversions.</param>
    public RolService(IUserRolRepository rolRepository, IMapper mapper)
    {
        _rolRepository = rolRepository;
        _mapper = mapper;
    }

    /// <summary>
    /// Retrieves all roles from the repository and maps them to RolDTOs.
    /// </summary>
    /// <returns>A <see cref="Result{T}"/> containing a list of <see cref="RolDTO"/>,
    /// or an error if no roles are found.</returns>
    public async Task<Result<PaginatedResult<UserRolDTO>>> GetAllUserRolsAsync()
    {
        var roles = await _rolRepository.GetAllRoles();

        if (roles == null || !roles.Data.Any())
            return Result.Fail<PaginatedResult<UserRolDTO>>("No users found", ErrorTypeEnum.NotFound);

        return Result.Success(_mapper.Map<PaginatedResult<UserRol>, PaginatedResult<UserRolDTO>>(roles));
    }
}