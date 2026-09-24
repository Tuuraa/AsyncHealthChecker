namespace AsyncHealthChecker.Domain.Entities;

public class TaskQueueMessage
{
    public Guid TaskId { get; set; }
    public List<string> Urls { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}