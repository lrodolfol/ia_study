//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
namespace Application.Tests;

using Application.DTOs.Categories;
using Application.Validators;
using Domain.Enums;
using FluentValidation.TestHelper;

public class CreateCategoryRequestValidatorTests
{
    private readonly CreateCategoryRequestValidator _validator = new();

    [Fact]
    public void Validate_WhenNameIsEmpty_ShouldHaveValidationError()
    {
        var request = new CreateCategoryRequest(string.Empty, TransactionType.Expense);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Validate_WhenNameIsProvided_ShouldNotHaveValidationError()
    {
        var request = new CreateCategoryRequest("Food", TransactionType.Expense);

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WhenNameExceeds100Characters_ShouldHaveValidationError()
    {
        string longName = new string('a', 101);
        var request = new CreateCategoryRequest(longName, TransactionType.Expense);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Name);
    }
}
