using AutoMapper;
using Bixa.Backend.DataAccess.Entities;
using Bixa.Backend.DataAccess.Entities.DbProfit;
using Bixa.Backend.Models.DTOs.UserModelDTO;
using Bixa.Backend.Models.DTOs.UserRolDTO;
using Bixa.Backend.Models.Response;
using System.Reflection;

namespace Bixa.Backend.Services.Mapper;

public class BllMappingProfile : Profile
{
    public BllMappingProfile()
    {
        #region User Mappings

        CreateMap<Users, UserDTO>().ReverseMap();

        CreateMap<Users, UserIdDTO>()
            .ForMember(dest => dest.Enabled, opt => opt.MapFrom(src => src.IsActive))
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.IdUserRol, opt => opt.MapFrom(src => src.IdUserRol))
            .ForMember(dest => dest.LastLogin, opt => opt.MapFrom(src => src.LastLogin))
            .ForMember(dest => dest.Modified, opt => opt.MapFrom(src => src.UpdatedAt))
            .ForMember(dest => dest.ModifiedById, opt => opt.MapFrom(src => src.ModifiedById))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.FirstName))
            .ForMember(dest => dest.Ci, opt => opt.MapFrom(src => src.Ci))
            .ForMember(dest => dest.UserRol, opt => opt.MapFrom(src => src.UserRol))
            .ForMember(dest => dest.Created, opt => opt.MapFrom(src => src.CreatedAt)).ReverseMap()
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.Enabled))
            .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.Name));

        CreateMap<Users, UserInsertDTO>()
            .ForMember(dest => dest.Ci, opt => opt.MapFrom(src => src.Ci)).ReverseMap()
            .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName))
            .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
            .ForMember(dest => dest.IdUserRol, opt => opt.MapFrom(src => src.IdUserRol));

        CreateMap<Users, UserChangePasswordDTO>()
            .ForMember(dest => dest.Password, opt => opt.Ignore()).ReverseMap()
            .ForMember(dest => dest.PasswordHash, opt => opt.MapFrom(src => src.Password));

        CreateMap<Users, UserFilterDTO>()
            .ForMember(dest => dest.Enabled, opt => opt.MapFrom(src => src.IsActive))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.FirstName)).ReverseMap()
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.Enabled))
            .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.Name));

        CreateMap<Users, SnEmple>()
            .ForMember(dest => dest.Apellidos, opt => opt.MapFrom(src => src.LastName))
            .ForMember(dest => dest.Nombres, opt => opt.MapFrom(src => src.FirstName))
            .ForMember(dest => dest.Ci, opt => opt.MapFrom(src => src.Ci));

        #endregion User Mappings

        #region UserRol Mappings

        CreateMap<UserRol, UserRolDTO>().ReverseMap();

        #endregion UserRol Mappings

        #region ResultPaginated

        CreateMap(typeof(PaginatedResult<>), typeof(PaginatedResult<>))
            .ForMember("TotalPages", opt => opt.Ignore())
            .ForMember("Data", opt => opt.MapFrom((src, _, __, context) =>
            {
                PropertyInfo? dataProperty = src.GetType().GetProperty("Data");
                var sourceData = dataProperty?.GetValue(src) as IEnumerable<object> ?? [];
                return context.Mapper.Map<IEnumerable<object>>(sourceData);
            }))
            .ForCtorParam("data", opt => opt.MapFrom(src => GetPropertyValueSafe(src, "Data")))
            .ForCtorParam("totalRecords", opt => opt.MapFrom(src => GetPropertyValueSafe(src, "TotalRecords")))
            .ForCtorParam("currentPage", opt => opt.MapFrom(src => GetPropertyValueSafe(src, "CurrentPage")))
            .ForCtorParam("pageSize", opt => opt.MapFrom(src => GetPropertyValueSafe(src, "PageSize")));

        #endregion ResultPaginated
    }

    private static object? GetPropertyValueSafe(object source, string propertyName)
    {
        if (source == null) return null;
        PropertyInfo? property = source.GetType().GetProperty(propertyName);
        return property?.GetValue(source);
    }
}