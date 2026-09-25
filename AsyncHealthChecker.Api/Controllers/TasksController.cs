using AsyncHealthChecker.Api.DTOs;
using AsyncHealthChecker.Application.DTOs;
using AsyncHealthChecker.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AsyncHealthChecker.Api.Controllers;

[ApiController]
[Route("api/v1/task")]
public class TasksController: ControllerBase
{
    private readonly ITaskService _taskService;
    
    public TasksController(ITaskService taskService)
    {
        _taskService = taskService;
    }
    
    [HttpPost]
    public async Task<IActionResult> CreateTask([FromBody] CreateTaskRequest request)
    {
        try
        {
            var result = await _taskService.CreateTask(request.Urls);

            return CreatedAtAction(nameof(CreateTask), result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
    
    [HttpGet("{taskId:guid}")]
    public async Task<IActionResult> GetTask(Guid taskId)
    {
        var result = await _taskService.GetTaskResult(taskId);
        if (result is null) return NotFound();
        return Ok(result);
    }
}