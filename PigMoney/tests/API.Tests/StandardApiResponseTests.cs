//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
namespace API.Tests;

using API.Models;

public class StandardApiResponseTests
{
    [Fact]
    public void SuccessCreatesResponseWithEmptyErrors()
    {
        object data = new { id = 1, name = "Test" };

        StandardApiResponse<object> response = StandardApiResponse<object>.Success(data, "OK", 200);

        Assert.Equal(data, response.Data);
        Assert.Empty(response.Errors);
        Assert.Equal("OK", response.Message);
        Assert.Equal(200, response.StatusCode);
    }

    [Fact]
    public void FailureCreatesResponseWithError()
    {
        StandardApiResponse<object> response = StandardApiResponse<object>.Failure("Test error", "Error", 400);

        Assert.Null(response.Data);
        Assert.Single(response.Errors);
        Assert.Equal("Test error", response.Errors[0]);
        Assert.Equal("Error", response.Message);
        Assert.Equal(400, response.StatusCode);
    }

    [Fact]
    public void FailureWithMultipleErrorsCreatesResponseWithAllErrors()
    {
        List<string> errors = ["Error 1", "Error 2", "Error 3"];

        StandardApiResponse<object> response = StandardApiResponse<object>.Failure(errors, "Multiple Errors", 422);

        Assert.Null(response.Data);
        Assert.Equal(3, response.Errors.Count);
        Assert.Contains("Error 1", response.Errors);
        Assert.Contains("Error 2", response.Errors);
        Assert.Contains("Error 3", response.Errors);
        Assert.Equal("Multiple Errors", response.Message);
        Assert.Equal(422, response.StatusCode);
    }
}
