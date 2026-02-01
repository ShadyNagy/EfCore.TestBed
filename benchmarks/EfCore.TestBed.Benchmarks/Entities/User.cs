using System.ComponentModel.DataAnnotations;

namespace EfCore.TestBed.Benchmarks.Entities;

public class User
{
  public int Id { get; set; }

  [Required]
  [MaxLength(100)]
  public string Name { get; set; } = null!;

  [Required]
  [EmailAddress]
  public string Email { get; set; } = null!;

  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

  public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
