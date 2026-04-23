using Bixa.Backend.Models.DTOs.UserModelDTO.Validators;
using Bixa.Backend.DataAccess.Context;
using Bixa.Backend.Controllers.Services;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using Bixa.Backend.Services.Mapper;
using FluentValidation.AspNetCore;
using Bixa.Backend.Configuration;
using FluentValidation;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

var connString = builder.Configuration.GetConnectionString("WebApiDatabase");
if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WebApiDatabase")))
    connString = Environment.GetEnvironmentVariable("WebApiDatabase");

builder.Services.AddDbContext<AppDbContext>(
    o => o.UseSqlServer(connString, x => x.MigrationsAssembly("Bixa.Backend.DataAccess"))
);

var ProfitConnString = builder.Configuration.GetConnectionString("DbProxi");
if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DbProxi")))
    ProfitConnString = Environment.GetEnvironmentVariable("DbProxi");

builder.Services.AddDbContext<ProfitDbContext>(
    o => o.UseSqlServer(ProfitConnString, x => x.MigrationsAssembly("Bixa.Backend.DataAccess"))
          .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
);

builder.Services.AddAutoMapper(_ => { }, typeof(BllMappingProfile).Assembly);

builder.Host.UseSerilog((hostContext, _, loggerConfiguration) => SerilogConfig.ConfigureSerilog(hostContext, loggerConfiguration), true);

builder.Services.AddApplicationServices();

builder.Services.ConfigureJwtAuthenticationAndServices(builder.Configuration);

builder.Services.AddControllers()
    .AddControllersAsServices()
    .AddJsonOptions(options => options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles)
    .ConfigureApiBehavior();

builder.Services.AddFluentValidationAutoValidation()
                .AddValidatorsFromAssembly(typeof(UserInsertDTOValidator).Assembly);

builder.Services.AddEndpointsApiExplorer();
builder.Services.ConfigureSwagger();

builder.Services.AddStandardPolicies();

var app = builder.Build();

var logger = app.Services.GetRequiredService<ILogger<Program>>();
var configuration = app.Services.GetRequiredService<IConfiguration>();

var appVersion = configuration["APP_VERSION"] ?? "LOCAL-DEBUG-NO-HASH";

logger.LogInformation("Bixa Backend API Initialized.");

app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Bixa.Backend API v1");
        c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
        c.RoutePrefix = string.Empty;
    });
}

app.UseRouting();
app.UseCors();

app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<CustomUnauthorizedMiddleware>();

app.MapControllers();
app.MapGet("/health", () => Results.Ok("Healthy"));

app.Run();