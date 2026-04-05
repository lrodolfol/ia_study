//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
using Domain.Common;

namespace pigMoney.Tests;

public class BaseEntityTests
{
    private sealed class TestEntity : BaseEntity;

    [Fact]
    public void Id_ShouldBeIntType()
    {
        var entity = new TestEntity();

        Assert.IsType<int>(entity.Id);
    }

    [Fact]
    public void CreatedAt_ShouldBeDateTimeType()
    {
        var entity = new TestEntity();

        Assert.IsType<DateTime>(entity.CreatedAt);
    }

    [Fact]
    public void UpdatedAt_ShouldBeDateTimeType()
    {
        var entity = new TestEntity();

        Assert.IsType<DateTime>(entity.UpdatedAt);
    }

    [Fact]
    public void Properties_ShouldBeSettable()
    {
        var now = DateTime.UtcNow;
        var entity = new TestEntity
        {
            Id = 10,
            CreatedAt = now,
            UpdatedAt = now
        };

        Assert.Equal(10, entity.Id);
        Assert.Equal(now, entity.CreatedAt);
        Assert.Equal(now, entity.UpdatedAt);
    }
}
