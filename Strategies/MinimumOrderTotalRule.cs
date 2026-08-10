using OrderApi.Models;

namespace OrderApi.Strategies;

public class MinimumOrderTotalRule : IOrderRule
{
    private const decimal MinimumTotal = 10m;

    public void Validate(Order order)
    {
        if (order.Total < MinimumTotal)
        {
            throw new ArgumentException(
                $"Order total must be at least {MinimumTotal}.");
        }
    }
}