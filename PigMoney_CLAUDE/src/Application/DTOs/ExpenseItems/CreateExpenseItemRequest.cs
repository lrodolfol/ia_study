//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
namespace Application.DTOs.ExpenseItems;

public record CreateExpenseItemRequest(string Name, decimal Quantity, decimal UnitPrice);
