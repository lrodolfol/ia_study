//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
namespace Domain.Tests;

using Domain.Entities;

public class EntityStringPropertyTests
{
    [Fact]
    public void Expense_StringProperties_DefaultToEmpty()
    {
        var expense = new Expense();

        Assert.Equal(string.Empty, expense.Description);
        Assert.Equal(string.Empty, expense.Notes);
    }

    [Fact]
    public void Income_StringProperties_DefaultToEmpty()
    {
        var income = new Income();

        Assert.Equal(string.Empty, income.Description);
        Assert.Equal(string.Empty, income.Notes);
    }

    [Fact]
    public void Category_StringProperties_DefaultToEmpty()
    {
        var category = new Category();

        Assert.Equal(string.Empty, category.Name);
    }

    [Fact]
    public void Account_StringProperties_DefaultToEmpty()
    {
        var account = new Account();

        Assert.Equal(string.Empty, account.Name);
    }

    [Fact]
    public void ExpenseItem_StringProperties_DefaultToEmpty()
    {
        var expenseItem = new ExpenseItem();

        Assert.Equal(string.Empty, expenseItem.Description);
    }
}
