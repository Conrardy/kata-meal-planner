using FluentAssertions;
using MealPlanner.Application.Common.Mediator;
using MealPlanner.Application.DailyDigest;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MealPlanner.Application.Tests.Common.Mediator;

public sealed class MediatorServiceCollectionExtensionsTests
{
    [Fact]
    public void AddMediator_WithAssembly_ShouldRegisterMediatorService()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddMediator(typeof(GetDailyDigestQuery).Assembly);
        var provider = services.BuildServiceProvider();

        // Assert
        var mediator = provider.GetService<IMediator>();
        mediator.Should().NotBeNull();
        mediator.Should().BeOfType<MealPlanner.Application.Common.Mediator.Mediator>();
    }

    [Fact]
    public void AddMediator_WithGenericType_ShouldRegisterMediatorService()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddMediator<GetDailyDigestQuery>();
        var provider = services.BuildServiceProvider();

        // Assert
        var mediator = provider.GetService<IMediator>();
        mediator.Should().NotBeNull();
    }

    [Fact]
    public void AddMediator_WithNoAssemblies_ShouldThrowArgumentException()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var act = () => services.AddMediator();

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*At least one assembly must be provided*");
    }

    [Fact]
    public void AddMediator_ShouldRegisterAsScopedLifetime()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddMediator(typeof(GetDailyDigestQuery).Assembly);

        // Assert
        var mediatorDescriptor = services.FirstOrDefault(s => s.ServiceType == typeof(IMediator));
        mediatorDescriptor.Should().NotBeNull();
        mediatorDescriptor!.Lifetime.Should().Be(ServiceLifetime.Scoped);
    }

    [Fact]
    public void AddMediator_ShouldReturnServiceCollectionForChaining()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddMediator(typeof(GetDailyDigestQuery).Assembly);

        // Assert
        result.Should().BeSameAs(services);
    }

    [Fact]
    public void AddMediator_WithMultipleAssemblies_ShouldScanAllAssemblies()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddMediator(
            typeof(GetDailyDigestQuery).Assembly,
            typeof(MediatorServiceCollectionExtensionsTests).Assembly
        );

        // Assert
        var provider = services.BuildServiceProvider();
        var mediator = provider.GetService<IMediator>();
        mediator.Should().NotBeNull();
    }
}
