//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
namespace Application.Validators;

using Application.DTOs.Categories;
using FluentValidation;

public class UpdateCategoryRequestValidator : AbstractValidator<UpdateCategoryRequest>
{
    public UpdateCategoryRequestValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(100)
            .When(x => !string.IsNullOrEmpty(x.Name))
            .WithMessage("Name must not exceed 100 characters");

        RuleFor(x => x.Type)
            .IsInEnum()
            .When(x => x.Type.HasValue)
            .WithMessage("Type must be a valid TransactionType");
    }
}
