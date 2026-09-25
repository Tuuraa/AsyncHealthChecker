using System.Text.Json.Serialization;
using AsyncHealthChecker.Api.Workers;
using AsyncHealthChecker.Application.Interfaces;
using AsyncHealthChecker.Application.Services;
using AsyncHealthChecker.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using StackExchange.Redis;

namespace AsyncHealthChecker.Api.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<ITaskService, TaskService>();
        services.AddSingleton<IMessageQueueService, RedisMessageQueueService>();
        
        services.AddHttpClient();
        
        var redisConnection = configuration.GetConnectionString("Redis");

        services.AddSingleton<IConnectionMultiplexer>(sp =>
            ConnectionMultiplexer.Connect(redisConnection!));
        
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection")));
        
        services
            .AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(
                    new JsonStringEnumConverter()
                );
            });
        
        services.AddHostedService<HealthCheckWorker>();
        
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Async Health Checker API",
                Version = "v1"
            });
        });
        
        services.Configure<HostOptions>(options =>
        {
            options.ShutdownTimeout = TimeSpan.FromSeconds(35);
        });
        
        return services;
    }
}