namespace KafkaPOC.Core.Models;

public class OrderEvent
{
	public Guid OrderId { get; set; }
	public string CustomerId { get; set; }
	public decimal Amount { get; set; }
	public DateTime CreatedAt { get; set; }
	public string Status { get; set; }
	public List<OrderItem> Items { get; set; } = new();
}

public class OrderItem
{
	public string ProductId { get; set; }
	public string ProductName { get; set; }
	public int Quantity { get; set; }
	public decimal Price { get; set; }
}