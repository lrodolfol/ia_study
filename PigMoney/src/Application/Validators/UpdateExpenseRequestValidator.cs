//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
namespace Application.Validators;

using Application.DTOs.Expenses;
using FluentValidation;

public class UpdateExpenseRequestValidator : AbstractValidator<UpdateExpenseRequest>
{
    public UpdateExpenseRequestValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThanOrEqualTo(0)
            .When(x => x.Amount.HasValue)
            .WithMessage("Amount must be greater than or equal to 0");

        RuleFor(x => x.CategoryId)
            .GreaterThan(0)
            .When(x => x.CategoryId.HasValue)
            .WithMessage("CategoryId must be greater than 0");

        RuleFor(x => x.AccountId)
            .GreaterThan(0)
            .When(x => x.AccountId.HasValue)
            .WithMessage("AccountId must be greater than 0");
    }
}
