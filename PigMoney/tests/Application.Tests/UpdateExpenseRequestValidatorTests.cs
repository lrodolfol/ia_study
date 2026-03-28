//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
namespace Application.Tests;

using Application.DTOs.Expenses;
using Application.Validators;
using FluentValidation.TestHelper;

public class UpdateExpenseRequestValidatorTests
{
    private readonly UpdateExpenseRequestValidator _validator = new();

    [Fact]
    public void Validate_WhenAllFieldsAreNull_ShouldNotHaveValidationError()
    {
        var request = new UpdateExpenseRequest(null, null, null, null, null, null);

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WhenAmountIsNegative_ShouldHaveValidationError()
    {
        var request = new UpdateExpenseRequest(-1, null, null, null, null, null);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Amount);
    }

    [Fact]
    public void Validate_WhenAmountIsPositive_ShouldPass()
    {
        var request = new UpdateExpenseRequest(100, null, null, null, null, null);

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(x => x.Amount);
    }
}
