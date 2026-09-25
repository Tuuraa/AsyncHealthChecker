using AsyncHealthChecker.Domain.Entities;

namespace AsyncHealthChecker.Application.Interfaces;

public interface IMessageQueueService
{
    Task PublishTask(TaskQueueMessage message);
    Task<TaskQueueMessage?> DequeueTask(CancellationToken cancellationToken);
    Task AcknowledgeTask(Guid taskId);
    Task RecoverAbandonedTasks();
}