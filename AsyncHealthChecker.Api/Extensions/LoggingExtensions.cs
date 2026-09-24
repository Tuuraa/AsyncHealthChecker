using System.Text.Json;

namespace AsyncHealthChecker.Api.Extensions;

public static class LoggingExtensions
{
    public static ILoggingBuilder AddApplicationLogging(this ILoggingBuilder logging)
    {
        logging.ClearProviders();
        logging.AddJsonConsole(options =>
        {
            options.IncludeScopes = true;
            options.TimestampFormat = "yyyy-MM-ddTHH:mm:ss.fffZ";
            options.JsonWriterOptions = new JsonWriterOptions()
            {
                Indented = true
            };
        });
        
        logging.AddFilter(
            "Microsoft.EntityFrameworkCore.Database.Command",
            LogLevel.Warning);
        
        return logging;
    }
}