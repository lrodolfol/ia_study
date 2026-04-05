//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
using API.Models;

namespace pigMoney.Tests.API.Models;

public class StandardApiResponseTests
{
    [Fact]
    public void Constructor_ShouldSetAllProperties()
    {
        var errors = new List<string> { "error1", "error2" };
        var data = new { Name = "Test" };

        var response = new StandardApiResponse<object>(200, "Success", errors, data);

        Assert.Equal(200, response.StatusCode);
        Assert.Equal("Success", response.Message);
        Assert.Equal(errors, response.Error);
        Assert.Equal(data, response.Data);
    }

    [Fact]
    public void Constructor_ShouldAcceptNullData()
    {
        var response = new StandardApiResponse<string>(404, "Not found", Enumerable.Empty<string>(), null);

        Assert.Equal(404, response.StatusCode);
        Assert.Null(response.Data);
    }

    [Fact]
    public void Constructor_ShouldAcceptEmptyErrorCollection()
    {
        var response = new StandardApiResponse<int>(200, "Success", Enumerable.Empty<string>(), 42);

        Assert.Empty(response.Error);
        Assert.Equal(42, response.Data);
    }

    [Fact]
    public void StatusCode_ShouldBeInt()
    {
        var response = new StandardApiResponse<object>(500, "Error", new[] { "fail" }, null);

        Assert.IsType<int>(response.StatusCode);
    }

    [Fact]
    public void Message_ShouldBeString()
    {
        var response = new StandardApiResponse<object>(200, "OK", Enumerable.Empty<string>(), null);

        Assert.IsType<string>(response.Message);
    }

    [Fact]
    public void Error_ShouldBeEnumerableOfString()
    {
        var response = new StandardApiResponse<object>(400, "Bad", new[] { "err" }, null);

        Assert.IsAssignableFrom<IEnumerable<string>>(response.Error);
    }

    [Fact]
    public void Record_ShouldSupportValueEquality()
    {
        var errors = Array.Empty<string>();
        var response1 = new StandardApiResponse<int>(200, "OK", errors, 1);
        var response2 = new StandardApiResponse<int>(200, "OK", errors, 1);

        Assert.Equal(response1, response2);
    }
}
