using AsyncHealthChecker.Api.DTOs.Responses;
using AsyncHealthChecker.Application.Interfaces;
using AsyncHealthChecker.Domain.Entities;
using AsyncHealthChecker.Domain.Enums;
using AsyncHealthChecker.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AsyncHealthChecker.Application.Services;

public class TaskService : ITaskService
{
    private readonly ILogger<TaskService> _logger;
    private readonly IMessageQueueService _queue;
    private readonly AppDbContext _db;
    
    public TaskService(
        ILogger<TaskService> logger, 
        IMessageQueueService queue,
        AppDbContext db)
    {
        _logger = logger;
        _queue = queue;
        _db = db;
    }
    
    public async Task<CheckTask> CreateTask(List<string> urls)
    {
        if (urls is null || urls.Count == 0)
            throw new ArgumentException("urls list cannot be empty");
        
        foreach (var url in urls)
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) ||
                (uri.Scheme != Uri.UriSchemeHttp &&
                 uri.Scheme != Uri.UriSchemeHttps))
            {
                throw new ArgumentException($"Invalid URL: {url}");
            }
        }
        
        var taskId = Guid.NewGuid();
        var createdAt = DateTime.UtcNow;

        var message = new TaskQueueMessage
        {
            TaskId = taskId,
            Urls = urls,
            CreatedAt = createdAt
        };
        
        var checkTask = new CheckTask
        {
            TaskId = taskId,
            UrlsCount = urls.Count,
            CreatedAt = createdAt
        };

        await _db.CheckTasks.AddAsync(checkTask);
        await _db.SaveChangesAsync();
        
        await _queue.PublishTask(message);
        
        _logger.LogInformation(
            "Task {TaskId} queued with {UrlCount} urls",
            taskId, urls.Count);

        return checkTask;
    }

    public async Task<CheckTaskResponse?> GetTaskResult(Guid taskId)
    {
        var task = await _db.CheckTasks
            .FirstOrDefaultAsync(t => t.TaskId == taskId);

        if (task is null) return null;

        var processedUrls = await _db.CheckResults
            .Where(r => r.TaskId == taskId)
            .ToListAsync();

        var status = processedUrls.Count >= task.UrlsCount
            ? CheckTaskStatus.Completed
            : processedUrls.Count > 0
                ? CheckTaskStatus.Processing
                : CheckTaskStatus.Queued;

        return new CheckTaskResponse()
        {
            TaskId = taskId,
            Status = status,
            TotalUrls = task.UrlsCount,
            ProcessedUrls = processedUrls.Count,
            Results = processedUrls.Select(p => new UrlCheckResponse()
            {
                Url = p.Url,
                StatusCode = p.StatusCode,
                ResponseTime = p.ResponseTime,
                IsAvailable = p.IsAvailable,
                CheckedAt = p.CheckedAt
            }).ToList()
        };
    }
    
}