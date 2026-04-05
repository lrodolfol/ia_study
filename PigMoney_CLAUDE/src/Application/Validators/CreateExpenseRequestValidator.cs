//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
using Application.DTOs.Expenses;
using FluentValidation;

namespace Application.Validators;

public class CreateExpenseRequestValidator : AbstractValidator<CreateExpenseRequest>
{
    public CreateExpenseRequestValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than 0.");

        RuleFor(x => x.Date)
            .NotEmpty().WithMessage("Date is required.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required.")
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters.");

        RuleFor(x => x.AccountId)
            .GreaterThan(0).WithMessage("AccountId must be greater than 0.");

        RuleFor(x => x.CategoryId)
            .GreaterThan(0).WithMessage("CategoryId must be greater than 0.");
    }
}
