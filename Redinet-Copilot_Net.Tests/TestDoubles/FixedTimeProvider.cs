using System;

namespace Redinet_Copilot_Net.Tests.TestDoubles;

public sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
{
  private readonly DateTimeOffset _utcNow = utcNow;

  public override DateTimeOffset GetUtcNow() => _utcNow;
}
