//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
using Application.DTOs.Budgets;
using Application.Validators;
using FluentValidation.TestHelper;

namespace pigMoney.Tests.Application.Validators;

public class CreateBudgetRequestValidatorTests
{
    private readonly CreateBudgetRequestValidator _validator = new();

    [Fact]
    public void ShouldHaveError_WhenLimitAmountIsZero()
    {
        var result = _validator.TestValidate(new CreateBudgetRequest(1, DateTime.UtcNow, DateTime.UtcNow.AddMonths(1), 0));
        result.ShouldHaveValidationErrorFor(x => x.LimitAmount);
    }

    [Fact]
    public void ShouldHaveError_WhenLimitAmountIsNegative()
    {
        var result = _validator.TestValidate(new CreateBudgetRequest(1, DateTime.UtcNow, DateTime.UtcNow.AddMonths(1), -100));
        result.ShouldHaveValidationErrorFor(x => x.LimitAmount);
    }

    [Fact]
    public void ShouldHaveError_WhenCategoryIdIsZero()
    {
        var result = _validator.TestValidate(new CreateBudgetRequest(0, DateTime.UtcNow, DateTime.UtcNow.AddMonths(1), 1000m));
        result.ShouldHaveValidationErrorFor(x => x.CategoryId);
    }

    [Fact]
    public void ShouldHaveError_WhenStartDateIsDefault()
    {
        var result = _validator.TestValidate(new CreateBudgetRequest(1, default, DateTime.UtcNow.AddMonths(1), 1000m));
        result.ShouldHaveValidationErrorFor(x => x.StartDate);
    }

    [Fact]
    public void ShouldHaveError_WhenEndDateIsDefault()
    {
        var result = _validator.TestValidate(new CreateBudgetRequest(1, DateTime.UtcNow, default, 1000m));
        result.ShouldHaveValidationErrorFor(x => x.EndDate);
    }

    [Fact]
    public void ShouldHaveError_WhenEndDateIsBeforeStartDate()
    {
        var startDate = DateTime.UtcNow;
        var endDate = startDate.AddDays(-1);
        var result = _validator.TestValidate(new CreateBudgetRequest(1, startDate, endDate, 1000m));
        result.ShouldHaveValidationErrorFor(x => x.EndDate);
    }

    [Fact]
    public void ShouldHaveError_WhenEndDateEqualsStartDate()
    {
        var date = DateTime.UtcNow;
        var result = _validator.TestValidate(new CreateBudgetRequest(1, date, date, 1000m));
        result.ShouldHaveValidationErrorFor(x => x.EndDate);
    }

    [Fact]
    public void ShouldNotHaveError_WhenLimitAmountIsMinimumValid()
    {
        var result = _validator.TestValidate(new CreateBudgetRequest(1, DateTime.UtcNow, DateTime.UtcNow.AddMonths(1), 0.01m));
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void ShouldNotHaveError_WhenRequestIsValid()
    {
        var result = _validator.TestValidate(new CreateBudgetRequest(1, DateTime.UtcNow, DateTime.UtcNow.AddMonths(1), 5000m));
        result.ShouldNotHaveAnyValidationErrors();
    }
}
