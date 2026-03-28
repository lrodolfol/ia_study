//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
namespace API.Models;

public record StandardApiResponse<T>(
    T? Data,
    List<string> Errors,
    string Message,
    int StatusCode
)
{
    public static StandardApiResponse<T> Success(T? data, string message = "Success", int statusCode = 200) =>
        new(data, [], message, statusCode);

    public static StandardApiResponse<T> Failure(string error, string message = "Error", int statusCode = 400) =>
        new(default, [error], message, statusCode);

    public static StandardApiResponse<T> Failure(List<string> errors, string message = "Error", int statusCode = 400) =>
        new(default, errors, message, statusCode);
}
