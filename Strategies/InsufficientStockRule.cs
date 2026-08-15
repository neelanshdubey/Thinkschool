using OrderApi.Models;

namespace OrderApi.Strategies;

public class InsufficientStockRule : IOrderRule
{
    public void Validate(Order order)
    {
        foreach (var item in order.Items)
        {
            // Preserves the legacy off-by-one: uses >= instead of >, so an
            // order quantity exactly equal to stock is incorrectly rejected.
            if (item.Quantity >= item.StockQuantity)
            {
                throw new ArgumentException(
                    "Not enough stock for product " + item.ProductId);
            }
        }
    }
}
