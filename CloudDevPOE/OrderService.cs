using CloudDevPOE.Data;
using CloudDevPOE.Models;
using Microsoft.EntityFrameworkCore;

namespace CloudDevPOE.Services
{
    public class OrderService
    {
        private readonly ApplicationDbContext _context;
        private readonly CartService _cartService;

        public OrderService(ApplicationDbContext context, CartService cartService)
        {
            _context = context;
            _cartService = cartService;
        }

        // Create order from cart
        public async Task<(bool Success, string Message, Order? Order)> CreateOrderFromCartAsync(int customerId, string? shippingAddress, string? notes)
        {
            try
            {
                // Get cart with items
                var cart = await _cartService.GetCartWithItemsAsync(customerId);

                if (cart == null || !cart.CartItems.Any())
                {
                    return (false, "Cart is empty", null);
                }

                // Generate unique order number
                string orderNumber = $"ORD-{DateTime.UtcNow:yyyyMMddHHmmss}-{customerId}";

                // Create order
                var order = new Order
                {
                    OrderNumber = orderNumber,
                    CustomerId = customerId,
                    OrderDate = DateTime.UtcNow,
                    TotalAmount = cart.TotalAmount,
                    Status = "PENDING",
                    ShippingAddress = shippingAddress,
                    Notes = notes
                };

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                // Create order items from cart items
                foreach (var cartItem in cart.CartItems)
                {
                    var orderItem = new OrderItem
                    {
                        OrderId = order.OrderId,
                        ProductId = cartItem.ProductId,
                        ProductName = cartItem.ProductName,
                        Price = cartItem.Price,
                        Quantity = cartItem.Quantity,
                        Subtotal = cartItem.Subtotal
                    };

                    _context.OrderItems.Add(orderItem);
                }

                await _context.SaveChangesAsync();

                // Clear the cart
                await _cartService.ClearCartAsync(customerId);

                return (true, "Order placed successfully", order);
            }
            catch (Exception ex)
            {
                return (false, $"Failed to create order: {ex.Message}", null);
            }
        }

        // Get order by ID
        public async Task<Order?> GetOrderByIdAsync(int orderId)
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                .Include(o => o.Customer)
                .ThenInclude(c => c!.User)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);
        }

        // Get order by order number
        public async Task<Order?> GetOrderByNumberAsync(string orderNumber)
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                .Include(o => o.Customer)
                .ThenInclude(c => c!.User)
                .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber);
        }

        // Get all orders for a customer
        public async Task<List<Order>> GetCustomerOrdersAsync(int customerId)
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                .Where(o => o.CustomerId == customerId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        // Get all orders (for admin)
        public async Task<List<Order>> GetAllOrdersAsync()
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                .Include(o => o.Customer)
                .ThenInclude(c => c!.User)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        // Get orders by status
        public async Task<List<Order>> GetOrdersByStatusAsync(string status)
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                .Include(o => o.Customer)
                .ThenInclude(c => c!.User)
                .Where(o => o.Status == status)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        // Update order status (Admin only)
        public async Task<(bool Success, string Message)> UpdateOrderStatusAsync(int orderId, string newStatus, int adminUserId)
        {
            try
            {
                var order = await _context.Orders.FindAsync(orderId);

                if (order == null)
                {
                    return (false, "Order not found");
                }

                // Validate status
                var validStatuses = new[] { "PENDING", "PROCESSED", "SHIPPED", "DELIVERED", "CANCELLED" };
                if (!validStatuses.Contains(newStatus.ToUpper()))
                {
                    return (false, "Invalid order status");
                }

                order.Status = newStatus.ToUpper();

                if (newStatus.ToUpper() == "PROCESSED")
                {
                    order.ProcessedDate = DateTime.UtcNow;
                    order.ProcessedBy = adminUserId;
                }

                await _context.SaveChangesAsync();
                return (true, "Order status updated successfully");
            }
            catch (Exception ex)
            {
                return (false, $"Failed to update order status: {ex.Message}");
            }
        }

        // Cancel order (Customer can cancel pending orders)
        public async Task<(bool Success, string Message)> CancelOrderAsync(int orderId, int customerId)
        {
            try
            {
                var order = await _context.Orders
                    .FirstOrDefaultAsync(o => o.OrderId == orderId && o.CustomerId == customerId);

                if (order == null)
                {
                    return (false, "Order not found");
                }

                if (order.Status != "PENDING")
                {
                    return (false, "Only pending orders can be cancelled");
                }

                order.Status = "CANCELLED";
                await _context.SaveChangesAsync();

                return (true, "Order cancelled successfully");
            }
            catch (Exception ex)
            {
                return (false, $"Failed to cancel order: {ex.Message}");
            }
        }

        // Get order statistics (for admin dashboard)
        public async Task<OrderStatistics> GetOrderStatisticsAsync()
        {
            var allOrders = await _context.Orders.ToListAsync();

            return new OrderStatistics
            {
                TotalOrders = allOrders.Count,
                PendingOrders = allOrders.Count(o => o.Status == "PENDING"),
                ProcessedOrders = allOrders.Count(o => o.Status == "PROCESSED"),
                ShippedOrders = allOrders.Count(o => o.Status == "SHIPPED"),
                DeliveredOrders = allOrders.Count(o => o.Status == "DELIVERED"),
                CancelledOrders = allOrders.Count(o => o.Status == "CANCELLED"),
                TotalRevenue = allOrders.Where(o => o.Status != "CANCELLED").Sum(o => o.TotalAmount)
            };
        }
    }

    // Helper class for order statistics
    public class OrderStatistics
    {
        public int TotalOrders { get; set; }
        public int PendingOrders { get; set; }
        public int ProcessedOrders { get; set; }
        public int ShippedOrders { get; set; }
        public int DeliveredOrders { get; set; }
        public int CancelledOrders { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}