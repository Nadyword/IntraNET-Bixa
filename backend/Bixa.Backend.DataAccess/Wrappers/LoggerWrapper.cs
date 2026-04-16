using Microsoft.Extensions.Logging;

namespace Bixa.Backend.DataAccess.Wrappers;

public class LoggerWrapper
{
    private readonly ILoggerFactory _loggerFactory;

    public LoggerWrapper(ILoggerFactory loggerFactory)
    {
        _loggerFactory = loggerFactory;
    }

    public virtual ILogger<T> CreateLogger<T>() =>
         _loggerFactory.CreateLogger<T>();
}