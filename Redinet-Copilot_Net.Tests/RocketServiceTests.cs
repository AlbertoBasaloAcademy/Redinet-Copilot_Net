using System.Threading.Tasks;
using NetAstroBookings.Business;
using NetAstroBookings.Dtos;
using NetAstroBookings.Models;
using NetAstroBookings.Persistence;
using NUnit.Framework;
using Redinet_Copilot_Net.Tests.TestDoubles;

namespace Redinet_Copilot_Net.Tests;

/// <summary>
/// Unit tests for <see cref="RocketService"/>.
/// </summary>
[TestFixture]
public sealed class RocketServiceTests
{
  private RocketService _service = null!;

  /// <summary>
  /// Creates a fresh service for each test.
  /// </summary>
  [SetUp]
  public void SetUp()
  {
    IRocketRepository repository = new InMemoryRocketRepository();
    _service = new RocketService(repository, new TestLogger<RocketService>());
  }

  /// <summary>
  /// Validates that missing or whitespace name is rejected.
  /// </summary>
  [TestCase("")]
  [TestCase(" ")]
  public async Task CreateAsync_NameMissing_ReturnsValidationFailed(string name)
  {
    // Arrange
    var dto = new RocketDto { Name = name, Capacity = 1, Range = "LEO" };

    // Act
    var result = await _service.CreateAsync(dto);

    // Assert
    Assert.That(result, Is.TypeOf<RocketService.CreateRocketResult.ValidationFailed>());
    var failed = (RocketService.CreateRocketResult.ValidationFailed)result;
    Assert.That(failed.Error, Does.Contain("Name"));
  }

  /// <summary>
  /// Validates that capacity 0 (missing in API binding) is rejected.
  /// </summary>
  [Test]
  public async Task CreateAsync_CapacityMissingOrZero_ReturnsValidationFailed()
  {
    // Arrange
    var dto = new RocketDto { Name = "Falcon", Capacity = 0, Range = "LEO" };

    // Act
    var result = await _service.CreateAsync(dto);

    // Assert
    Assert.That(result, Is.TypeOf<RocketService.CreateRocketResult.ValidationFailed>());
    var failed = (RocketService.CreateRocketResult.ValidationFailed)result;
    Assert.That(failed.Error, Does.Contain("Capacity"));
  }

  /// <summary>
  /// Validates that capacity greater than 10 is rejected.
  /// </summary>
  [TestCase(11)]
  [TestCase(999)]
  public async Task CreateAsync_CapacityTooHigh_ReturnsValidationFailed(int capacity)
  {
    // Arrange
    var dto = new RocketDto { Name = "Falcon", Capacity = capacity, Range = "LEO" };

    // Act
    var result = await _service.CreateAsync(dto);

    // Assert
    Assert.That(result, Is.TypeOf<RocketService.CreateRocketResult.ValidationFailed>());
    var failed = (RocketService.CreateRocketResult.ValidationFailed)result;
    Assert.That(failed.Error, Does.Contain("<= 10"));
  }

  /// <summary>
  /// Validates that invalid range values are rejected.
  /// </summary>
  [TestCase("PLUTO")]
  [TestCase("INVALID")]
  public async Task CreateAsync_RangeInvalid_ReturnsValidationFailed(string range)
  {
    // Arrange
    var dto = new RocketDto { Name = "Falcon", Capacity = 1, Range = range };

    // Act
    var result = await _service.CreateAsync(dto);

    // Assert
    Assert.That(result, Is.TypeOf<RocketService.CreateRocketResult.ValidationFailed>());
    var failed = (RocketService.CreateRocketResult.ValidationFailed)result;
    Assert.That(failed.Error, Does.Contain("LEO"));
    Assert.That(failed.Error, Does.Contain("MOON"));
    Assert.That(failed.Error, Does.Contain("MARS"));
  }

