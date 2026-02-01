namespace EfCore.TestBed.Configuration;

/// <summary>
/// Query tracking behavior options.
/// </summary>
public enum QueryTrackingBehavior
{
  /// <summary>
  /// Track all queries (default EF behavior).
  /// </summary>
  TrackAll,

  /// <summary>
  /// No tracking by default.
  /// </summary>
  NoTracking,

  /// <summary>
  /// No tracking with identity resolution.
  /// </summary>
  NoTrackingWithIdentityResolution
}
