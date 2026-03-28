//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
namespace Domain.Common;

public class Result<T>(bool isSuccess, T? value, string error)
{
    public bool IsSuccess { get; } = isSuccess;
    public T? Value { get; } = value;
    public string Error { get; } = error;

    public static Result<T> Success(T value) => new(true, value, string.Empty);
    public static Result<T> Failure(string error) => new(false, default, error);
}

public class Result(bool isSuccess, string error)
{
    public bool IsSuccess { get; } = isSuccess;
    public string Error { get; } = error;

    public static Result Success() => new(true, string.Empty);
    public static Result Failure(string error) => new(false, error);
}
