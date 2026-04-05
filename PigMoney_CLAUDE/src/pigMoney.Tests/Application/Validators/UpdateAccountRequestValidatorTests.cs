//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
using Application.DTOs.Accounts;
using Application.Validators;
using Domain.Enums;
using FluentValidation.TestHelper;

namespace pigMoney.Tests.Application.Validators;

public class UpdateAccountRequestValidatorTests
{
    private readonly UpdateAccountRequestValidator _validator = new();

    [Fact]
    public void ShouldHaveError_WhenNameIsEmpty()
    {
        var result = _validator.TestValidate(new UpdateAccountRequest("", AccountType.Checking, 100));
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void ShouldHaveError_WhenBalanceIsNegative()
    {
        var result = _validator.TestValidate(new UpdateAccountRequest("Account", AccountType.Checking, -1));
        result.ShouldHaveValidationErrorFor(x => x.Balance);
    }

    [Fact]
    public void ShouldHaveError_WhenBalanceIsMinimalNegative()
    {
        var result = _validator.TestValidate(new UpdateAccountRequest("Account", AccountType.Checking, -0.01m));
        result.ShouldHaveValidationErrorFor(x => x.Balance);
    }

    [Fact]
    public void ShouldHaveError_WhenNameExceedsMaxLength()
    {
        var result = _validator.TestValidate(new UpdateAccountRequest(new string('A', 101), AccountType.Checking, 100));
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void ShouldHaveError_WhenAccountTypeIsInvalid()
    {
        var result = _validator.TestValidate(new UpdateAccountRequest("Account", (AccountType)99, 100));
        result.ShouldHaveValidationErrorFor(x => x.Type);
    }

    [Fact]
    public void ShouldNotHaveError_WhenRequestIsValid()
    {
        var result = _validator.TestValidate(new UpdateAccountRequest("Updated", AccountType.Credit, 200));
        result.ShouldNotHaveAnyValidationErrors();
    }
}
