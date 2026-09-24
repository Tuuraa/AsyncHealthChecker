namespace AsyncHealthChecker.Domain.Enums;

public enum CheckTaskStatus
{
    Queued,
    Processing,
    Completed,
    Failed
}