//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
namespace API.Models;

public record StandardApiResponse<T>(int StatusCode, string Message, IEnumerable<string> Error, T? Data);
