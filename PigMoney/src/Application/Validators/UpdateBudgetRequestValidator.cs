//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
namespace Application.Validators;

using Application.DTOs.Budgets;
using FluentValidation;

public class UpdateBudgetRequestValidator : AbstractValidator<UpdateBudgetRequest>
{
    public UpdateBudgetRequestValidator()
    {
        RuleFor(x => x.CategoryId)
            .GreaterThan(0)
            .When(x => x.CategoryId.HasValue)
            .WithMessage("CategoryId must be greater than 0");

        RuleFor(x => x.LimitAmount)
            .GreaterThanOrEqualTo(0)
            .When(x => x.LimitAmount.HasValue)
            .WithMessage("LimitAmount must be greater than or equal to 0");

        RuleFor(x => x.EndDate)
            .GreaterThan(x => x.StartDate!.Value)
            .When(x => x.StartDate.HasValue && x.EndDate.HasValue)
            .WithMessage("EndDate must be greater than StartDate");
    }
}
