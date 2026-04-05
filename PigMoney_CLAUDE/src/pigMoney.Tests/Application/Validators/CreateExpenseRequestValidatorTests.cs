//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
using Application.DTOs.Expenses;
using Application.Validators;
using FluentValidation.TestHelper;

namespace pigMoney.Tests.Application.Validators;

public class CreateExpenseRequestValidatorTests
{
    private readonly CreateExpenseRequestValidator _validator = new();

    [Fact]
    public void ShouldHaveError_WhenAmountIsZero()
    {
        var result = _validator.TestValidate(new CreateExpenseRequest(0, DateTime.UtcNow, "Test", 1, 1));
        result.ShouldHaveValidationErrorFor(x => x.Amount);
    }

    [Fact]
    public void ShouldHaveError_WhenAmountIsNegative()
    {
        var result = _validator.TestValidate(new CreateExpenseRequest(-1, DateTime.UtcNow, "Test", 1, 1));
        result.ShouldHaveValidationErrorFor(x => x.Amount);
    }

    [Fact]
    public void ShouldHaveError_WhenAccountIdIsZero()
    {
        var result = _validator.TestValidate(new CreateExpenseRequest(100, DateTime.UtcNow, "Test", 0, 1));
        result.ShouldHaveValidationErrorFor(x => x.AccountId);
    }

    [Fact]
    public void ShouldHaveError_WhenCategoryIdIsZero()
    {
        var result = _validator.TestValidate(new CreateExpenseRequest(100, DateTime.UtcNow, "Test", 1, 0));
        result.ShouldHaveValidationErrorFor(x => x.CategoryId);
    }

    [Fact]
    public void ShouldHaveError_WhenDescriptionIsEmpty()
    {
        var result = _validator.TestValidate(new CreateExpenseRequest(100, DateTime.UtcNow, "", 1, 1));
        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void ShouldHaveError_WhenDescriptionExceedsMaxLength()
    {
        var result = _validator.TestValidate(new CreateExpenseRequest(100, DateTime.UtcNow, new string('A', 501), 1, 1));
        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void ShouldNotHaveError_WhenAmountIsMinimumValid()
    {
        var result = _validator.TestValidate(new CreateExpenseRequest(0.01m, DateTime.UtcNow, "Small expense", 1, 1));
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void ShouldNotHaveError_WhenRequestIsValid()
    {
        var result = _validator.TestValidate(new CreateExpenseRequest(250m, DateTime.UtcNow, "Groceries", 1, 2));
        result.ShouldNotHaveAnyValidationErrors();
    }
}
