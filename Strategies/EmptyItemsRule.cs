using OrderApi.Models;

namespace OrderApi.Strategies;

public class EmptyItemsRule : IOrderRule
{
    public void Validate(Order order)
    {
        if (order.Items.Count == 0)
        {
            throw new ArgumentException("Order must contain at least one item");
        }
    }
}
