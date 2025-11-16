using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace TaskTracker.IntegrationTests;

public class ApiTestFixture : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;

    public ApiTestFixture()
    {
        _baseUrl = Environment.GetEnvironmentVariable("TASKTRACKER_API_URL") ?? "http://localhost:5000";
        _httpClient = new HttpClient { BaseAddress = new Uri(_baseUrl) };
    }

    public HttpClient CreateClient()
    {
        return _httpClient;
    }

    public async Task<string> GetAuthTokenAsync()
    {
        // Register a test user
        var testEmail = $"test{Guid.NewGuid():N}@example.com";
        var registerRequest = new
        {
            email = testEmail,
            password = "TestPass123!",
            confirmPassword = "TestPass123!",
            firstName = "Test",
            lastName = "User"
        };

        var registerJson = JsonSerializer.Serialize(registerRequest);
        var registerContent = new StringContent(registerJson, Encoding.UTF8, "application/json");
        
        var registerResponse = await _httpClient.PostAsync("/api/auth/register", registerContent);
        registerResponse.EnsureSuccessStatusCode();

        // Login to get token
        var loginRequest = new
        {
            email = registerRequest.email,
            password = registerRequest.password
        };

        var loginJson = JsonSerializer.Serialize(loginRequest);
        var loginContent = new StringContent(loginJson, Encoding.UTF8, "application/json");
        
        var loginResponse = await _httpClient.PostAsync("/api/auth/login", loginContent);
        loginResponse.EnsureSuccessStatusCode();

        var loginResponseContent = await loginResponse.Content.ReadAsStringAsync();
        var loginResult = JsonSerializer.Deserialize<JsonElement>(loginResponseContent);
        
        return loginResult.GetProperty("token").GetString()!;
    }

    public HttpClient CreateAuthenticatedClient(string token)
    {
        var client = new HttpClient { BaseAddress = new Uri(_baseUrl) };
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
    }
}