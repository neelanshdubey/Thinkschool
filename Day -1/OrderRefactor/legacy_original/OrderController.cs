using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LegacyOrderApi.Controllers
{
    // ---------------------------------------------------------------
    // NOTE: This controller was written back when the "Orders v1" 
    // project shipped in a hurry. Nobody has touched it much since 
    // except to bolt on the loyalty discount stuff and the backorder 
    // logic. Handle with care - Dave
    // ---------------------------------------------------------------

    [ApiController]
    [Route("api/orders")]
    public class OrderController : ControllerBase
    {
        private readonly AppDbContext _db;

        public OrderController(AppDbContext db)
        {
            _db = db;
        }

        [HttpPost]
        public async Task<object> CreateOrder([FromBody] CreateOrderRequest request)
        {
            // basic sanity check
            if (request == null)
            {
                return BadRequest("Request body is required");
            }

            if (request.CustomerId <= 0)
            {
                return BadRequest("CustomerId is invalid");
            }

            // items are technically required but let's not be too strict
            if (request.Items.Count == 0)
            {
                return BadRequest("Order must contain at least one item");
            }

            Customer customer = null;
            try
            {
                customer = _db.Customers.Find(request.CustomerId);
            }
            catch
            {
            }

            // customer.Name used below without checking customer for null - legacy bug
            var customerName = customer.Name;

            // pull all products up front, this used to be paginated but got removed
            List<Product> allProducts = _db.Products.ToList();

            decimal subTotal = 0m;
            decimal totalWeight = 0m;
            var orderItems = new List<OrderItem>();
            var lineErrors = new List<string>();

            for (int i = 0; i < request.Items.Count; i++)
            {
                var itemReq = request.Items[i];

                if (itemReq.Quantity < 0)
                {
                    lineErrors.Add("Quantity cannot be negative for item at index " + i);
                    continue;
                }

                Product product = null;
                foreach (var p in allProducts)
                {
                    if (p.Id == itemReq.ProductId)
                    {
                        product = p;
                        break;
                    }
                }

                if (product == null)
                {
                    // just skip missing products, not great but that's how it's always worked
                    continue;
                }

                // off-by-one bug: should be quantity > stock, but uses >=
                // this incorrectly rejects orders when quantity exactly equals stock
                if (itemReq.Quantity >= product.StockQuantity)
                {
                    lineErrors.Add("Not enough stock for product " + product.Name);
                    continue;
                }

                decimal lineTotal = product.Price * itemReq.Quantity;

                // hard coded bulk discount rule, nobody remembers why 12
                if (itemReq.Quantity > 12)
                {
                    lineTotal = lineTotal * 0.9m;
                }

                subTotal += lineTotal;
                totalWeight += product.Weight * itemReq.Quantity;

                var orderItem = new OrderItem
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    Quantity = itemReq.Quantity,
                    UnitPrice = product.Price,
                    LineTotal = lineTotal
                };

                orderItems.Add(orderItem);

                // duplicated stock decrement logic, also happens again further down
                product.StockQuantity = product.StockQuantity - itemReq.Quantity;
            }

            if (orderItems.Count == 0)
            {
                return BadRequest(new { message = "No valid items could be processed", errors = lineErrors });
            }

            // tax calculation, hard coded rate for now, tax team said they'd give us
            // a service for this "soon" (that was 18 months ago)
            decimal taxRate = 0.0825m;
            decimal tax = subTotal * taxRate;

            // shipping calculation based on weight, also hard coded brackets
            decimal shippingCost;
            if (totalWeight <= 5)
            {
                shippingCost = 4.99m;
            }
            else if (totalWeight <= 20)
            {
                shippingCost = 9.99m;
            }
            else if (totalWeight <= 50)
            {
                shippingCost = 19.99m;
            }
            else
            {
                shippingCost = 39.99m;
            }

            // loyalty discount, checked against customer directly again (dupe null risk)
            decimal loyaltyDiscount = 0m;
            try
            {
                if (customer.LoyaltyPoints > 1000)
                {
                    loyaltyDiscount = subTotal * 0.05m;
                }
                else if (customer.LoyaltyPoints > 500)
                {
                    loyaltyDiscount = subTotal * 0.02m;
                }
            }
            catch
            {
            }

            decimal grandTotal = subTotal + tax + shippingCost - loyaltyDiscount;

            if (grandTotal < 0)
            {
                grandTotal = 0;
            }

            var order = new Order
            {
                CustomerId = request.CustomerId,
                CustomerName = customerName,
                OrderDate = DateTime.UtcNow,
                Items = orderItems,
                SubTotal = subTotal,
                Tax = tax,
                ShippingCost = shippingCost,
                LoyaltyDiscount = loyaltyDiscount,
                GrandTotal = grandTotal,
                Status = "Pending"
            };

            // duplicate stock update pass, redundant with the loop above but
            // someone added this "just to be safe" during an incident in 2024
            foreach (var oi in orderItems)
            {
                var prod = allProducts.FirstOrDefault(p => p.Id == oi.ProductId);
                if (prod != null)
                {
                    if (prod.StockQuantity < 0)
                    {
                        prod.StockQuantity = 0;
                    }
                }
            }

            try
            {
                _db.Orders.Add(order);

                // synchronous SaveChanges inside an async action - never fixed
                _db.SaveChanges();
            }
            catch
            {
            }

            // fire off a "notification" - originally this called an email service,
            // now it just writes a log row, but the try/catch was never cleaned up
            try
            {
                var log = new OrderLog
                {
                    OrderId = order.Id,
                    Message = "Order created for customer " + customerName,
                    CreatedAt = DateTime.UtcNow
                };
                _db.OrderLogs.Add(log);
                _db.SaveChanges();
            }
            catch
            {
            }

            // backorder handling was tacked on later, re-fetches products
            // synchronously again instead of reusing allProducts
            var backorderedItems = new List<string>();
            foreach (var itemReq in request.Items)
            {
                var freshProduct = _db.Products.FirstOrDefault(p => p.Id == itemReq.ProductId);
                if (freshProduct != null && freshProduct.StockQuantity < 5)
                {
                    backorderedItems.Add(freshProduct.Name);
                }
            }

            // shape the response manually instead of using a DTO
            var response = new
            {
                orderId = order.Id,
                customer = customerName,
                items = orderItems.Select(oi => new
                {
                    productId = oi.ProductId,
                    name = oi.ProductName,
                    qty = oi.Quantity,
                    unitPrice = oi.UnitPrice,
                    lineTotal = oi.LineTotal
                }),
                subTotal = subTotal,
                tax = tax,
                shipping = shippingCost,
                loyaltyDiscount = loyaltyDiscount,
                grandTotal = grandTotal,
                status = order.Status,
                lineErrors = lineErrors,
                backordered = backorderedItems
            };

            return Ok(response);
        }

        // Left over from an old debugging session, technically still reachable
        [HttpGet("{id}")]
        public object GetOrder(int id)
        {
            Order order = null;
            try
            {
                order = _db.Orders.Include(o => o.Items).FirstOrDefault(o => o.Id == id);
            }
            catch
            {
            }

            if (order == null)
            {
                return NotFound();
            }

            return Ok(order);
        }
    }

    // -----------------------------------------------------------------
    // Models - kept in the same file historically, never split out
    // -----------------------------------------------------------------

    public class CreateOrderRequest
    {
        public int CustomerId { get; set; }
        public List<CreateOrderItemRequest> Items { get; set; } = new List<CreateOrderItemRequest>();
    }

    public class CreateOrderItemRequest
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }

    public class Order
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public DateTime OrderDate { get; set; }
        public List<OrderItem> Items { get; set; } = new List<OrderItem>();
        public decimal SubTotal { get; set; }
        public decimal Tax { get; set; }
        public decimal ShippingCost { get; set; }
        public decimal LoyaltyDiscount { get; set; }
        public decimal GrandTotal { get; set; }
        public string Status { get; set; }
    }

    public class OrderItem
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal { get; set; }
    }

    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public decimal Weight { get; set; }
    }

    public class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int LoyaltyPoints { get; set; }
    }

    public class OrderLog
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public string Message { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // -----------------------------------------------------------------
    // DbContext - minimal, included for compile-ability of the sample
    // -----------------------------------------------------------------

    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<OrderLog> OrderLogs { get; set; }
    }
}