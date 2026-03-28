//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
namespace E2ETests;

using System.Text.Json;

public class ApiResponse<T>
{
    public T? Data { get; set; }
    public List<string>? Errors { get; set; }
    public string? Message { get; set; }
    public int StatusCode { get; set; }
}

public static class TestHelpers
{
    public static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };
}
