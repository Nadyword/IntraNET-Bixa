using Bixa.Backend.DataAccess.Interfaces.Repositories;
using Bixa.Backend.DataAccess.Interfaces.Repositories.Profit;
using Bixa.Backend.DataAccess.Repository;
using Bixa.Backend.DataAccess.Repository.Profit;
using Bixa.Backend.DataAccess.UnitOfWork;
using Bixa.Backend.DataAccess.Wrappers;
using Bixa.Backend.Services.Interfaces;
using Bixa.Backend.Services.Services;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Extensions.Options;

namespace Bixa.Backend.Configuration;

public static class ServiceRegistrationExtensions
{
    /// <summary>
    /// Adds all application-specific services (repositories, business services, CORS, etc.)
    /// to the Dependency Injection service collection.
    /// </summary>
    /// <param name="services">The service collection to which the services will be added.</param>
    /// <returns>The same IServiceCollection instance for chaining.</returns>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Infraestructura Central
        AddCoreInfrastructure(services);

        // Repositorios
        AddRepositories(services);

        // Repositorios de solo lectura (BD secundaria)
        AddProfitRepositories(services);

        // Servicios de Lógica de Negocio
        AddServices(services);

        // Configuración de CORS
        AddCorsConfiguration(services);

        return services;
    }

    #region Private Helper Methods

    /// <summary>
    /// Registra los repositorios base.
    /// </summary>
    /// <param name="services">La colección de servicios.</param>
    private static void AddBaseRepositories(IServiceCollection services)
    {
        // Base Repositories
        services.AddScoped<IAuthRepository, AuthRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserRolRepository, UserRolRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
    }

    /// <summary>
    /// Registra los servicios base.
    /// </summary>
    /// <param name="services">La colección de servicios.</param>
    private static void AddBaseServices(IServiceCollection services)
    {
        // Base Services
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IUserRolService, RolService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<ISendMailServices, SendMailServices>();
    }

    /// <summary>
    /// Configura los servicios de infraestructura central (Logging, UoW, Validadores).
    /// </summary>
    /// <param name="services">La colección de servicios.</param>
    private static void AddCoreInfrastructure(IServiceCollection services)
    {
        services.AddSingleton<LoggerWrapper>();
        services.AddHttpContextAccessor();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IEntityRelationshipValidatorService, EntityRelationshipValidatorService>();
    }

    /// <summary>
    /// Configura los servicios de Cross-Origin Resource Sharing (CORS).
    /// </summary>
    /// <param name="services">La colección de servicios.</param>
    private static void AddCorsConfiguration(IServiceCollection services)
    {
        services.AddSingleton<IConfigureOptions<CorsOptions>, CorsConfigurator>();
        services.AddCors();
    }

    /// <summary>
    /// Registra todos los repositorios de la aplicación.
    /// </summary>
    /// <param name="services">La colección de servicios.</param>
    private static void AddRepositories(IServiceCollection services)
    {
        AddBaseRepositories(services);
    }

    /// <summary>
    /// Registra los repositorios y el UnitOfWork de solo lectura para la BD secundaria.
    /// </summary>
    private static void AddProfitRepositories(IServiceCollection services)
    {
        services.AddScoped<ISnEmpleProfitRepository, SnEmpleProfitRepository>();
        services.AddScoped<IReadOnlyUnitOfWork, ReadOnlyUnitOfWork>();
    }

    /// <summary>
    /// Registra todos los servicios de lógica de negocio de la aplicación.
    /// </summary>
    /// <param name="services">La colección de servicios.</param>
    private static void AddServices(IServiceCollection services)
    {
        AddBaseServices(services);
    }

    #endregion Private Helper Methods
}