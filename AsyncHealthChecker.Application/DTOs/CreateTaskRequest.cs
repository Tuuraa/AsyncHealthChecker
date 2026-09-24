using System.ComponentModel.DataAnnotations;

namespace AsyncHealthChecker.Application.DTOs;

public class CreateTaskRequest
{
    [MinLength(1)]
    public List<string> Urls { get; set; } = new List<string>();
}