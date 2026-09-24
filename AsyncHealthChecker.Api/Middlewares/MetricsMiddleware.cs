using System.Diagnostics;
using AsyncHealthChecker.Application.Services.Implementations;

namespace AsyncHealthChecker.Api.Middlewares;

public class MetricsMiddleware
{
    private readonly RequestDelegate _next;
    
    public MetricsMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();

            var endpoint = context.GetEndpoint()?.DisplayName ?? context.Request.Path;

            MetricsService.HttpRequests
                .WithLabels(
                    context.Request.Method,
                    endpoint)
                .Inc();

            MetricsService.HttpRequestDuration
                .WithLabels(
                    context.Request.Method,
                    endpoint)
                .Observe(stopwatch.Elapsed.TotalSeconds);
        }
    }
    
}