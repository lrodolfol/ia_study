//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
using Application.DTOs.ExpenseItems;
using FluentValidation;

namespace Application.Validators;

public class CreateExpenseItemRequestValidator : AbstractValidator<CreateExpenseItemRequest>
{
    public CreateExpenseItemRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than 0.");

        RuleFor(x => x.UnitPrice)
            .GreaterThan(0).WithMessage("UnitPrice must be greater than 0.");
    }
}
