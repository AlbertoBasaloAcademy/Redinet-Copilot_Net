using System;
using System.Threading.Tasks;
using NetAstroBookings.Business;
using NetAstroBookings.Dtos;
using NetAstroBookings.Models;
using NetAstroBookings.Persistence;
using NUnit.Framework;
using Redinet_Copilot_Net.Tests.TestDoubles;

namespace Redinet_Copilot_Net.Tests;

/// <summary>
/// Unit tests for <see cref="FlightService"/> flight creation validation.
/// </summary>
[TestFixture]
public sealed class FlightServiceCreateTests
{
  private static readonly DateTimeOffset FixedNow = new(2025, 12, 17, 12, 0, 0, TimeSpan.Zero);

  private FlightService CreateService(IRocketRepository rocketRepository)
  {
    IFlightRepository flightRepository = new InMemoryFlightRepository();
    IBookingRepository bookingRepository = new InMemoryBookingRepository();
    IFlightOperationGate operationGate = new FlightOperationGate();
    var timeProvider = new FixedTimeProvider(FixedNow);

    return new FlightService(
      flightRepository,
      bookingRepository,
      rocketRepository,
      operationGate,
      timeProvider,
      new TestLogger<FlightService>());
  }

  private static async Task<Rocket> SeedRocketAsync(IRocketRepository rocketRepository)
  {
    return await rocketRepository.AddAsync(new Rocket
    {
      Name = "Falcon",
      Capacity = 3,
      Range = RocketRange.LEO
    });
  }

  [TestCase(0)]
  [TestCase(-60)]
  public async Task CreateAsync_LaunchDateNotInFuture_ReturnsValidationFailed(int secondsDeltaFromNow)
  {
    // Arrange
    IRocketRepository rocketRepository = new InMemoryRocketRepository();
    var rocket = await SeedRocketAsync(rocketRepository);

    var service = CreateService(rocketRepository);

    var dto = new CreateFlightDto
    {
      RocketId = rocket.Id!,
      LaunchDate = FixedNow.AddSeconds(secondsDeltaFromNow),
      BasePrice = 10m,
      MinimumPassengers = 1
    };

    // Act
    var result = await service.CreateAsync(dto);

    // Assert
    Assert.That(result, Is.TypeOf<FlightService.CreateFlightResult.ValidationFailed>());
    var failed = (FlightService.CreateFlightResult.ValidationFailed)result;
    Assert.That(failed.Error, Does.Contain("launchDate"));
  }

  [TestCase(0)]
  [TestCase(-1)]
  public async Task CreateAsync_BasePriceNonPositive_ReturnsValidationFailed(decimal basePrice)
  {
    // Arrange
    IRocketRepository rocketRepository = new InMemoryRocketRepository();
    var rocket = await SeedRocketAsync(rocketRepository);

    var service = CreateService(rocketRepository);

    var dto = new CreateFlightDto
    {
      RocketId = rocket.Id!,
      LaunchDate = FixedNow.AddHours(1),
      BasePrice = basePrice,
      MinimumPassengers = 1
    };

    // Act
    var result = await service.CreateAsync(dto);

    // Assert
    Assert.That(result, Is.TypeOf<FlightService.CreateFlightResult.ValidationFailed>());
    var failed = (FlightService.CreateFlightResult.ValidationFailed)result;
    Assert.That(failed.Error, Does.Contain("basePrice"));
  }

  [Test]
  public async Task CreateAsync_RocketDoesNotExist_ReturnsRocketNotFound()
  {
    // Arrange
    IRocketRepository rocketRepository = new InMemoryRocketRepository();
    var service = CreateService(rocketRepository);

    var dto = new CreateFlightDto
    {
      RocketId = "r9999",
      LaunchDate = FixedNow.AddHours(1),
      BasePrice = 10m,
      MinimumPassengers = 1
    };

    // Act
    var result = await service.CreateAsync(dto);

    // Assert
    Assert.That(result, Is.TypeOf<FlightService.CreateFlightResult.RocketNotFound>());
  }

  [Test]
  public async Task CreateAsync_MinimumPassengersOmitted_DefaultsTo5()
  {
    // Arrange
    IRocketRepository rocketRepository = new InMemoryRocketRepository();
    var rocket = await SeedRocketAsync(rocketRepository);

    var service = CreateService(rocketRepository);

    var dto = new CreateFlightDto
    {
      RocketId = rocket.Id!,
      LaunchDate = FixedNow.AddHours(1),
      BasePrice = 10m,
      MinimumPassengers = null
    };

    // Act
    var result = await service.CreateAsync(dto);

    // Assert
    Assert.That(result, Is.TypeOf<FlightService.CreateFlightResult.Success>());
    var success = (FlightService.CreateFlightResult.Success)result;
    Assert.That(success.Flight.MinimumPassengers, Is.EqualTo(5));
  }

  [Test]
  public async Task CreateAsync_ValidInput_ReturnsSuccessWithScheduledState()
  {
    // Arrange
    IRocketRepository rocketRepository = new InMemoryRocketRepository();
    var rocket = await SeedRocketAsync(rocketRepository);

    var service = CreateService(rocketRepository);

    var dto = new CreateFlightDto
    {
      RocketId = $"  {rocket.Id}  ",
      LaunchDate = FixedNow.AddHours(1),
      BasePrice = 10m,
      MinimumPassengers = 2
    };

    // Act
    var result = await service.CreateAsync(dto);

    // Assert
    Assert.That(result, Is.TypeOf<FlightService.CreateFlightResult.Success>());
    var success = (FlightService.CreateFlightResult.Success)result;

    Assert.That(success.Flight.Id, Is.Not.Null.And.Not.Empty);
    Assert.That(success.Flight.RocketId, Is.EqualTo(rocket.Id));
    Assert.That(success.Flight.State, Is.EqualTo(FlightState.SCHEDULED));
  }
}
