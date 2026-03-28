//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
namespace Application.Validators;

using Application.DTOs.ExpenseItems;
using FluentValidation;

public class CreateExpenseItemRequestValidator : AbstractValidator<CreateExpenseItemRequest>
{
    public CreateExpenseItemRequestValidator()
    {
        RuleFor(x => x.ExpenseId)
            .GreaterThan(0)
            .WithMessage("ExpenseId must be greater than 0");

        RuleFor(x => x.Amount)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Amount must be greater than or equal to 0");

        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("Description is required")
            .MaximumLength(200)
            .WithMessage("Description must not exceed 200 characters");

        RuleFor(x => x.CategoryId)
            .GreaterThan(0)
            .When(x => x.CategoryId.HasValue)
            .WithMessage("CategoryId must be greater than 0");
    }
}
