using System;
using Microsoft.Extensions.Logging;

namespace Redinet_Copilot_Net.Tests.TestDoubles;

internal sealed class TestLogger<T> : ILogger<T>
{
  public IDisposable BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;

  public bool IsEnabled(LogLevel logLevel) => true;

  public void Log<TState>(
    LogLevel logLevel,
    EventId eventId,
    TState state,
    Exception? exception,
    Func<TState, Exception?, string> formatter)
  {
  }

  private sealed class NullScope : IDisposable
  {
    public static NullScope Instance { get; } = new();

    public void Dispose()
    {
    }
  }
}
