using System.Text.Json;
using AsyncHealthChecker.Application.Services.Interfaces;
using AsyncHealthChecker.Domain.Entities;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace AsyncHealthChecker.Application.Services.Implementations;

public class RedisMessageQueueService : IMessageQueueService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RedisMessageQueueService> _logger;
    
    private const string QueueChannel = "health_check_tasks";
    
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
        await Db.ListLeftPushAsync(QueueChannel, payload);

        _logger.LogInformation(
            "Published task {TaskId} to queue with {UrlCount} urls",
            message.TaskId, message.Urls.Count);
    }

    public async Task<TaskQueueMessage?> DequeueTask(CancellationToken cancellationToken)
    {
        var value = await Db.ListRightPopAsync(QueueChannel);

        if (value.IsNullOrEmpty)
            return null;

        return JsonSerializer.Deserialize<TaskQueueMessage>(value!);
    }
}