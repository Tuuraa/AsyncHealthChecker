namespace AsyncHealthChecker.Domain.Entities;

public class CheckResult
{
    public int Id { get; set; } 
    
    public Guid TaskId { get; set; }
    
    public string Url { get; set; }
    
    public int? StatusCode { get; set; }
    
    public double? ResponseTime { get; set; }
    
    public bool IsAvailable { get; set; }
    
    public string? ErrorMessage { get; set; }
    
    public DateTime CheckedAt { get; set; }
}