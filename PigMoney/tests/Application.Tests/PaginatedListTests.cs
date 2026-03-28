//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
namespace Application.Tests;

using Application.DTOs.Common;

public class PaginatedListTests
{
    [Fact]
    public void TotalPages_WhenItemsFitExactly_ReturnsCorrectCount()
    {
        List<int> items = [1, 2, 3, 4, 5];
        var paginatedList = new PaginatedList<int>(items, 100, 1, 50);

        Assert.Equal(2, paginatedList.TotalPages);
    }

    [Fact]
    public void TotalPages_WhenItemsHaveRemainder_RoundsUp()
    {
        List<int> items = [1, 2, 3, 4, 5];
        var paginatedList = new PaginatedList<int>(items, 105, 1, 50);

        Assert.Equal(3, paginatedList.TotalPages);
    }

    [Fact]
    public void TotalPages_WhenTotalCountIsZero_ReturnsZero()
    {
        List<int> items = [];
        var paginatedList = new PaginatedList<int>(items, 0, 1, 50);

        Assert.Equal(0, paginatedList.TotalPages);
    }

    [Fact]
    public void TotalPages_WhenTotalCountIsLessThanPageSize_ReturnsOne()
    {
        List<int> items = [1, 2, 3];
        var paginatedList = new PaginatedList<int>(items, 3, 1, 50);

        Assert.Equal(1, paginatedList.TotalPages);
    }

    [Fact]
    public void Properties_AreSetCorrectly()
    {
        List<string> items = ["a", "b", "c"];
        var paginatedList = new PaginatedList<string>(items, 100, 2, 25);

        Assert.Equal(items, paginatedList.Items);
        Assert.Equal(100, paginatedList.TotalCount);
        Assert.Equal(2, paginatedList.Page);
        Assert.Equal(25, paginatedList.PageSize);
        Assert.Equal(4, paginatedList.TotalPages);
    }
}
