namespace EfCore.TestBed.Benchmarks.Entities;

public class Order
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public virtual User User { get; set; } = null!;
    public decimal Total { get; set; }
    public string Status { get; set; } = "Pending";
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public virtual ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}
