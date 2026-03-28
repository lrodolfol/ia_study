//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
namespace Domain.Tests;

using Domain.Common;

public class ResultTests
{
    [Fact]
    public void Result_Success_CreatesSuccessfulResult()
    {
        var result = Result.Success();

        Assert.True(result.IsSuccess);
        Assert.Equal(string.Empty, result.Error);
    }

    [Fact]
    public void Result_Failure_CreatesFailedResultWithErrorMessage()
    {
        string errorMessage = "Test error message";

        var result = Result.Failure(errorMessage);

        Assert.False(result.IsSuccess);
        Assert.Equal(errorMessage, result.Error);
    }

    [Fact]
    public void ResultT_Success_CreatesSuccessfulResultWithValue()
    {
        string testValue = "test value";

        var result = Result<string>.Success(testValue);

        Assert.True(result.IsSuccess);
        Assert.Equal(testValue, result.Value);
        Assert.Equal(string.Empty, result.Error);
    }

    [Fact]
    public void ResultT_Failure_CreatesFailedResultWithErrorMessage()
    {
        string errorMessage = "Test error message";

        var result = Result<string>.Failure(errorMessage);

        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Equal(errorMessage, result.Error);
    }

    [Fact]
    public void ResultT_Success_WithIntValue_ReturnsCorrectValue()
    {
        int testValue = 42;

        var result = Result<int>.Success(testValue);

        Assert.True(result.IsSuccess);
        Assert.Equal(testValue, result.Value);
    }

    [Fact]
    public void ResultT_Success_WithComplexType_ReturnsCorrectValue()
    {
        var testValue = new TestObject { Id = 1, Name = "Test" };

        var result = Result<TestObject>.Success(testValue);

        Assert.True(result.IsSuccess);
        Assert.Equal(testValue.Id, result.Value!.Id);
        Assert.Equal(testValue.Name, result.Value.Name);
    }

    private class TestObject
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
