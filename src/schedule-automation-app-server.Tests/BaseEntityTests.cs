using schedule_automation_app_server.Domain.Common;

namespace schedule_automation_app_server.Tests;

public class BaseEntityTests
{
    private class ConcreteEntity : BaseEntity
    {
        public ConcreteEntity() : base() { }
        public ConcreteEntity(Guid id) : base(id) { }
    }

    [Fact]
    public void BaseEntity_NewInstance_HasNonEmptyId()
    {
        ConcreteEntity entity = new ConcreteEntity();
        Assert.NotEqual(Guid.Empty, entity.Id);
    }

    [Fact]
    public void BaseEntity_SameId_AreEqual()
    {
        Guid id = Guid.NewGuid();
        ConcreteEntity a = new ConcreteEntity(id);
        ConcreteEntity b = new ConcreteEntity(id);

        Assert.Equal(a, b);
        Assert.True(a == b);
        Assert.False(a != b);
    }

    [Fact]
    public void BaseEntity_DifferentId_AreNotEqual()
    {
        ConcreteEntity a = new ConcreteEntity();
        ConcreteEntity b = new ConcreteEntity();

        Assert.NotEqual(a, b);
        Assert.False(a == b);
        Assert.True(a != b);
    }

    [Fact]
    public void BaseEntity_CompareWithNull_ReturnsFalse()
    {
        ConcreteEntity entity = new ConcreteEntity();
        Assert.False(entity.Equals(null));
    }

    [Fact]
    public void BaseEntity_OperatorWithBothNull_ReturnsTrue()
    {
        ConcreteEntity? a = null;
        ConcreteEntity? b = null;
        Assert.True(a == b);
    }

    [Fact]
    public void BaseEntity_OperatorWithOneNull_ReturnsFalse()
    {
        ConcreteEntity? a = new ConcreteEntity();
        ConcreteEntity? b = null;
        Assert.False(a == b);
        Assert.False(b == a);
    }

    [Fact]
    public void BaseEntity_GetHashCode_SameForSameId()
    {
        Guid id = Guid.NewGuid();
        ConcreteEntity a = new ConcreteEntity(id);
        ConcreteEntity b = new ConcreteEntity(id);

        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void BaseEntity_CompareWithDifferentType_ReturnsFalse()
    {
        ConcreteEntity entity = new ConcreteEntity();
        Assert.False(entity.Equals("not an entity"));
    }
}