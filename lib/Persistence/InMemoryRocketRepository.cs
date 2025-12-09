using System.Threading.Tasks;
using System.Collections.Concurrent;
using NetAstroBookings.Models;
using System;

namespace NetAstroBookings.Persistence
{
  public class InMemoryRocketRepository
  {
    private readonly ConcurrentDictionary<string, Rocket> _store = new();

    public Task<Rocket> AddAsync(Rocket rocket)
    {
      var count = _store.Count + 1;
      var id = "r" + count.ToString("D4");
      rocket.Id = id;
      _store[id] = rocket;
      return Task.FromResult(rocket);
    }
  }
}
