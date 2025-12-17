using System.Threading.Tasks;
using NetAstroBookings.Models;
using NetAstroBookings.Persistence;
using NUnit.Framework;

namespace Redinet_Copilot_Net.Tests;

/// <summary>
/// Unit tests for <see cref="InMemoryRocketRepository"/>.
/// </summary>
[TestFixture]
public sealed class InMemoryRocketRepositoryTests
{
  /// <summary>
  /// Validates that IDs are assigned deterministically and sequentially.
  /// </summary>
  [Test]
  public async Task AddAsync_Twice_AssignsSequentialIds()
  {
    // Arrange
    var repository = new InMemoryRocketRepository();

    // Act
    var first = await repository.AddAsync(new Rocket { Name = "Falcon", Capacity = 1, Range = RocketRange.LEO });
    var second = await repository.AddAsync(new Rocket { Name = "Dragon", Capacity = 2, Range = RocketRange.MOON });

    // Assert
    Assert.That(first.Id, Is.EqualTo("r0001"));
    Assert.That(second.Id, Is.EqualTo("r0002"));
  }

  /// <summary>
  /// Validates that blank identifiers return null.
  /// </summary>
  [TestCase("")]
  [TestCase(" ")]
  public async Task GetByIdAsync_BlankId_ReturnsNull(string id)
  {
    // Arrange
    var repository = new InMemoryRocketRepository();

    // Act
    var rocket = await repository.GetByIdAsync(id);

    // Assert
    Assert.That(rocket, Is.Null);
  }

  /// <summary>
  /// Validates that retrieved instances are clones and do not mutate stored data.
  /// </summary>
  [Test]
  public async Task GetByIdAsync_ReturnsClone_MutationsDoNotAffectStore()
  {
    // Arrange
    var repository = new InMemoryRocketRepository();
    var created = await repository.AddAsync(new Rocket { Name = "Falcon", Capacity = 1, Range = RocketRange.LEO });

    // Act
    var fetched1 = await repository.GetByIdAsync(created.Id!);
    fetched1!.Name = "Hacked";

    var fetched2 = await repository.GetByIdAsync(created.Id!);

    // Assert
    Assert.That(fetched2, Is.Not.Null);
    Assert.That(fetched2!.Name, Is.EqualTo("Falcon"));
  }

  /// <summary>
  /// Validates that list returns clones and includes all stored rockets.
  /// </summary>
  [Test]
  public async Task ListAsync_AfterAdds_ReturnsAllRockets()
  {
    // Arrange
    var repository = new InMemoryRocketRepository();
    await repository.AddAsync(new Rocket { Name = "Falcon", Capacity = 1, Range = RocketRange.LEO });
    await repository.AddAsync(new Rocket { Name = "Dragon", Capacity = 2, Range = RocketRange.MOON });

    // Act
    var list = await repository.ListAsync();

    // Assert
    Assert.That(list, Has.Count.EqualTo(2));
    Assert.That(list, Has.Exactly(1).Matches<Rocket>(r => r.Name == "Falcon"));
    Assert.That(list, Has.Exactly(1).Matches<Rocket>(r => r.Name == "Dragon"));
  }
}
