using OrderApi.Models;

namespace OrderApi.Strategies;

public class ZeroQuantityRule : IOrderRule
{
    public void Validate(Order order)
    {
        if (order.Items.Any(item => item.Quantity == 0))
        {
            throw new ArgumentException(
                "Order quantity must be greater than zero.");
        }
    }
}