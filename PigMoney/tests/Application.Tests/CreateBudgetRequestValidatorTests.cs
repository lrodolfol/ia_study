//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
namespace Application.Tests;

using Application.DTOs.Budgets;
using Application.Validators;
using FluentValidation.TestHelper;

public class CreateBudgetRequestValidatorTests
{
    private readonly CreateBudgetRequestValidator _validator = new();

    [Fact]
    public void Validate_WhenEndDateIsLessThanStartDate_ShouldHaveValidationError()
    {
        DateTime startDate = new DateTime(2024, 6, 15, 0, 0, 0, DateTimeKind.Utc);
        DateTime endDate = new DateTime(2024, 6, 10, 0, 0, 0, DateTimeKind.Utc);
        var request = new CreateBudgetRequest(1, 1000, startDate, endDate);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.EndDate);
    }

    [Fact]
    public void Validate_WhenEndDateEqualsStartDate_ShouldHaveValidationError()
    {
        DateTime date = new DateTime(2024, 6, 15, 0, 0, 0, DateTimeKind.Utc);
        var request = new CreateBudgetRequest(1, 1000, date, date);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.EndDate);
    }

    [Fact]
    public void Validate_WhenEndDateIsGreaterThanStartDate_ShouldPass()
    {
        DateTime startDate = new DateTime(2024, 6, 1, 0, 0, 0, DateTimeKind.Utc);
        DateTime endDate = new DateTime(2024, 6, 30, 0, 0, 0, DateTimeKind.Utc);
        var request = new CreateBudgetRequest(1, 1000, startDate, endDate);

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(x => x.EndDate);
    }

    [Fact]
    public void Validate_WhenLimitAmountIsNegative_ShouldHaveValidationError()
    {
        var request = new CreateBudgetRequest(1, -100, DateTime.UtcNow, DateTime.UtcNow.AddDays(30));

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.LimitAmount);
    }
}
