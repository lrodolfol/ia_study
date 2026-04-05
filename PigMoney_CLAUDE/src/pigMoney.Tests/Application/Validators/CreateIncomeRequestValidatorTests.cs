//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
using Application.DTOs.Incomes;
using Application.Validators;
using FluentValidation.TestHelper;

namespace pigMoney.Tests.Application.Validators;

public class CreateIncomeRequestValidatorTests
{
    private readonly CreateIncomeRequestValidator _validator = new();

    [Fact]
    public void ShouldHaveError_WhenAmountIsZero()
    {
        var result = _validator.TestValidate(new CreateIncomeRequest(0, DateTime.UtcNow, "Test", 1));
        result.ShouldHaveValidationErrorFor(x => x.Amount);
    }

    [Fact]
    public void ShouldHaveError_WhenAmountIsNegative()
    {
        var result = _validator.TestValidate(new CreateIncomeRequest(-1, DateTime.UtcNow, "Test", 1));
        result.ShouldHaveValidationErrorFor(x => x.Amount);
    }

    [Fact]
    public void ShouldHaveError_WhenAccountIdIsZero()
    {
        var result = _validator.TestValidate(new CreateIncomeRequest(100, DateTime.UtcNow, "Test", 0));
        result.ShouldHaveValidationErrorFor(x => x.AccountId);
    }

    [Fact]
    public void ShouldHaveError_WhenDescriptionIsEmpty()
    {
        var result = _validator.TestValidate(new CreateIncomeRequest(100, DateTime.UtcNow, "", 1));
        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void ShouldHaveError_WhenDescriptionExceedsMaxLength()
    {
        var result = _validator.TestValidate(new CreateIncomeRequest(100, DateTime.UtcNow, new string('A', 501), 1));
        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void ShouldNotHaveError_WhenAmountIsMinimumValid()
    {
        var result = _validator.TestValidate(new CreateIncomeRequest(0.01m, DateTime.UtcNow, "Small income", 1));
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void ShouldNotHaveError_WhenRequestIsValid()
    {
        var result = _validator.TestValidate(new CreateIncomeRequest(1500m, DateTime.UtcNow, "Monthly salary", 1));
        result.ShouldNotHaveAnyValidationErrors();
    }
}
