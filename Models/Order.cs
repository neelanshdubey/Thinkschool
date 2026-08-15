namespace OrderApi.Models;

public class Order
{
    public int Id { get; set; }

    public int CustomerId { get; set; }

    public List<OrderItem> Items { get; set; } = new();

    public decimal Total => Items.Sum(x => x.Quantity * x.UnitPrice);
}

public class OrderItem
{
    public int ProductId { get; set; }

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public int StockQuantity { get; set; }
}