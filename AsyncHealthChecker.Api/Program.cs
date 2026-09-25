using AsyncHealthChecker.Api.Extensions;
using AsyncHealthChecker.Api.Middlewares;
using AsyncHealthChecker.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Prometheus;

namespace AsyncHealthChecker.Api;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Logging.AddApplicationLogging();
        builder.Services.AddApplicationServices(builder.Configuration);

        var app = builder.Build();

        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await db.Database.MigrateAsync();
        }
        
        app.UseSwagger();

        app.UseSwaggerUI(options =>
        {
            options.RoutePrefix = "docs";
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1");
        });

        app.UseMiddleware<MetricsMiddleware>();
        
        app.UseAuthorization();

        app.MapControllers();
        app.MapMetrics();
     
        app.Run();
    }
}
