//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
using System.Text.Json;
using API.Models;

namespace pigMoney.Tests.Integration;

public class StandardApiResponseTests
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    [Fact]
    public void StandardApiResponse_ShouldSerializeCorrectly()
    {
        var response = new StandardApiResponse<string>(200, "Success", Enumerable.Empty<string>(), "test data");

        var json = JsonSerializer.Serialize(response, JsonOptions);
        using var doc = JsonDocument.Parse(json);
        JsonElement root = doc.RootElement;

        Assert.Equal(200, root.GetProperty("statusCode").GetInt32());
        Assert.Equal("Success", root.GetProperty("message").GetString());
        Assert.Equal(JsonValueKind.Array, root.GetProperty("error").ValueKind);
        Assert.Equal(0, root.GetProperty("error").GetArrayLength());
        Assert.Equal("test data", root.GetProperty("data").GetString());
    }

    [Fact]
    public void StandardApiResponse_WithErrors_ShouldSerializeErrorArray()
    {
        var errors = new[] { "Field is required", "Invalid value" };
        var response = new StandardApiResponse<object>(400, "Validation failed", errors, default);

        var json = JsonSerializer.Serialize(response, JsonOptions);
        using var doc = JsonDocument.Parse(json);
        JsonElement root = doc.RootElement;

        Assert.Equal(400, root.GetProperty("statusCode").GetInt32());
        Assert.Equal("Validation failed", root.GetProperty("message").GetString());
        Assert.Equal(2, root.GetProperty("error").GetArrayLength());
        Assert.Equal("Field is required", root.GetProperty("error")[0].GetString());
        Assert.Equal("Invalid value", root.GetProperty("error")[1].GetString());
        Assert.Equal(JsonValueKind.Null, root.GetProperty("data").ValueKind);
    }

    [Fact]
    public void StandardApiResponse_WithNullData_ShouldSerializeDataAsNull()
    {
        var response = new StandardApiResponse<object>(404, "Resource not found", new[] { "Not found" }, default);

        var json = JsonSerializer.Serialize(response, JsonOptions);
        using var doc = JsonDocument.Parse(json);

        Assert.Equal(JsonValueKind.Null, doc.RootElement.GetProperty("data").ValueKind);
    }

    [Fact]
    public void StandardApiResponse_WithComplexData_ShouldSerializeDataCorrectly()
    {
        var data = new { Id = 1, Name = "Test" };
        var response = new StandardApiResponse<object>(200, "Success", Enumerable.Empty<string>(), data);

        var json = JsonSerializer.Serialize(response, JsonOptions);
        using var doc = JsonDocument.Parse(json);
        JsonElement dataElement = doc.RootElement.GetProperty("data");

        Assert.Equal(1, dataElement.GetProperty("id").GetInt32());
        Assert.Equal("Test", dataElement.GetProperty("name").GetString());
    }

    [Fact]
    public void StandardApiResponse_ShouldDeserializeCorrectly()
    {
        var json = """{"statusCode":200,"message":"Success","error":[],"data":"hello"}""";

        StandardApiResponse<string>? response = JsonSerializer.Deserialize<StandardApiResponse<string>>(json, JsonOptions);

        Assert.NotNull(response);
        Assert.Equal(200, response.StatusCode);
        Assert.Equal("Success", response.Message);
        Assert.Empty(response.Error);
        Assert.Equal("hello", response.Data);
    }
}
