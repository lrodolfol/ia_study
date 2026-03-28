//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
namespace Application.Validators;

using Application.DTOs.ExpenseItems;
using FluentValidation;

public class UpdateExpenseItemRequestValidator : AbstractValidator<UpdateExpenseItemRequest>
{
    public UpdateExpenseItemRequestValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThanOrEqualTo(0)
            .When(x => x.Amount.HasValue)
            .WithMessage("Amount must be greater than or equal to 0");

        RuleFor(x => x.Description)
            .MaximumLength(200)
            .When(x => !string.IsNullOrEmpty(x.Description))
            .WithMessage("Description must not exceed 200 characters");

        RuleFor(x => x.CategoryId)
            .GreaterThan(0)
            .When(x => x.CategoryId.HasValue)
            .WithMessage("CategoryId must be greater than 0");
    }
}
