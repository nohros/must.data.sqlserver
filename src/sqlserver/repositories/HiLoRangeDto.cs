using System;

namespace Nohros.Data.SqlServer
{
  /// <summary>
  /// A basic implementation of the <see cref="IHiLoRange"/> interface.
  /// </summary>
  public class HiLoRangeDto : IHiLoRange
  {
    /// <inheritdoc/>
    public long High { get; set; }

    /// <inheritdoc/>
    public int MaxLow { get; set; }
  }
}
