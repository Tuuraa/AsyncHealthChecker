using System.Diagnostics;
using AsyncHealthChecker.Application.Services.Implementations;
using AsyncHealthChecker.Application.Services.Interfaces;
using AsyncHealthChecker.Domain.Entities;
using AsyncHealthChecker.Domain.Enums;
using AsyncHealthChecker.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AsyncHealthChecker.Api.Workers;

public class HealthCheckWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<HealthCheckWorker> _logger;
    
    private readonly SemaphoreSlim _semaphore = new (20);
    
    private static readonly TimeSpan PollDelay = TimeSpan.FromSeconds(1);
    
    public HealthCheckWorker(
        IServiceProvider serviceProvider,
        ILogger<HealthCheckWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        MetricsService.ActiveWorkers.Set(1);

        _logger.LogInformation(
            "HealthCheckWorker started, active_workers = 1");

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var queue = scope.ServiceProvider.GetRequiredService<IMessageQueueService>();

                    var message = await queue.DequeueTask(stoppingToken);

                    if (message is null)
                    {
                        await Task.Delay(PollDelay, stoppingToken);
                        continue;
                    }

                    await ProcessTask(message, stoppingToken);

                    MetricsService.TasksProcessed.Inc();
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unhandled error in worker loop");
                }
            }
        }
        finally
        {
            MetricsService.ActiveWorkers.Set(0);

            _logger.LogInformation(
                "HealthCheckWorker stopped, active_workers = 0");
        }
    }
    
    private async Task ProcessTask(TaskQueueMessage message, CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var httpClientFactory = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>();
        
        _logger.LogInformation(
            "Processing task {TaskId} with {UrlCount} urls",
            message.TaskId, message.Urls.Count);

        var currentTask = await db.CheckTasks
            .FirstOrDefaultAsync(c => c.TaskId == message.TaskId, stoppingToken);

        if (currentTask is null) return;

        try
        {
            currentTask.Status = CheckTaskStatus.Processing;
            await db.SaveChangesAsync(stoppingToken);
        
            var tasks = message.Urls
                .Select(url => CheckUrlAsync(url, message.TaskId, httpClientFactory, stoppingToken))
                .ToList();

            while (tasks.Count > 0)
            {
                var finished = await Task.WhenAny(tasks);
                tasks.Remove(finished);

                var result = await finished;

                MetricsService.UrlsChecked
                    .WithLabels(result.IsAvailable ? "true" : "false")
                    .Inc();
                
                db.CheckResults.Add(result);
                await db.SaveChangesAsync(stoppingToken);
            }
        
            currentTask.Status = CheckTaskStatus.Completed;
            await db.SaveChangesAsync(stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Task {TaskId} failed", message.TaskId);

            currentTask.Status = CheckTaskStatus.Failed; 
            await db.SaveChangesAsync(CancellationToken.None);
        }
        
    }

    private async Task<CheckResult> CheckUrlAsync(
        string url,
        Guid taskId,
        IHttpClientFactory httpClientFactory,
        CancellationToken stoppingToken)
    {
        await _semaphore.WaitAsync(stoppingToken);
        
        var client = httpClientFactory.CreateClient();
        client.Timeout = TimeSpan.FromSeconds(10);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            var response = await client.GetAsync(url, stoppingToken);

            stopwatch.Stop();

            return new CheckResult
            {
                TaskId = taskId,
                Url = url,
                StatusCode = (int)response.StatusCode,
                ResponseTime = stopwatch.Elapsed.TotalMilliseconds,
                IsAvailable = response.IsSuccessStatusCode,
                ErrorMessage = null,
                CheckedAt = DateTime.UtcNow
            };
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            _logger.LogWarning(
                "Failed to check {Url} for task {TaskId}: {Error}",
                url, taskId, ex.Message);

            return new CheckResult
            {
                TaskId = taskId,
                Url = url,
                StatusCode = null,
                ResponseTime = null,
                IsAvailable = false,
                ErrorMessage = ex.Message,
                CheckedAt = DateTime.UtcNow
            };
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public override void Dispose()
    {
        _semaphore.Dispose();
        base.Dispose();
    }
}