using Microsoft.EntityFrameworkCore.Design;
using Bixa.Backend.DataAccess.Context;
using Microsoft.EntityFrameworkCore;

namespace Bixa.Backend.Base;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    private readonly string archivo = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development"
        ? "appsettings.Development.json"
        : "appsettings.json";

    public AppDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile(archivo, optional: false, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("WebApiDatabase");

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseSqlServer(connectionString,
            b => b.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName));

        return new AppDbContext(optionsBuilder.Options);
    }
}