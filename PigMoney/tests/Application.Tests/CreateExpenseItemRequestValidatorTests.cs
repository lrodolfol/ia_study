//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
namespace Application.Tests;

using Application.DTOs.ExpenseItems;
using Application.Validators;
using FluentValidation.TestHelper;

public class CreateExpenseItemRequestValidatorTests
{
    private readonly CreateExpenseItemRequestValidator _validator = new();

    [Fact]
    public void Validate_WhenAmountIsNegative_ShouldHaveValidationError()
    {
        var request = new CreateExpenseItemRequest(1, -10, "Test Item", null);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Amount);
    }

    [Fact]
    public void Validate_WhenAllFieldsAreValid_ShouldNotHaveValidationError()
    {
        var request = new CreateExpenseItemRequest(1, 50, "Test Item", null);

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WhenDescriptionIsEmpty_ShouldHaveValidationError()
    {
        var request = new CreateExpenseItemRequest(1, 50, string.Empty, null);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Description);
    }
}
