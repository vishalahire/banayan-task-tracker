using System.Net;
using System.Text;
using System.Text.Json;

namespace TaskTracker.IntegrationTests;

public class TasksApiTests : IClassFixture<ApiTestFixture>
{
    private readonly ApiTestFixture _fixture;

    public TasksApiTests(ApiTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task HealthCheck_ReturnsOk()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var response = await client.GetAsync("/health");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Healthy", content);
    }

    [Fact]
    public async Task AuthApi_RegisterAndLogin_WorksCorrectly()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var testEmail = $"auth-test-{Guid.NewGuid():N}@example.com";
        
        var registerRequest = new
        {
            email = testEmail,
            password = "TestPass123!",
            confirmPassword = "TestPass123!",
            firstName = "Integration",
            lastName = "Test"
        };

        // Act & Assert - Register
        var registerJson = JsonSerializer.Serialize(registerRequest);
        var registerContent = new StringContent(registerJson, Encoding.UTF8, "application/json");
        
        var registerResponse = await client.PostAsync("/api/auth/register", registerContent);
        Assert.Equal(HttpStatusCode.OK, registerResponse.StatusCode);

        // Act & Assert - Login
        var loginRequest = new
        {
            email = testEmail,
            password = "TestPass123!"
        };
        
        var loginJson = JsonSerializer.Serialize(loginRequest);
        var loginContent = new StringContent(loginJson, Encoding.UTF8, "application/json");
        
        var loginResponse = await client.PostAsync("/api/auth/login", loginContent);
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
        
        var loginResponseContent = await loginResponse.Content.ReadAsStringAsync();
        var loginResult = JsonSerializer.Deserialize<JsonElement>(loginResponseContent);
        
        Assert.True(loginResult.TryGetProperty("token", out var tokenProperty));
        Assert.False(string.IsNullOrEmpty(tokenProperty.GetString()));
    }

    [Fact]
    public async Task TasksApi_CreateFetchDelete_WorksCorrectly()
    {
        // Arrange
        var token = await _fixture.GetAuthTokenAsync();
        var client = _fixture.CreateAuthenticatedClient(token);

        var createTaskRequest = new
        {
            title = "Integration Test Task",
            description = "This is a test task created by integration test",
            priority = "Medium",
            dueDate = DateTimeOffset.UtcNow.AddDays(1).ToString("O"),
            tags = new[] { "test", "integration" }
        };

        // Act & Assert - Create Task
        var createJson = JsonSerializer.Serialize(createTaskRequest);
        var createContent = new StringContent(createJson, Encoding.UTF8, "application/json");
        
        var createResponse = await client.PostAsync("/api/tasks", createContent);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var createResponseContent = await createResponse.Content.ReadAsStringAsync();
        var createdTask = JsonSerializer.Deserialize<JsonElement>(createResponseContent);
        var taskId = createdTask.GetProperty("id").GetString();
        Assert.NotNull(taskId);

        // Act & Assert - Fetch Task
        var fetchResponse = await client.GetAsync($"/api/tasks/{taskId}");
        Assert.Equal(HttpStatusCode.OK, fetchResponse.StatusCode);

        var fetchResponseContent = await fetchResponse.Content.ReadAsStringAsync();
        var fetchedTask = JsonSerializer.Deserialize<JsonElement>(fetchResponseContent);
        Assert.Equal(createTaskRequest.title, fetchedTask.GetProperty("title").GetString());
        Assert.Equal(createTaskRequest.description, fetchedTask.GetProperty("description").GetString());

        // Act & Assert - Delete Task
        var deleteResponse = await client.DeleteAsync($"/api/tasks/{taskId}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        // Verify task is deleted
        var fetchAfterDeleteResponse = await client.GetAsync($"/api/tasks/{taskId}");
        Assert.Equal(HttpStatusCode.NotFound, fetchAfterDeleteResponse.StatusCode);
    }

    [Fact]
    public async Task RemindersApi_GetPendingReminders_ReturnsOk()
    {
        // Arrange
        var token = await _fixture.GetAuthTokenAsync();
        var client = _fixture.CreateAuthenticatedClient(token);

        // Act
        var response = await client.GetAsync("/api/reminders/pending");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var reminders = JsonSerializer.Deserialize<JsonElement>(content);
        Assert.True(reminders.ValueKind == JsonValueKind.Array);
    }
}