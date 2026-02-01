using System.Reflection;
using System.ComponentModel.DataAnnotations;

namespace EfCore.TestBed.TestsExample;

/// <summary>
/// Simple data generator for test entities.
/// Generates random but valid data based on property types and attributes.
/// </summary>
public static class TestDataGenerator
{
  private static readonly Random _random = new();

  private static readonly string[] _firstNames = { "John", "Jane", "Bob", "Alice", "Charlie", "Diana", "Eve", "Frank", "Grace", "Henry" };
  private static readonly string[] _lastNames = { "Smith", "Johnson", "Williams", "Brown", "Jones", "Garcia", "Miller", "Davis", "Wilson", "Taylor" };
  private static readonly string[] _domains = { "example.com", "test.com", "demo.org", "sample.net" };
  private static readonly string[] _words = { "lorem", "ipsum", "dolor", "sit", "amet", "consectetur", "adipiscing", "elit", "sed", "do" };

  /// <summary>
  /// Generates a random entity with all properties filled.
  /// </summary>
  public static T Generate<T>() where T : new()
  {
    return Generate<T>(null);
  }

  /// <summary>
  /// Generates a random entity with optional overrides.
  /// </summary>
  public static T Generate<T>(Action<T>? configure) where T : new()
  {
    var entity = new T();

    foreach (var property in typeof(T).GetProperties().Where(p => p.CanWrite))
    {
      // Skip navigation properties (virtual properties that are classes/collections)
      if (property.GetMethod?.IsVirtual == true &&
          (property.PropertyType.IsClass && property.PropertyType != typeof(string) ||
           property.PropertyType.IsGenericType && property.PropertyType.GetGenericTypeDefinition() == typeof(ICollection<>)))
      {
        continue;
      }

      var value = GenerateValue(property);
      if (value != null)
      {
        property.SetValue(entity, value);
      }
    }

    configure?.Invoke(entity);
    return entity;
  }

  /// <summary>
  /// Generates multiple entities.
  /// </summary>
  public static List<T> GenerateMany<T>(int count, Action<T, int>? configure = null) where T : new()
  {
    var entities = new List<T>();
    for (int i = 0; i < count; i++)
    {
      var entity = Generate<T>();
      configure?.Invoke(entity, i);
      entities.Add(entity);
    }
    return entities;
  }

  /// <summary>
  /// Generates a value for a property based on its type and attributes.
  /// </summary>
  private static object? GenerateValue(PropertyInfo property)
  {
    var propertyType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
    var propertyName = property.Name.ToLower();

    // Skip Id properties (usually auto-generated)
    if (propertyName == "id" || propertyName.EndsWith("id"))
    {
      return propertyType == typeof(Guid) ? Guid.NewGuid() : null;
    }

    // Check for attributes
    var maxLength = property.GetCustomAttribute<MaxLengthAttribute>()?.Length ?? 100;
    var minLength = property.GetCustomAttribute<MinLengthAttribute>()?.Length ?? 0;
    var range = property.GetCustomAttribute<RangeAttribute>();

    // Generate based on type
    if (propertyType == typeof(string))
    {
      return GenerateString(propertyName, minLength, maxLength);
    }
    if (propertyType == typeof(int))
    {
      return GenerateInt(range);
    }
    if (propertyType == typeof(long))
    {
      return (long)GenerateInt(range);
    }
    if (propertyType == typeof(decimal))
    {
      return GenerateDecimal(range);
    }
    if (propertyType == typeof(double))
    {
      return (double)GenerateDecimal(range);
    }
    if (propertyType == typeof(float))
    {
      return (float)GenerateDecimal(range);
    }
    if (propertyType == typeof(bool))
    {
      return _random.Next(2) == 1;
    }
    if (propertyType == typeof(DateTime))
    {
      return GenerateDateTime();
    }
    if (propertyType == typeof(DateTimeOffset))
    {
      return new DateTimeOffset(GenerateDateTime());
    }
    if (propertyType == typeof(DateOnly))
    {
      return DateOnly.FromDateTime(GenerateDateTime());
    }
    if (propertyType == typeof(TimeOnly))
    {
      return new TimeOnly(_random.Next(0, 24), _random.Next(0, 60));
    }
    if (propertyType == typeof(Guid))
    {
      return Guid.NewGuid();
    }
    if (propertyType.IsEnum)
    {
      var values = Enum.GetValues(propertyType);
      return values.GetValue(_random.Next(values.Length));
    }

    return null;
  }

