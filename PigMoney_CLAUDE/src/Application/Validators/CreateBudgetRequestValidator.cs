//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
using Application.DTOs.Budgets;
using FluentValidation;

namespace Application.Validators;

public class CreateBudgetRequestValidator : AbstractValidator<CreateBudgetRequest>
{
    public CreateBudgetRequestValidator()
    {
        RuleFor(x => x.CategoryId)
            .GreaterThan(0).WithMessage("CategoryId must be greater than 0.");

        RuleFor(x => x.LimitAmount)
            .GreaterThan(0).WithMessage("LimitAmount must be greater than 0.");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("StartDate is required.");

        RuleFor(x => x.EndDate)
            .NotEmpty().WithMessage("EndDate is required.");

        RuleFor(x => x.EndDate)
            .GreaterThan(x => x.StartDate).WithMessage("EndDate must be greater than StartDate.");
    }
}
