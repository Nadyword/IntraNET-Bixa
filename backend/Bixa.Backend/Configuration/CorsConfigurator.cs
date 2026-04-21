using System.Net;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Extensions.Options;

namespace Bixa.Backend.Configuration;

/// <summary>
/// Configures CORS policies for the application using the IConfigureOptions pattern.
/// This allows for dependency injection of configuration, environment, and logger.
/// </summary>
/// <remarks>
/// Initializes a new instance of the CorsConfigurator class.
/// </remarks>
/// <param name="logger">The logger for logging information and errors.</param>
/// <param name="configuration">The application's configuration settings.</param>
/// <param name="environment">The application's web hosting environment.</param>
public class CorsConfigurator(ILogger<CorsConfigurator> logger, IConfiguration configuration, IWebHostEnvironment environment) : IConfigureOptions<CorsOptions>
{
    private readonly IConfiguration _configuration = configuration;
    private readonly IWebHostEnvironment _environment = environment;
    private readonly ILogger<CorsConfigurator> _logger = logger;

    /// <summary>
    /// Configures the CorsOptions with default policy settings.
    /// This method is called automatically by the Options system.
    /// </summary>
    /// <param name="options">The CorsOptions instance to configure.</param>
    public void Configure(CorsOptions options)
    {
        try
        {
            var allowedOrigins = _configuration.GetSection("AllowedOrigins").Get<List<string>>() ?? [];
            var allowedIPs = _configuration.GetSection("AllowedIPs").Get<List<string>>() ?? [];

            if (_environment.IsDevelopment())
            {
                if (allowedOrigins.Count > 0 && !allowedOrigins.Contains("localhost", StringComparer.OrdinalIgnoreCase))
                {
                    allowedOrigins.Add("localhost");
                }
            }

            options.AddDefaultPolicy(policyBuilder =>
            {
                policyBuilder.SetIsOriginAllowed(origin =>
                {
                    if (string.IsNullOrEmpty(origin))
                        return false;

                    if (Uri.TryCreate(origin, UriKind.Absolute, out Uri? uri))
                    {
                        var host = uri.Host;

                        if (IPAddress.TryParse(host, out IPAddress? ipAddress))
                            return allowedIPs.Any(ip => IPAddress.TryParse(ip, out var configuredIp) && configuredIp.Equals(ipAddress));
                        else
                            return allowedOrigins.Any(ao => string.Equals(ao, host, StringComparison.OrdinalIgnoreCase));
                    }
                    return false;
                })
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials();
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error configuring CORS options: {Message}", ex.Message);
        }
    }
}