//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
namespace Application.Tests;

using Application.DTOs.Expenses;
using Application.Validators;
using FluentValidation.TestHelper;

public class CreateExpenseRequestValidatorTests
{
    private readonly CreateExpenseRequestValidator _validator = new();

    [Fact]
    public void Validate_WhenAmountIsNegative_ShouldHaveValidationError()
    {
        var request = new CreateExpenseRequest(-1, DateTime.UtcNow, 1, 1, null, null);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Amount);
    }

    [Fact]
    public void Validate_WhenAllFieldsAreValid_ShouldNotHaveValidationError()
    {
        var request = new CreateExpenseRequest(100, DateTime.UtcNow, 1, 1, "Test", "Notes");

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WhenAmountIsZero_ShouldPass()
    {
        var request = new CreateExpenseRequest(0, DateTime.UtcNow, 1, 1, null, null);

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(x => x.Amount);
    }

    [Fact]
    public void Validate_WhenCategoryIdIsZero_ShouldHaveValidationError()
    {
        var request = new CreateExpenseRequest(100, DateTime.UtcNow, 0, 1, null, null);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.CategoryId);
    }

    [Fact]
    public void Validate_WhenAccountIdIsZero_ShouldHaveValidationError()
    {
        var request = new CreateExpenseRequest(100, DateTime.UtcNow, 1, 0, null, null);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.AccountId);
    }
}
