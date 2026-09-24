using AsyncHealthChecker.Domain.Enums;

namespace AsyncHealthChecker.Api.DTOs.Responses;

public class CheckTaskResponse
{
    public Guid TaskId { get; set; }

    public CheckTaskStatus Status { get; set; } = CheckTaskStatus.Processing;

    public int TotalUrls { get; set; }

    public int ProcessedUrls { get; set; }

    public List<UrlCheckResponse> Results { get; set; } = new();
}

public class UrlCheckResponse
{
    public string Url { get; set; } = string.Empty;

    public int? StatusCode { get; set; }

    public double? ResponseTime { get; set; }

    public bool IsAvailable { get; set; }

    public DateTime CheckedAt { get; set; }
}