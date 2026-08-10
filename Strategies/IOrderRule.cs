using OrderApi.Models;

namespace OrderApi.Strategies;

public interface IOrderRule
{
    void Validate(Order order);
}