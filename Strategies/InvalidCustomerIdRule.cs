using OrderApi.Models;

namespace OrderApi.Strategies;

public class InvalidCustomerIdRule : IOrderRule
{
    public void Validate(Order order)
    {
        if (order.CustomerId <= 0)
        {
            throw new ArgumentException("CustomerId is invalid");
        }
    }
}
