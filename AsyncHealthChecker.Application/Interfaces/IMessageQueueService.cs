using AsyncHealthChecker.Domain.Entities;

namespace AsyncHealthChecker.Application.Services.Interfaces;

public interface IMessageQueueService
{
    Task PublishTask(TaskQueueMessage message);
    Task<TaskQueueMessage?> DequeueTask(CancellationToken cancellationToken);
}