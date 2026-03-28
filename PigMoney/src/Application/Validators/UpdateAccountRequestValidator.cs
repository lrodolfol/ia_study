//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
namespace Application.Validators;

using Application.DTOs.Accounts;
using FluentValidation;

public class UpdateAccountRequestValidator : AbstractValidator<UpdateAccountRequest>
{
    public UpdateAccountRequestValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(100)
            .When(x => !string.IsNullOrEmpty(x.Name))
            .WithMessage("Name must not exceed 100 characters");

        RuleFor(x => x.Type)
            .IsInEnum()
            .When(x => x.Type.HasValue)
            .WithMessage("Type must be a valid AccountType");
    }
}
