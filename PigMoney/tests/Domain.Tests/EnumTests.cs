//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
namespace Domain.Tests;

using Domain.Enums;

public class EnumTests
{
    [Fact]
    public void TransactionType_Expense_HasValueZero()
    {
        Assert.Equal(0, (int)TransactionType.Expense);
    }

    [Fact]
    public void TransactionType_Income_HasValueOne()
    {
        Assert.Equal(1, (int)TransactionType.Income);
    }

    [Fact]
    public void AccountType_Checking_HasValueZero()
    {
        Assert.Equal(0, (int)AccountType.Checking);
    }

    [Fact]
    public void AccountType_Savings_HasValueOne()
    {
        Assert.Equal(1, (int)AccountType.Savings);
    }

    [Fact]
    public void AccountType_CreditCard_HasValueTwo()
    {
        Assert.Equal(2, (int)AccountType.CreditCard);
    }

    [Fact]
    public void AccountType_Cash_HasValueThree()
    {
        Assert.Equal(3, (int)AccountType.Cash);
    }
}
