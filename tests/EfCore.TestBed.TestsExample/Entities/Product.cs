using System.ComponentModel.DataAnnotations;

namespace EfCore.TestBed.TestsExample.Entities;

public class Product
{
  public int Id { get; set; }

  [Required]
  public string Name { get; set; } = null!;

  [Required]
  public string SKU { get; set; } = null!;

  public decimal Price { get; set; }
  public int Stock { get; set; }
}
