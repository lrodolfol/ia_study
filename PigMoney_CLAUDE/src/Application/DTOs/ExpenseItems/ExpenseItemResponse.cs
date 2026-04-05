//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
namespace Application.DTOs.ExpenseItems;

public record ExpenseItemResponse(int Id, string Name, decimal Quantity, decimal UnitPrice, int ExpenseId, DateTime CreatedAt, DateTime UpdatedAt);
