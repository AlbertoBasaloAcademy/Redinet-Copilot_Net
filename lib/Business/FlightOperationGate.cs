using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace NetAstroBookings.Business
{
  /// <summary>
  /// In-memory implementation of <see cref="IFlightOperationGate"/> based on <see cref="SemaphoreSlim"/>.
  /// </summary>
  public sealed class FlightOperationGate : IFlightOperationGate
  {
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _gates = new(StringComparer.Ordinal);

    /// <inheritdoc />
    public async ValueTask<IAsyncDisposable> AcquireAsync(string flightId, CancellationToken cancellationToken = default)
    {
      if (string.IsNullOrWhiteSpace(flightId))
      {
        return NoopReleaser.Instance;
      }

      var gate = _gates.GetOrAdd(flightId, _ => new SemaphoreSlim(1, 1));
      await gate.WaitAsync(cancellationToken);
      return new GateReleaser(gate);
    }

    private sealed class GateReleaser(SemaphoreSlim gate) : IAsyncDisposable
    {
      public ValueTask DisposeAsync()
      {
        gate.Release();
        return ValueTask.CompletedTask;
      }
    }

    private sealed class NoopReleaser : IAsyncDisposable
    {
      public static NoopReleaser Instance { get; } = new();

      public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
  }
}
