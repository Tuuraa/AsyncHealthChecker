using AsyncHealthChecker.Api.DTOs.Responses;
using AsyncHealthChecker.Domain.Entities;

namespace AsyncHealthChecker.Application.Services.Interfaces;

public interface ITaskService
{
    Task<CheckTask> CreateTask(List<string> urls);
    Task<CheckTaskResponse?> GetTaskResult(Guid taskId);
}