//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
using Application.DTOs.ExpenseItems;
using Application.Validators;
using FluentValidation.TestHelper;

namespace pigMoney.Tests.Application.Validators;

public class UpdateExpenseItemRequestValidatorTests
{
    private readonly UpdateExpenseItemRequestValidator _validator = new();

    [Fact]
    public void ShouldHaveError_WhenNameIsEmpty()
    {
        var result = _validator.TestValidate(new UpdateExpenseItemRequest("", 2m, 5.50m));
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void ShouldHaveError_WhenNameExceedsMaxLength()
    {
        var result = _validator.TestValidate(new UpdateExpenseItemRequest(new string('A', 201), 2m, 5.50m));
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void ShouldHaveError_WhenQuantityIsZero()
    {
        var result = _validator.TestValidate(new UpdateExpenseItemRequest("Rice", 0, 5.50m));
        result.ShouldHaveValidationErrorFor(x => x.Quantity);
    }

    [Fact]
    public void ShouldHaveError_WhenQuantityIsNegative()
    {
        var result = _validator.TestValidate(new UpdateExpenseItemRequest("Rice", -1, 5.50m));
        result.ShouldHaveValidationErrorFor(x => x.Quantity);
    }

    [Fact]
    public void ShouldHaveError_WhenUnitPriceIsZero()
    {
        var result = _validator.TestValidate(new UpdateExpenseItemRequest("Rice", 2m, 0));
        result.ShouldHaveValidationErrorFor(x => x.UnitPrice);
    }

    [Fact]
    public void ShouldHaveError_WhenUnitPriceIsNegative()
    {
        var result = _validator.TestValidate(new UpdateExpenseItemRequest("Rice", 2m, -1));
        result.ShouldHaveValidationErrorFor(x => x.UnitPrice);
    }

    [Fact]
    public void ShouldNotHaveError_WhenQuantityIsMinimumValid()
    {
        var result = _validator.TestValidate(new UpdateExpenseItemRequest("Rice", 0.01m, 5.50m));
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void ShouldNotHaveError_WhenUnitPriceIsMinimumValid()
    {
        var result = _validator.TestValidate(new UpdateExpenseItemRequest("Rice", 2m, 0.01m));
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void ShouldNotHaveError_WhenRequestIsValid()
    {
        var result = _validator.TestValidate(new UpdateExpenseItemRequest("Rice", 2m, 5.50m));
        result.ShouldNotHaveAnyValidationErrors();
    }
}
