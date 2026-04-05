//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
using Domain.Common;

namespace pigMoney.Tests;

public class ResultTests
{
    [Fact]
    public void Success_ShouldHaveIsSuccessTrue()
    {
        Result<string> result = Result<string>.Success("value");

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Success_ShouldHaveCorrectValue()
    {
        Result<int> result = Result<int>.Success(42);

        Assert.Equal(42, result.Value);
    }

    [Fact]
    public void Success_ShouldHaveEmptyError()
    {
        Result<string> result = Result<string>.Success("value");

        Assert.Equal(string.Empty, result.Error);
    }

    [Fact]
    public void Failure_ShouldHaveIsSuccessFalse()
    {
        Result<string> result = Result<string>.Failure("err");

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void Failure_ShouldHaveNullValue()
    {
        Result<string> result = Result<string>.Failure("err");

        Assert.Null(result.Value);
    }

    [Fact]
    public void Failure_ShouldHaveCorrectErrorMessage()
    {
        Result<string> result = Result<string>.Failure("something went wrong");

        Assert.Equal("something went wrong", result.Error);
    }
}
