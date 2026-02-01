namespace EfCore.TestBed.Configuration;

/// <summary>
/// Configuration options for TestBed.
/// </summary>
public class TestBedOptions
{
  /// <summary>
  /// The database provider to use. Default: SQLite in-memory.
  /// </summary>
  public TestDbProvider Provider { get; set; } = TestDbProvider.SqliteInMemory;

  /// <summary>
  /// Whether to automatically call EnsureCreated(). Default: true.
  /// </summary>
  public bool AutoCreateDatabase { get; set; } = true;

  /// <summary>
  /// Whether to enable sensitive data logging. Default: false.
  /// </summary>
  public bool EnableSensitiveDataLogging { get; set; } = false;

  /// <summary>
  /// Whether to enable detailed errors. Default: true.
  /// </summary>
  public bool EnableDetailedErrors { get; set; } = true;

  /// <summary>
  /// Whether to log to console. Default: false.
  /// </summary>
  public bool LogToConsole { get; set; } = false;

  /// <summary>
  /// Custom database name (for InMemory provider). Default: auto-generated GUID.
  /// </summary>
  public string? DatabaseName { get; set; }

  /// <summary>
  /// Connection string (for file-based SQLite). Default: in-memory.
  /// </summary>
  public string? ConnectionString { get; set; }

  /// <summary>
  /// Whether to use lazy loading proxies. Default: false.
  /// </summary>
  public bool UseLazyLoadingProxies { get; set; } = false;

  /// <summary>
  /// Query tracking behavior. Default: TrackAll.
  /// </summary>
  public QueryTrackingBehavior QueryTrackingBehavior { get; set; } = QueryTrackingBehavior.TrackAll;
}
