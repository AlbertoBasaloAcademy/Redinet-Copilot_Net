using System;
using System.Threading;
using System.Threading.Tasks;

namespace NetAstroBookings.Business
{
  /// <summary>
  /// Provides a per-flight async gate to serialize operations that must be consistent under concurrency.
  /// </summary>
  public interface IFlightOperationGate
  {
    /// <summary>
    /// Acquires an async lock for a given flight.
    /// </summary>
    /// <param name="flightId">Flight identifier used as the lock key.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An async disposable handle that releases the lock when disposed.</returns>
    ValueTask<IAsyncDisposable> AcquireAsync(string flightId, CancellationToken cancellationToken = default);
  }
}
