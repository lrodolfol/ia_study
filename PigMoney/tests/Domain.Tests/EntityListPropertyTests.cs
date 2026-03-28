//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
namespace Domain.Tests;

using Domain.Entities;

public class EntityListPropertyTests
{
    [Fact]
    public void Expense_ExpenseItems_IsInitialized()
    {
        var expense = new Expense();

        Assert.NotNull(expense.ExpenseItems);
        Assert.Empty(expense.ExpenseItems);
    }

    [Fact]
    public void Category_Expenses_IsInitialized()
    {
        var category = new Category();

        Assert.NotNull(category.Expenses);
        Assert.Empty(category.Expenses);
    }

    [Fact]
    public void Category_Incomes_IsInitialized()
    {
        var category = new Category();

        Assert.NotNull(category.Incomes);
        Assert.Empty(category.Incomes);
    }

    [Fact]
    public void Category_Budgets_IsInitialized()
    {
        var category = new Category();

        Assert.NotNull(category.Budgets);
        Assert.Empty(category.Budgets);
    }

    [Fact]
    public void Account_Expenses_IsInitialized()
    {
        var account = new Account();

        Assert.NotNull(account.Expenses);
        Assert.Empty(account.Expenses);
    }

    [Fact]
    public void Account_Incomes_IsInitialized()
    {
        var account = new Account();

        Assert.NotNull(account.Incomes);
        Assert.Empty(account.Incomes);
    }
}
