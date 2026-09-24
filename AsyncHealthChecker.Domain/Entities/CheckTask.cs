using AsyncHealthChecker.Domain.Enums;

namespace AsyncHealthChecker.Domain.Entities;

public class CheckTask
{
    public Guid TaskId { get; set; }
    
    public CheckTaskStatus Status { get; set; } = CheckTaskStatus.Queued;
    
    public int UrlsCount { get; set; }
    
    public DateTime CreatedAt { get; set; }
}