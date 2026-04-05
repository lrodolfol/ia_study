//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
namespace Application.DTOs.ExpenseItems;

public record UpdateExpenseItemRequest(string Name, decimal Quantity, decimal UnitPrice);
