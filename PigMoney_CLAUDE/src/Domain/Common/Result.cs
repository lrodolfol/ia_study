//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
namespace Domain.Common;

public record Result<T>
{
    public T? Value { get; init; }
    public string Error { get; init; } = string.Empty;
    public bool IsSuccess => string.Equals(Error, string.Empty, StringComparison.Ordinal);
    public static Result<T> Success(T value) => new() { Value = value };
    public static Result<T> Failure(string error) => new() { Error = error };
}
