//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
namespace Application.Tests;

using Application.DTOs.Accounts;
using Application.Validators;
using Domain.Enums;
using FluentValidation.TestHelper;

public class CreateAccountRequestValidatorTests
{
    private readonly CreateAccountRequestValidator _validator = new();

    [Fact]
    public void Validate_WhenNameIsEmpty_ShouldHaveValidationError()
    {
        var request = new CreateAccountRequest(string.Empty, AccountType.Checking, 0);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Validate_WhenNameIsProvided_ShouldNotHaveValidationError()
    {
        var request = new CreateAccountRequest("Main Checking", AccountType.Checking, 1000);

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