  private static string GenerateString(string propertyName, int minLength, int maxLength)
  {
    // Smart generation based on property name
    if (propertyName.Contains("email"))
    {
      return GenerateEmail();
    }
    if (propertyName.Contains("phone"))
    {
      return GeneratePhone();
    }
    if (propertyName.Contains("name"))
    {
      if (propertyName.Contains("first"))
        return _firstNames[_random.Next(_firstNames.Length)];
      if (propertyName.Contains("last"))
        return _lastNames[_random.Next(_lastNames.Length)];
      return $"{_firstNames[_random.Next(_firstNames.Length)]} {_lastNames[_random.Next(_lastNames.Length)]}";
    }
    if (propertyName.Contains("url") || propertyName.Contains("website"))
    {
      return $"https://www.{_domains[_random.Next(_domains.Length)]}";
    }
    if (propertyName.Contains("description") || propertyName.Contains("content") || propertyName.Contains("body"))
    {
      return GenerateText(Math.Min(500, maxLength));
    }
    if (propertyName.Contains("title"))
    {
      return string.Join(" ", Enumerable.Range(0, _random.Next(3, 6)).Select(_ => _words[_random.Next(_words.Length)]));
    }
    if (propertyName.Contains("code") || propertyName.Contains("sku"))
    {
      return $"{(char)('A' + _random.Next(26))}{(char)('A' + _random.Next(26))}-{_random.Next(1000, 9999)}";
    }
    if (propertyName.Contains("zip") || propertyName.Contains("postal"))
    {
      return $"{_random.Next(10000, 99999)}";
    }
    if (propertyName.Contains("city"))
    {
      string[] cities = { "New York", "Los Angeles", "Chicago", "Houston", "Phoenix", "Philadelphia" };
      return cities[_random.Next(cities.Length)];
    }
    if (propertyName.Contains("country"))
    {
      string[] countries = { "USA", "Canada", "UK", "Germany", "France", "Japan" };
      return countries[_random.Next(countries.Length)];
    }
    if (propertyName.Contains("address") || propertyName.Contains("street"))
    {
      return $"{_random.Next(100, 9999)} {_lastNames[_random.Next(_lastNames.Length)]} Street";
    }

    // Default: random text
    var length = Math.Max(minLength, _random.Next(5, Math.Min(50, maxLength)));
    return GenerateText(length);
  }

  private static int GenerateInt(RangeAttribute? range)
  {
    var min = range?.Minimum as int? ?? 0;
    var max = range?.Maximum as int? ?? 1000;
    return _random.Next(min, max + 1);
  }

  private static decimal GenerateDecimal(RangeAttribute? range)
  {
    var min = Convert.ToDouble(range?.Minimum ?? 0);
    var max = Convert.ToDouble(range?.Maximum ?? 1000);
    return Math.Round((decimal)(min + _random.NextDouble() * (max - min)), 2);
  }

  private static DateTime GenerateDateTime()
  {
    var start = DateTime.Now.AddYears(-5);
    var range = (DateTime.Now.AddYears(1) - start).Days;
    return start.AddDays(_random.Next(range));
  }

  /// <summary>
  /// Generates a random email address.
  /// </summary>
  public static string GenerateEmail()
  {
    var first = _firstNames[_random.Next(_firstNames.Length)].ToLower();
    var last = _lastNames[_random.Next(_lastNames.Length)].ToLower();
    var domain = _domains[_random.Next(_domains.Length)];
    return $"{first}.{last}{_random.Next(100)}@{domain}";
  }

  /// <summary>
  /// Generates a random phone number.
  /// </summary>
  public static string GeneratePhone()
  {
    return $"+1-{_random.Next(200, 999)}-{_random.Next(100, 999)}-{_random.Next(1000, 9999)}";
  }

  /// <summary>
  /// Generates random text of specified length.
  /// </summary>
  public static string GenerateText(int maxLength)
  {
    var wordCount = Math.Max(1, maxLength / 6); // Average word length
    var text = string.Join(" ", Enumerable.Range(0, wordCount).Select(_ => _words[_random.Next(_words.Length)]));
    return text.Length > maxLength ? text.Substring(0, maxLength) : text;
  }

  /// <summary>
  /// Generates a random name.
  /// </summary>
  public static string GenerateName()
  {
    return $"{_firstNames[_random.Next(_firstNames.Length)]} {_lastNames[_random.Next(_lastNames.Length)]}";
  }

  /// <summary>
  /// Generates a random first name.
  /// </summary>
  public static string GenerateFirstName()
  {
    return _firstNames[_random.Next(_firstNames.Length)];
  }

  /// <summary>
  /// Generates a random last name.
  /// </summary>
  public static string GenerateLastName()
  {
    return _lastNames[_random.Next(_lastNames.Length)];
  }

  /// <summary>
  /// Picks a random item from an array.
  /// </summary>
  public static T PickRandom<T>(params T[] items)
  {
    return items[_random.Next(items.Length)];
  }

  /// <summary>
  /// Picks a random item from a list.
  /// </summary>
  public static T PickRandom<T>(IList<T> items)
  {
    return items[_random.Next(items.Count)];
  }

  /// <summary>
  /// Generates a random integer in range.
  /// </summary>
  public static int RandomInt(int min = 0, int max = 100)
  {
    return _random.Next(min, max + 1);
  }

  /// <summary>
  /// Generates a random decimal in range.
  /// </summary>
  public static decimal RandomDecimal(decimal min = 0, decimal max = 100)
  {
    return Math.Round(min + (decimal)_random.NextDouble() * (max - min), 2);
  }
}
