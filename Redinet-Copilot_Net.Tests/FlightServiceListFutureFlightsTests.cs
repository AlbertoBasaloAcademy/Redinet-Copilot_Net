using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using NetAstroBookings.Business;
using NetAstroBookings.Models;
using NetAstroBookings.Persistence;
using NetAstroBookings.Presentation;
using NUnit.Framework;
using Redinet_Copilot_Net.Tests.TestDoubles;

namespace Redinet_Copilot_Net.Tests;

/// <summary>
/// Unit tests for <see cref="FlightService.ListFutureFlightsAsync"/> and its HTTP mapping in <see cref="FlightEndpoints"/>.
/// </summary>
[TestFixture]
public sealed class FlightServiceListFutureFlightsTests
{
  private static readonly DateTimeOffset FixedNow = new(2025, 12, 17, 12, 0, 0, TimeSpan.Zero);

  private static FlightService CreateService(IFlightRepository flightRepository)
  {
    IBookingRepository bookingRepository = new InMemoryBookingRepository();
    IRocketRepository rocketRepository = new InMemoryRocketRepository();
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

  private static async Task SeedFlightAsync(IFlightRepository flightRepository, DateTimeOffset launchDate, FlightState state)
  {
    await flightRepository.AddAsync(new Flight
    {
      RocketId = "r0001",
      LaunchDate = launchDate,
      BasePrice = 10m,
      MinimumPassengers = 1,
      State = state
    });
  }

  [Test]
  public async Task ListFutureFlightsAsync_NoStateFilter_ReturnsOnlyStrictlyFutureFlights()
  {
    // Arrange
    IFlightRepository flightRepository = new InMemoryFlightRepository();
    var service = CreateService(flightRepository);

    await SeedFlightAsync(flightRepository, FixedNow.AddMinutes(-5), FlightState.SCHEDULED);
    await SeedFlightAsync(flightRepository, FixedNow, FlightState.CONFIRMED);
    await SeedFlightAsync(flightRepository, FixedNow.AddMinutes(5), FlightState.SCHEDULED);

    // Act
    var result = await service.ListFutureFlightsAsync(null);

    // Assert
    Assert.That(result, Is.TypeOf<FlightService.ListFlightsResult.Success>());
    var success = (FlightService.ListFlightsResult.Success)result;

    Assert.That(success.Flights, Has.Count.EqualTo(1));
    Assert.That(success.Flights.Single().LaunchDate, Is.GreaterThan(FixedNow));
  }

  [Test]
  public async Task ListFutureFlightsAsync_StateFilter_ReturnsOnlyFutureFlightsInThatState()
  {
    // Arrange
    IFlightRepository flightRepository = new InMemoryFlightRepository();
    var service = CreateService(flightRepository);

    await SeedFlightAsync(flightRepository, FixedNow.AddHours(1), FlightState.SCHEDULED);
    await SeedFlightAsync(flightRepository, FixedNow.AddHours(2), FlightState.CONFIRMED);
    await SeedFlightAsync(flightRepository, FixedNow.AddHours(-1), FlightState.SCHEDULED);

    // Act
    var result = await service.ListFutureFlightsAsync("scheduled");

    // Assert
    Assert.That(result, Is.TypeOf<FlightService.ListFlightsResult.Success>());
    var success = (FlightService.ListFlightsResult.Success)result;

    Assert.That(success.Flights, Has.Count.EqualTo(1));
    Assert.That(success.Flights.Single().State, Is.EqualTo(FlightState.SCHEDULED));
    Assert.That(success.Flights.Single().LaunchDate, Is.GreaterThan(FixedNow));
  }

  [TestCase("")]
  [TestCase(" ")]
  [TestCase("   ")]
  public async Task ListFutureFlightsAsync_StateFilterBlank_ReturnsValidationFailed(string state)
  {
    // Arrange
    IFlightRepository flightRepository = new InMemoryFlightRepository();
    var service = CreateService(flightRepository);

    // Act
    var result = await service.ListFutureFlightsAsync(state);

    // Assert
    Assert.That(result, Is.TypeOf<FlightService.ListFlightsResult.ValidationFailed>());
    var failed = (FlightService.ListFlightsResult.ValidationFailed)result;
    Assert.That(failed.Error, Does.Contain("state"));
  }

  [TestCase("INVALID")]
  [TestCase("PLUTO")]
  public async Task ListFutureFlightsAsync_StateFilterInvalid_ReturnsValidationFailed(string state)
  {
    // Arrange
    IFlightRepository flightRepository = new InMemoryFlightRepository();
    var service = CreateService(flightRepository);

    // Act
    var result = await service.ListFutureFlightsAsync(state);

    // Assert
    Assert.That(result, Is.TypeOf<FlightService.ListFlightsResult.ValidationFailed>());
    var failed = (FlightService.ListFlightsResult.ValidationFailed)result;
    Assert.That(failed.Error, Does.Contain("SCHEDULED"));
  }

  [Test]
  public async Task FlightEndpoints_ListFlights_InvalidState_ReturnsBadRequest()
  {
    // Arrange
    IFlightRepository flightRepository = new InMemoryFlightRepository();
    var service = CreateService(flightRepository);

    var listFlights = typeof(FlightEndpoints)
      .GetMethod("ListFlights", BindingFlags.NonPublic | BindingFlags.Static);

    Assert.That(listFlights, Is.Not.Null, "Expected FlightEndpoints to contain a private static ListFlights handler.");

    // Act
    var task = (Task<IResult>)listFlights!
      .Invoke(null, new object?[] { "INVALID", service })!;

    var result = await task;

    // Assert
    Assert.That(result, Is.AssignableTo<IStatusCodeHttpResult>());
    var statusCode = ((IStatusCodeHttpResult)result).StatusCode;
    Assert.That(statusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
  }

  [Test]
  public async Task FlightEndpoints_ListFlights_ValidState_ReturnsOk()
  {
    // Arrange
    IFlightRepository flightRepository = new InMemoryFlightRepository();
    var service = CreateService(flightRepository);

    await SeedFlightAsync(flightRepository, FixedNow.AddHours(1), FlightState.SCHEDULED);

    var listFlights = typeof(FlightEndpoints)
      .GetMethod("ListFlights", BindingFlags.NonPublic | BindingFlags.Static);

    Assert.That(listFlights, Is.Not.Null, "Expected FlightEndpoints to contain a private static ListFlights handler.");

    // Act
    var task = (Task<IResult>)listFlights!
      .Invoke(null, new object?[] { "SCHEDULED", service })!;

    var result = await task;

    // Assert
    Assert.That(result, Is.AssignableTo<IStatusCodeHttpResult>());
    var statusCode = ((IStatusCodeHttpResult)result).StatusCode;
    Assert.That(statusCode, Is.EqualTo(StatusCodes.Status200OK));
  }
}
