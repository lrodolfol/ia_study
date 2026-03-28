//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
namespace Application.Tests;

using Application.DTOs.Expenses;

public class ExpenseFilterParamsTests
{
    [Fact]
    public void DefaultValues_AreSetCorrectly()
    {
        var filterParams = new ExpenseFilterParams();

        Assert.Equal(1, filterParams.Page);
        Assert.Equal(50, filterParams.PageSize);
        Assert.Null(filterParams.StartDate);
        Assert.Null(filterParams.EndDate);
        Assert.Null(filterParams.CategoryId);
        Assert.Null(filterParams.AccountId);
    }

    [Fact]
    public void CustomValues_AreSetCorrectly()
    {
        DateTime startDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        DateTime endDate = new DateTime(2024, 12, 31, 0, 0, 0, DateTimeKind.Utc);
        var filterParams = new ExpenseFilterParams(startDate, endDate, 1, 2, 3, 25);

        Assert.Equal(startDate, filterParams.StartDate);
        Assert.Equal(endDate, filterParams.EndDate);
        Assert.Equal(1, filterParams.CategoryId);
        Assert.Equal(2, filterParams.AccountId);
        Assert.Equal(3, filterParams.Page);
        Assert.Equal(25, filterParams.PageSize);
    }
}
