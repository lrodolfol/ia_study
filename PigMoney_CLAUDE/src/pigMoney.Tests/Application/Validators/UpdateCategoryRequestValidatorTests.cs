//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
using Application.DTOs.Categories;
using Application.Validators;
using FluentValidation.TestHelper;

namespace pigMoney.Tests.Application.Validators;

public class UpdateCategoryRequestValidatorTests
{
    private readonly UpdateCategoryRequestValidator _validator = new();

    [Fact]
    public void ShouldHaveError_WhenNameIsEmpty()
    {
        var result = _validator.TestValidate(new UpdateCategoryRequest(""));
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void ShouldHaveError_WhenNameExceedsMaxLength()
    {
        var result = _validator.TestValidate(new UpdateCategoryRequest(new string('A', 101)));
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void ShouldNotHaveError_WhenRequestIsValid()
    {
        var result = _validator.TestValidate(new UpdateCategoryRequest("Updated", "New desc"));
        result.ShouldNotHaveAnyValidationErrors();
    }
}
