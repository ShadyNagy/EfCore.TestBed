namespace EfCore.TestBed.Assertions;

/// <summary>
/// Exception thrown when an assertion fails.
/// </summary>
public class AssertionException : Exception
{
  public AssertionException(string message) : base(message)
  {
  }

  public AssertionException(string message, Exception innerException) : base(message, innerException)
  {
  }
}
