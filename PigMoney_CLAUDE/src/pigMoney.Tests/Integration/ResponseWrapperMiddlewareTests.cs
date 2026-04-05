//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Application.DTOs.Accounts;
using Domain.Enums;

namespace pigMoney.Tests.Integration;

public class ResponseWrapperMiddlewareTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public ResponseWrapperMiddlewareTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task SuccessResponse_ShouldHaveCorrectEnvelopeShape()
    {
        var request = new CreateAccountRequest("Envelope Test", AccountType.Cash, 100m);
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/accounts", request);

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        JsonElement root = doc.RootElement;

        Assert.True(root.TryGetProperty("statusCode", out JsonElement statusCode));
        Assert.True(root.TryGetProperty("message", out JsonElement message));
        Assert.True(root.TryGetProperty("error", out JsonElement error));
        Assert.True(root.TryGetProperty("data", out JsonElement data));

        Assert.Equal(201, statusCode.GetInt32());
        Assert.Equal("Success", message.GetString());
        Assert.Equal(JsonValueKind.Array, error.ValueKind);
        Assert.Equal(0, error.GetArrayLength());
        Assert.NotEqual(JsonValueKind.Null, data.ValueKind);
    }

    [Fact]
    public async Task NotFoundResponse_ShouldHaveErrorInEnvelope()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/v1/accounts/9999");

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        JsonElement root = doc.RootElement;

        Assert.True(root.TryGetProperty("statusCode", out JsonElement statusCode));
        Assert.Equal(404, statusCode.GetInt32());
        Assert.True(root.TryGetProperty("message", out JsonElement message));
        Assert.Equal("Resource not found", message.GetString());
        Assert.True(root.TryGetProperty("error", out JsonElement error));
        Assert.True(error.GetArrayLength() > 0);
    }

    [Fact]
    public async Task ValidationErrorResponse_ShouldHaveErrorArrayPopulated()
    {
        var request = new CreateAccountRequest("", AccountType.Checking, -1m);
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/v1/accounts", request);

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        JsonElement root = doc.RootElement;

        Assert.True(root.TryGetProperty("statusCode", out JsonElement statusCode));
        Assert.Equal(400, statusCode.GetInt32());
        Assert.True(root.TryGetProperty("message", out JsonElement message));
        Assert.Equal("Validation failed", message.GetString());
        Assert.True(root.TryGetProperty("error", out JsonElement error));
        Assert.True(error.GetArrayLength() > 0);
        Assert.True(root.TryGetProperty("data", out JsonElement data));
        Assert.Equal(JsonValueKind.Null, data.ValueKind);
    }

    [Fact]
    public async Task OkResponse_ShouldHaveSuccessMessage()
    {
        var request = new CreateAccountRequest("OK Test", AccountType.Savings, 50m);
        HttpResponseMessage createResponse = await _client.PostAsJsonAsync("/api/v1/accounts", request);
        var json = await createResponse.Content.ReadAsStringAsync();
        using var createDoc = JsonDocument.Parse(json);
        int id = createDoc.RootElement.GetProperty("data").GetProperty("id").GetInt32();

        HttpResponseMessage response = await _client.GetAsync($"/api/v1/accounts/{id}");

        var responseJson = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(responseJson);
        JsonElement root = doc.RootElement;

        Assert.Equal(200, root.GetProperty("statusCode").GetInt32());
        Assert.Equal("Success", root.GetProperty("message").GetString());
        Assert.Equal(0, root.GetProperty("error").GetArrayLength());
        Assert.NotEqual(JsonValueKind.Null, root.GetProperty("data").ValueKind);
    }

    [Fact]
    public async Task ConflictResponse_ShouldHaveConflictMessage()
    {
        var accountRequest = new CreateAccountRequest("Conflict Test", AccountType.Cash, 5000m);
        HttpResponseMessage accResponse = await _client.PostAsJsonAsync("/api/v1/accounts", accountRequest);
        var accJson = await accResponse.Content.ReadAsStringAsync();
        using var accDoc = JsonDocument.Parse(accJson);
        int accountId = accDoc.RootElement.GetProperty("data").GetProperty("id").GetInt32();

        var incomeRequest = new { Amount = 1000m, Date = DateTime.UtcNow, Description = "Salary", AccountId = accountId };
        await _client.PostAsJsonAsync("/api/v1/incomes", incomeRequest);

        HttpResponseMessage response = await _client.DeleteAsync($"/api/v1/accounts/{accountId}");

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        JsonElement root = doc.RootElement;

        Assert.Equal(409, root.GetProperty("statusCode").GetInt32());
        Assert.Equal("Conflict", root.GetProperty("message").GetString());
        Assert.True(root.GetProperty("error").GetArrayLength() > 0);
    }

    [Fact]
    public async Task ResponseContentType_ShouldBeJson()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/v1/accounts?page=1&pageSize=10");

        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
    }
}
