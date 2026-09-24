using System.Net;
using System.Net.Http.Json;
using AsyncHealthChecker.Tests.Fixtures;

namespace AsyncHealthChecker.Tests.Controllers;

public class TasksControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public TasksControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateTask_ValidRequest_ReturnsCreated()
    {
        var request = new
        {
            urls = new[]
            {
                "https://google.com",
                "https://github.com"
            }
        };

        var response = await _client.PostAsJsonAsync(
            "/api/v1/task",
            request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<CreateTaskResponse>();

        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.TaskId);
    }

    [Fact]
    public async Task CreateTask_InvalidUrl_ReturnsBadRequest()
    {
        var request = new
        {
            urls = new[]
            {
                "not-a-valid-url"
            }
        };

        var response = await _client.PostAsJsonAsync(
            "/api/v1/task",
            request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateTask_EmptyUrls_ReturnsBadRequest()
    {
        var request = new
        {
            urls = Array.Empty<string>()
        };

        var response = await _client.PostAsJsonAsync(
            "/api/v1/task",
            request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateTask_MultipleUrls_ReturnsCreated()
    {
        var request = new
        {
            urls = new[]
            {
                "https://google.com",
                "https://github.com",
                "https://microsoft.com"
            }
        };

        var response = await _client.PostAsJsonAsync(
            "/api/v1/task",
            request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<CreateTaskResponse>();

        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.TaskId);
    }

    private sealed class CreateTaskResponse
    {
        public Guid TaskId { get; set; }
    }
}