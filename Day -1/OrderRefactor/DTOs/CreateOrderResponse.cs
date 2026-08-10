namespace OrderRefactor.DTOs;

public class CreateOrderResponse
{
    public int OrderId { get; set; }

    public int CustomerId { get; set; }

    public decimal TotalAmount { get; set; }

    public DateTime CreatedAt { get; set; }
}

