using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Caching.Memory;
using IntranetCorp.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using IntranetCorp.Infrastructure.Data;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Identity;
using IntranetCorp.Domain.Entities;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Configuration
var jwtKey = builder.Configuration["Jwt:Key"] ?? "CHANGE_THIS_TO_A_SECURE_KEY_MIN_32_CHARS";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "IntranetCorp";
var connectionString = builder.Configuration.GetConnectionString("Default") ?? throw new InvalidOperationException("Connection string 'Default' not found.");
var VEnvironment  = builder.Configuration["environment"];
var PathDocumensClient = builder.Configuration["PathDocumentsClient"] ?? throw new InvalidOperationException("Configuration 'PathDocumentsClient' not found.");

// Add services to the container
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

// Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// Authentication JWT
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactClient", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:3000")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

// Services
builder.Services.AddScoped<IEmailService>(sp =>
    new EmailService("smtp.gmail.com", 587,
        builder.Configuration["Email:User"] ?? "",
        builder.Configuration["Email:AppPassword"] ?? ""));

builder.Services.AddScoped<IPdfService, PdfService>();
builder.Services.AddScoped<IChatbotService, ChatbotFaqService>();

var cacheTtl = int.TryParse(builder.Configuration["ExternalApi:CacheTtlMinutes"], out var ttl) ? ttl : 60;
builder.Services.AddScoped<IExternalApiService>(sp =>
    new ExternalApiService(new HttpClient(), sp.GetRequiredService<IMemoryCache>(), cacheTtl));

builder.Services.AddScoped<IFileStorageService>(sp =>
    new FileStorageService(PathDocumensClient));

builder.Services.AddMemoryCache();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Create directories
Directory.CreateDirectory(PathDocumensClient);

// Migrate database and seed initial data
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();

    // Seed roles and admin user
    await DataSeeder.SeedAsync(scope.ServiceProvider);
}

if (VEnvironment == "Development")
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowReactClient");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
