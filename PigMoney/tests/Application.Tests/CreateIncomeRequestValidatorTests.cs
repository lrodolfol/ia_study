//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
namespace Application.Tests;

using Application.DTOs.Incomes;
using Application.Validators;
using FluentValidation.TestHelper;

public class CreateIncomeRequestValidatorTests
{
    private readonly CreateIncomeRequestValidator _validator = new();

    [Fact]
    public void Validate_WhenRequiredFieldsMissing_ShouldHaveValidationErrors()
    {
        var request = new CreateIncomeRequest(100, default, 0, 0, null, null);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Date);
        result.ShouldHaveValidationErrorFor(x => x.CategoryId);
        result.ShouldHaveValidationErrorFor(x => x.AccountId);
    }

    [Fact]
    public void Validate_WhenAllFieldsAreValid_ShouldNotHaveValidationError()
    {
        var request = new CreateIncomeRequest(1000, DateTime.UtcNow, 1, 1, "Salary", null);

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
