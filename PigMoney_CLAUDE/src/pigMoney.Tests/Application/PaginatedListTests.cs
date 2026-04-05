//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
using Application.DTOs.Common;

namespace pigMoney.Tests.Application;

public class PaginatedListTests
{
    [Fact]
    public void Constructor_ShouldSetItemsCorrectly()
    {
        var items = new List<string> { "a", "b", "c" };

        var paginated = new PaginatedList<string>(items, 10, 1, 5);

        Assert.Equal(3, paginated.Items.Count());
        Assert.Contains("a", paginated.Items);
        Assert.Contains("b", paginated.Items);
        Assert.Contains("c", paginated.Items);
    }

    [Fact]
    public void Constructor_ShouldSetTotalCountCorrectly()
    {
        var paginated = new PaginatedList<int>([], 42, 1, 10);

        Assert.Equal(42, paginated.TotalCount);
    }

    [Fact]
    public void Constructor_ShouldSetPageCorrectly()
    {
        var paginated = new PaginatedList<int>([], 100, 3, 10);

        Assert.Equal(3, paginated.Page);
    }

    [Fact]
    public void Constructor_ShouldSetPageSizeCorrectly()
    {
        var paginated = new PaginatedList<int>([], 100, 1, 25);

        Assert.Equal(25, paginated.PageSize);
    }

    [Fact]
    public void Constructor_WithEmptyItems_ShouldReturnEmptyCollection()
    {
        var paginated = new PaginatedList<string>([], 0, 1, 10);

        Assert.Empty(paginated.Items);
        Assert.Equal(0, paginated.TotalCount);
    }

    [Fact]
    public void Record_ShouldSupportEquality()
    {
        var items = new List<int> { 1, 2, 3 };
        var paginated1 = new PaginatedList<int>(items, 10, 1, 5);
        var paginated2 = new PaginatedList<int>(items, 10, 1, 5);

        Assert.Equal(paginated1, paginated2);
    }

    [Fact]
    public void Record_ShouldSupportInequality()
    {
        var items = new List<int> { 1, 2, 3 };
        var paginated1 = new PaginatedList<int>(items, 10, 1, 5);
        var paginated2 = new PaginatedList<int>(items, 20, 2, 5);

        Assert.NotEqual(paginated1, paginated2);
    }
}
