//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
namespace Domain.Common;

public record ExpenseFilterParams(DateTime? StartDate, DateTime? EndDate, int? AccountId, int? CategoryId);
