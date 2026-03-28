//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
namespace Application.DTOs.Categories;

using Domain.Enums;

public record UpdateCategoryRequest(
    string? Name,
    TransactionType? Type
);