  /// <summary>
  /// Validates that range parsing is case-insensitive.
  /// </summary>
  [TestCase("moon", RocketRange.MOON)]
  [TestCase("MaRs", RocketRange.MARS)]
  public async Task CreateAsync_RangeCaseInsensitive_ReturnsSuccess(string range, RocketRange expected)
  {
    // Arrange
    var dto = new RocketDto { Name = "Falcon", Capacity = 1, Range = range };

    // Act
    var result = await _service.CreateAsync(dto);

    // Assert
    Assert.That(result, Is.TypeOf<RocketService.CreateRocketResult.Success>());
    var success = (RocketService.CreateRocketResult.Success)result;
    Assert.That(success.Rocket.Range, Is.EqualTo(expected));
  }

  /// <summary>
  /// Validates that blank range defaults to LEO.
  /// </summary>
  [TestCase("")]
  [TestCase(" ")]
  public async Task CreateAsync_RangeBlank_DefaultsToLeo(string range)
  {
    // Arrange
    var dto = new RocketDto { Name = "Falcon", Capacity = 1, Range = range };

    // Act
    var result = await _service.CreateAsync(dto);

    // Assert
    Assert.That(result, Is.TypeOf<RocketService.CreateRocketResult.Success>());
    var success = (RocketService.CreateRocketResult.Success)result;
    Assert.That(success.Rocket.Range, Is.EqualTo(RocketRange.LEO));
  }

  /// <summary>
  /// Validates that a rocket is created successfully and fields are normalized.
  /// </summary>
  [Test]
  public async Task CreateAsync_ValidInput_ReturnsSuccessWithAssignedIdAndTrimmedName()
  {
    // Arrange
    var dto = new RocketDto
    {
      Name = "  Falcon  ",
      Capacity = 3,
      Speed = 250,
      Range = "LEO"
    };

    // Act
    var result = await _service.CreateAsync(dto);

    // Assert
    Assert.That(result, Is.TypeOf<RocketService.CreateRocketResult.Success>());
    var success = (RocketService.CreateRocketResult.Success)result;

    Assert.That(success.Rocket.Id, Is.Not.Null.And.Not.Empty);
    Assert.That(success.Rocket.Name, Is.EqualTo("Falcon"));
    Assert.That(success.Rocket.Capacity, Is.EqualTo(3));
    Assert.That(success.Rocket.Speed, Is.EqualTo(250));
    Assert.That(success.Rocket.Range, Is.EqualTo(RocketRange.LEO));
  }

  /// <summary>
  /// Validates that listing returns all created rockets.
  /// </summary>
  [Test]
  public async Task ListAsync_AfterCreates_ReturnsAllRockets()
  {
    // Arrange
    await _service.CreateAsync(new RocketDto { Name = "Falcon", Capacity = 2, Range = "LEO" });
    await _service.CreateAsync(new RocketDto { Name = "Dragon", Capacity = 4, Range = "MOON" });

    // Act
    var rockets = await _service.ListAsync();

    // Assert
    Assert.That(rockets, Has.Count.EqualTo(2));
    Assert.That(rockets, Has.Exactly(1).Matches<Rocket>(r => r.Name == "Falcon"));
    Assert.That(rockets, Has.Exactly(1).Matches<Rocket>(r => r.Name == "Dragon"));
  }

  /// <summary>
  /// Validates that get-by-id returns NotFound for unknown ids.
  /// </summary>
  [Test]
  public async Task GetByIdAsync_UnknownId_ReturnsNotFound()
  {
    // Act
    var result = await _service.GetByIdAsync("r9999");

    // Assert
    Assert.That(result, Is.TypeOf<RocketService.GetRocketResult.NotFound>());
  }

  /// <summary>
  /// Validates that get-by-id returns the rocket when it exists.
  /// </summary>
  [Test]
  public async Task GetByIdAsync_ExistingId_ReturnsFound()
  {
    // Arrange
    var create = await _service.CreateAsync(new RocketDto { Name = "Falcon", Capacity = 2, Range = "LEO" });
    var created = ((RocketService.CreateRocketResult.Success)create).Rocket;

    // Act
    var result = await _service.GetByIdAsync(created.Id!);

    // Assert
    Assert.That(result, Is.TypeOf<RocketService.GetRocketResult.Found>());
    var found = (RocketService.GetRocketResult.Found)result;
    Assert.That(found.Rocket.Id, Is.EqualTo(created.Id));
    Assert.That(found.Rocket.Name, Is.EqualTo("Falcon"));
  }
}
