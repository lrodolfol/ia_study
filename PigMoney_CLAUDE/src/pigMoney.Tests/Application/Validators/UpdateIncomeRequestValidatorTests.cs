//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
using Application.DTOs.Incomes;
using Application.Validators;
using FluentValidation.TestHelper;

namespace pigMoney.Tests.Application.Validators;

public class UpdateIncomeRequestValidatorTests
{
    private readonly UpdateIncomeRequestValidator _validator = new();

    [Fact]
    public void ShouldHaveError_WhenAmountIsZero()
    {
        var result = _validator.TestValidate(new UpdateIncomeRequest(0, DateTime.UtcNow, "Test", 1));
        result.ShouldHaveValidationErrorFor(x => x.Amount);
    }

    [Fact]
    public void ShouldHaveError_WhenAmountIsNegative()
    {
        var result = _validator.TestValidate(new UpdateIncomeRequest(-5, DateTime.UtcNow, "Test", 1));
        result.ShouldHaveValidationErrorFor(x => x.Amount);
    }

    [Fact]
    public void ShouldHaveError_WhenAccountIdIsZero()
    {
        var result = _validator.TestValidate(new UpdateIncomeRequest(100, DateTime.UtcNow, "Test", 0));
        result.ShouldHaveValidationErrorFor(x => x.AccountId);
    }

    [Fact]
    public void ShouldHaveError_WhenDescriptionIsEmpty()
    {
        var result = _validator.TestValidate(new UpdateIncomeRequest(100, DateTime.UtcNow, "", 1));
        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void ShouldNotHaveError_WhenRequestIsValid()
    {
        var result = _validator.TestValidate(new UpdateIncomeRequest(2000m, DateTime.UtcNow, "Updated salary", 1));
        result.ShouldNotHaveAnyValidationErrors();
    }
}
