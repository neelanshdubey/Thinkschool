using OrderApi.Models;
using OrderApi.Strategies;

namespace OrderApi.Services;

public class OrderService
{
    private readonly IEnumerable<IOrderRule> _rules;

    public OrderService(IEnumerable<IOrderRule> rules)
    {
        _rules = rules;
    }

    public Order CreateOrder(Order order)
    {
        foreach (var rule in _rules)
        {
            rule.Validate(order);
        }

        return order;
    }
}