//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
using Application.DTOs.Accounts;
using Application.Validators;
using Domain.Enums;
using FluentValidation.TestHelper;

namespace pigMoney.Tests.Application.Validators;

public class CreateAccountRequestValidatorTests
{
    private readonly CreateAccountRequestValidator _validator = new();

    [Fact]
    public void ShouldHaveError_WhenNameIsEmpty()
    {
        var result = _validator.TestValidate(new CreateAccountRequest("", AccountType.Checking, 100));
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void ShouldHaveError_WhenNameExceedsMaxLength()
    {
        var result = _validator.TestValidate(new CreateAccountRequest(new string('A', 101), AccountType.Checking, 100));
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void ShouldHaveError_WhenBalanceIsNegative()
    {
        var result = _validator.TestValidate(new CreateAccountRequest("Account", AccountType.Checking, -1));
        result.ShouldHaveValidationErrorFor(x => x.Balance);
    }

    [Fact]
    public void ShouldHaveError_WhenAccountTypeIsInvalid()
    {
        var result = _validator.TestValidate(new CreateAccountRequest("Account", (AccountType)99, 100));
        result.ShouldHaveValidationErrorFor(x => x.Type);
    }

    [Fact]
    public void ShouldNotHaveError_WhenBalanceIsZero()
    {
        var result = _validator.TestValidate(new CreateAccountRequest("Account", AccountType.Cash, 0));
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void ShouldNotHaveError_WhenRequestIsValid()
    {
        var result = _validator.TestValidate(new CreateAccountRequest("My Account", AccountType.Savings, 500));
        result.ShouldNotHaveAnyValidationErrors();
    }
}
