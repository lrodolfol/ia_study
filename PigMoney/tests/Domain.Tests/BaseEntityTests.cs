//created by: rodolfojesus - tinosnegocios.com.br - rodolfo0ti@gmail.com - linkedin: rodolfojesus
namespace Domain.Tests;

using Domain.Entities;

public class BaseEntityTests
{
    [Fact]
    public void BaseEntity_InitializesCreatedAtToUtcTime()
    {
        DateTime before = DateTime.UtcNow;

        var entity = new TestEntity();

        DateTime after = DateTime.UtcNow;

        Assert.True(entity.CreatedAt >= before && entity.CreatedAt <= after);
        Assert.Equal(DateTimeKind.Utc, entity.CreatedAt.Kind);
    }

    [Fact]
    public void BaseEntity_InitializesUpdatedAtToUtcTime()
    {
        DateTime before = DateTime.UtcNow;

        var entity = new TestEntity();

        DateTime after = DateTime.UtcNow;

        Assert.True(entity.UpdatedAt >= before && entity.UpdatedAt <= after);
        Assert.Equal(DateTimeKind.Utc, entity.UpdatedAt.Kind);
    }

    [Fact]
    public void BaseEntity_InitializesIsDeletedToFalse()
    {
        var entity = new TestEntity();

        Assert.False(entity.IsDeleted);
    }

    [Fact]
    public void BaseEntity_Id_DefaultsToZero()
    {
        var entity = new TestEntity();

        Assert.Equal(0, entity.Id);
    }

    [Fact]
    public void BaseEntity_Id_CanBeSet()
    {
        var entity = new TestEntity { Id = 42 };

        Assert.Equal(42, entity.Id);
    }

    private class TestEntity : BaseEntity;
}
