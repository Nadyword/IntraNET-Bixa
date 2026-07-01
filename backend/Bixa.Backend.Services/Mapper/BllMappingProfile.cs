using Bixa.Backend.Models.DTOs.SoporteChatModelDTO;
using Bixa.Backend.Models.DTOs.UserModelDTO;
using Bixa.Backend.DataAccess.Entities.DbProfit;
using Bixa.Backend.Models.DTOs.UserRolDTO;
using Bixa.Backend.Models.DTOs.FAQsDTO;
using Bixa.Backend.DataAccess.Entities;
using Bixa.Backend.Models.Response;
using System.Reflection;
using AutoMapper;
using Bixa.Backend.DataAccess.Entities.Solicitudes;
using Bixa.Backend.Models.DTOs.SolicitudesModelDTO;

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
            .ForMember(dest => dest.ModifiedByCi, opt => opt.MapFrom(src => src.ModifiedByCi))
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
            .ForMember(dest => dest.IdUserRol, opt => opt.MapFrom(src => src.IdUserRol))
            .ForMember(dest => dest.PasswordHash, opt => opt.MapFrom(src => src.PasswordHash));

        CreateMap<Users, UserChangePasswordDTO>()
            .ForMember(dest => dest.Password, opt => opt.Ignore()).ReverseMap()
            .ForMember(dest => dest.PasswordHash, opt => opt.MapFrom(src => src.Password));

        CreateMap<Users, UserEditDTO>()
            .ForMember(dest => dest.Enabled, opt => opt.MapFrom(src => src.IsActive))
            .ForMember(dest => dest.IdUserRol, opt => opt.MapFrom(src => src.IdUserRol))
            .ForMember(dest => dest.Password, opt => opt.Ignore())
            .ForMember(dest => dest.Ci, opt => opt.Ignore()).ReverseMap();

        CreateMap<SnEmple, Users>()
            .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.Nombres))
            .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.Apellidos))
            .ForMember(dest => dest.Ci, opt => opt.MapFrom(src => src.Ci))
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
            .ForMember(dest => dest.IdUserRol, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.Ignore())
            .ForMember(dest => dest.LastLogin, opt => opt.Ignore())
            .ForMember(dest => dest.RefreshToken, opt => opt.Ignore())
            .ForMember(dest => dest.RefreshTokenDate, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.ModifiedByCi, opt => opt.Ignore())
            .ForMember(dest => dest.ModifiedUser, opt => opt.Ignore())
            .ForMember(dest => dest.Notification, opt => opt.Ignore())
            .ForMember(dest => dest.UserRol, opt => opt.Ignore());

        #endregion User Mappings

        #region UserRol Mappings

        CreateMap<UserRol, UserRolDTO>().ReverseMap();

        #endregion UserRol Mappings

        #region SoporteChat Mappings

        CreateMap<SoporteChat, SoporteChatMDTO>()
            .ForMember(dest => dest.UserCi, opt => opt.MapFrom(src => src.UserCi))
            .ForMember(dest => dest.Message, opt => opt.MapFrom(src => src.Message))
            .ReverseMap();

        CreateMap<SoporteChat, SoporteChatRDTO>()
            .ForMember(dest => dest.UserCi, opt => opt.MapFrom(src => src.UserCi))
            .ForMember(dest => dest.Message, opt => opt.MapFrom(src => src.Message))
            .ForMember(dest => dest.RespondidoPorCi, opt => opt.MapFrom(src => src.RespondidoPorCi))
            .ReverseMap();

        #endregion SoporteChat Mappings

        #region FAQs Mappings

        CreateMap<FAQs, FAQsDTO>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Question, opt => opt.MapFrom(src => src.Question))
            .ForMember(dest => dest.Response, opt => opt.MapFrom(src => src.Response))
            .ReverseMap();

        #endregion FAQs Mappings

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

        #region Solicitudes Mappings

        #region Tramites

        CreateMap<TramiteDTO, Tramite>()
           .ForMember(dest => dest.TipoTramiteId, opt => opt.MapFrom(src => src.TipoTramiteId))
           .ForMember(dest => dest.UserCi, opt => opt.MapFrom(src => src.UserCi))
           .ForMember(dest => dest.Estado, opt => opt.MapFrom(src => src.Estado))
           .ForMember(dest => dest.MotivoRechazo, opt => opt.MapFrom(src => src.MotivoRechazo));

        CreateMap<Tramite, TramiteDTO>()
           .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
           .ForMember(dest => dest.TipoTramiteNombre, opt => opt.MapFrom(src => src.TipoTramite != null ? src.TipoTramite.Nombre : string.Empty))
           .ForMember(dest => dest.UserNombre, opt => opt.MapFrom(src => src.User != null ? $"{src.User.FirstName} {src.User.LastName}" : null))
           .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt))
           .ForMember(dest => dest.Vacaciones, opt => opt.MapFrom(src => src.SolicitudVacaciones));

        CreateMap<SolicitudVacaciones, VacacionesDetalleDTO>();

        #endregion Tramites

        #region Aprobaciones

        CreateMap<Aprobacion, AprobacionDTO>()
            .ForMember(dest => dest.TramiteId, opt => opt.MapFrom(src => src.TramiteId))
            .ForMember(dest => dest.AprobadorCi, opt => opt.MapFrom(src => src.AprobadorCi))
            .ForMember(dest => dest.Nombre, opt => opt.MapFrom(src =>
                src.Aprobador != null
                    ? (src.Aprobador.FirstName + " " + src.Aprobador.LastName).Trim()
                    : src.Nombre))
            .ForMember(dest => dest.Orden, opt => opt.MapFrom(src => src.Orden))
            .ForMember(dest => dest.Estado, opt => opt.MapFrom(src => src.Estado))
            .ForMember(dest => dest.Comentario, opt => opt.MapFrom(src => src.Comentario))
            .ForMember(dest => dest.FechaRespuesta, opt => opt.MapFrom(src => src.FechaRespuesta));

        #endregion Aprobaciones

        #region Vacaciones

        CreateMap<SolicVacacionesDTO, Tramite>()
            .ForMember(dest => dest.TipoTramiteId, opt => opt.MapFrom(src => src.TipoTramiteId))
            .ForMember(dest => dest.UserCi, opt => opt.MapFrom(src => src.Ci))
            .ForMember(dest => dest.Estado, opt => opt.MapFrom(src => src.Estado))
            .ReverseMap();

        CreateMap<SolicVacacionesDTO, SolicitudVacaciones>()
            .ForMember(dest => dest.Desde, opt => opt.MapFrom(src => src.FechaInicio))
            .ForMember(dest => dest.Hasta, opt => opt.MapFrom(src => src.FechaFin))
            .ForMember(dest => dest.DiasTotales, opt => opt.MapFrom(src => src.DiasTotales))
            .ForMember(dest => dest.Observaciones, opt => opt.MapFrom(src => src.Observaciones))
            .ReverseMap();

        #endregion Vacaciones

        #endregion Solicitudes Mappings
    }

    private static object? GetPropertyValueSafe(object source, string propertyName)
    {
        if (source == null) return null;
        PropertyInfo? property = source.GetType().GetProperty(propertyName);
        return property?.GetValue(source);
    }
}