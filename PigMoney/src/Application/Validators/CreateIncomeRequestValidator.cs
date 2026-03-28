//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
namespace Application.Validators;

using Application.DTOs.Incomes;
using FluentValidation;

public class CreateIncomeRequestValidator : AbstractValidator<CreateIncomeRequest>
{
    public CreateIncomeRequestValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Amount must be greater than or equal to 0");

        RuleFor(x => x.Date)
            .NotEmpty()
            .WithMessage("Date is required");

        RuleFor(x => x.CategoryId)
            .GreaterThan(0)
            .WithMessage("CategoryId must be greater than 0");

        RuleFor(x => x.AccountId)
            .GreaterThan(0)
            .WithMessage("AccountId must be greater than 0");
    }
}
