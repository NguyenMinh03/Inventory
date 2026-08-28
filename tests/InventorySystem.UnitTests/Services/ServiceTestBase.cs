using AutoMapper;
using InventorySystem.Application.Mappings;
using InventorySystem.Domain.Interfaces;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace InventorySystem.UnitTests.Services;

// AutoMapper validates the whole profile on construction, so building a
// MapperConfiguration is the one non-trivial cost in these tests. Sharing a
// single instance across the collection means it happens once per test run
// instead of once per test method (xUnit constructs the test class fresh for
// every [Fact]).
public sealed class MapperFixture
{
    public IMapper Mapper { get; } = new MapperConfiguration(
        cfg => cfg.AddProfile<MappingProfile>(), NullLoggerFactory.Instance).CreateMapper();
}

[CollectionDefinition(Name)]
public sealed class MapperCollection : ICollectionFixture<MapperFixture>
{
    public const string Name = "Mapper";
}

// Shared by every *ServiceTests class: they all mock the same IUnitOfWork
// seam and wire individual repositories onto it in their own constructors.
public abstract class UnitOfWorkTestBase
{
    protected Mock<IUnitOfWork> UnitOfWork { get; } = new();
}

public abstract class ServiceTestBase : UnitOfWorkTestBase
{
    protected IMapper Mapper { get; }

    protected ServiceTestBase(MapperFixture mapperFixture)
    {
        Mapper = mapperFixture.Mapper;
    }
}
