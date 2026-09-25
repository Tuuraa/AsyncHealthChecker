using System.Text.Json;
using AsyncHealthChecker.Application.Interfaces;
using AsyncHealthChecker.Domain.Entities;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace AsyncHealthChecker.Application.Services;

public class RedisMessageQueueService : IMessageQueueService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RedisMessageQueueService> _logger;
    
    private const string MainQueue = "health_check_tasks";
    private const string ProcessingQueue = "processing_tasks";
    
    private IDatabase Db => _redis.GetDatabase();
    
    public RedisMessageQueueService(
        IConnectionMultiplexer redis,
        ILogger<RedisMessageQueueService> logger)
    {
        _redis = redis;
        _logger = logger;
    }
    
    public async Task PublishTask(TaskQueueMessage message)
    {
        var payload = JsonSerializer.Serialize(message);
        await Db.ListLeftPushAsync(MainQueue, payload);

        _logger.LogInformation(
            "Published task {TaskId} to queue with {UrlCount} urls",
            message.TaskId, message.Urls.Count);
    }

    public async Task<TaskQueueMessage?> DequeueTask(CancellationToken cancellationToken)
    {
        var value = await Db.ListRightPopLeftPushAsync(MainQueue, ProcessingQueue);
        if (value.IsNullOrEmpty) return null;
        return JsonSerializer.Deserialize<TaskQueueMessage>(value!);
    }

    public async Task AcknowledgeTask(Guid taskId)
    {
        var items = await Db.ListRangeAsync(ProcessingQueue);
        foreach (var item in items)
        {
            var msg = JsonSerializer.Deserialize<TaskQueueMessage>(item!);
            if (msg?.TaskId == taskId)
            {
                await Db.ListRemoveAsync(ProcessingQueue, item, count: 1);
                break; 
            }
        }
    }

    public async Task RecoverAbandonedTasks()
    {
        while (await Db.ListLengthAsync(ProcessingQueue) > 0)
        {
            await Db.ListRightPopLeftPushAsync(ProcessingQueue, MainQueue);
        }
    }
}