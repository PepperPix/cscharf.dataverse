using System.Runtime.Intrinsics.X86;
using CScharf.Dataverse.Tests.Fixtures;

namespace CScharf.Dataverse.Tests;

public class AttributeResolverTests
{
    [Fact]
    public void Ctors()
    {
        var target = new TestEntity(Guid.NewGuid());
        var image = new TestEntity(Guid.NewGuid());

        var sut1 = new AttributeResolver<Entity>(image,target);
        var sut2 = new AttributeResolver<TestEntity>(image, target);
        var sut3 = new AttributeResolver<Entity>(target);
        var sut4 = new AttributeResolver<TestEntity>(target);

        Assert.Equal(sut1.PreEntity!.Id, sut2.PreEntity!.Id);
        Assert.Equal(sut1.TargetEntity.Id, sut2.TargetEntity.Id);
        Assert.Equal(sut2.TargetEntity.Id, sut3.TargetEntity.Id);
        Assert.Equal(sut3.TargetEntity.Id, sut4.TargetEntity.Id);

        Assert.Null(sut3.PreEntity);
        Assert.Null(sut4.PreEntity);
    }
}
