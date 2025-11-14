using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CloudDevPOE.Services;
using System.Security.Claims;

namespace CloudDevPOE.Controllers
{
    // All actions require authentication
    [Authorize]
    public class OrderController : Controller
    {
        private readonly OrderService _orderService;
        private readonly CartService _cartService;

        // Constructor injection of services
        public OrderController(OrderService orderService, CartService cartService)
        {
            _orderService = orderService;
            _cartService = cartService;
        }

        // Helper method to get the current customer's ID from claims
        private int GetCustomerId()
        {
            var customerIdClaim = User.FindFirst("CustomerId")?.Value;
            return int.Parse(customerIdClaim ?? "0"); // Defaults to 0 if claim not found
        }

        // Helper method to get the current user's ID from claims
        private int GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.Parse(userIdClaim ?? "0");
        }

        // Helper method to check if current user is an Admin
        private bool IsAdmin()
        {
            return User.IsInRole("Admin");
        }

        // ==================== CUSTOMER ACTIONS ====================

        // GET: Order/Index
        // Shows the authenticated customer's order history
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> Index()
        {
            try
            {
                int customerId = GetCustomerId();
                var orders = await _orderService.GetCustomerOrdersAsync(customerId);
                return View(orders);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error loading orders: {ex.Message}";
                return View(new List<CloudDevPOE.Models.Order>());
            }
        }

        // GET: Order/Details/{id}
        // Shows details of a specific order
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var order = await _orderService.GetOrderByIdAsync(id);

                if (order == null)
                {
                    TempData["Error"] = "Order not found";
                    return RedirectToAction(nameof(Index));
                }

                // Ensure customer can only view their own orders
                if (!IsAdmin() && order.CustomerId != GetCustomerId())
                {
                    TempData["Error"] = "You do not have permission to view this order";
                    return RedirectToAction(nameof(Index));
                }

                return View(order);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error loading order details: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Order/PlaceOrder
        // Places a new order from the customer's cart
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> PlaceOrder(string? shippingAddress, string? notes)
        {
            try
            {
                int customerId = GetCustomerId();

                // Ensure cart has items
                var cart = await _cartService.GetCartWithItemsAsync(customerId);
                if (cart == null || !cart.CartItems.Any())
                {
                    TempData["Error"] = "Your cart is empty";
                    return RedirectToAction("Index", "Cart");
                }

                // Create order via OrderService
                var result = await _orderService.CreateOrderFromCartAsync(customerId, shippingAddress, notes);

                if (result.Success && result.Order != null)
                {
                    TempData["Success"] = $"Order {result.Order.OrderNumber} placed successfully!";
                    return RedirectToAction(nameof(OrderConfirmation), new { id = result.Order.OrderId });
                }
                else
                {
                    TempData["Error"] = result.Message;
                    return RedirectToAction("Checkout", "Cart");
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error placing order: {ex.Message}";
                return RedirectToAction("Checkout", "Cart");
            }
        }

        // GET: Order/OrderConfirmation/{id}
        // Shows the confirmation page for a placed order
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> OrderConfirmation(int id)
        {
            try
            {
                var order = await _orderService.GetOrderByIdAsync(id);

                if (order == null)
                {
                    TempData["Error"] = "Order not found";
                    return RedirectToAction(nameof(Index));
                }

                // Ensure customer can only view their own order
                if (order.CustomerId != GetCustomerId())
                {
                    TempData["Error"] = "You do not have permission to view this order";
                    return RedirectToAction(nameof(Index));
                }

                return View(order);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error loading order confirmation: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Order/CancelOrder/{id}
        // Allows a customer to cancel a pending order
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> CancelOrder(int id)
        {
            try
            {
                int customerId = GetCustomerId();
                var result = await _orderService.CancelOrderAsync(id, customerId);

                if (result.Success)
                    TempData["Success"] = result.Message;
                else
                    TempData["Error"] = result.Message;

                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error cancelling order: {ex.Message}";
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        // ==================== ADMIN ACTIONS ====================

        // GET: Order/AdminDashboard
        // Displays all orders to admin, optionally filtered by status
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminDashboard(string? status = null)
        {
            try
            {
                List<CloudDevPOE.Models.Order> orders;

                // Get all orders or filter by status
                if (string.IsNullOrEmpty(status))
                    orders = await _orderService.GetAllOrdersAsync();
                else
                    orders = await _orderService.GetOrdersByStatusAsync(status);

                // Retrieve order statistics for dashboard
                var statistics = await _orderService.GetOrderStatisticsAsync();
                ViewBag.Statistics = statistics;
                ViewBag.SelectedStatus = status;

                return View(orders);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error loading dashboard: {ex.Message}";
                return View(new List<CloudDevPOE.Models.Order>());
            }
        }

        // POST: Order/UpdateStatus
        // Allows admin to update the status of an order
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateStatus(int orderId, string newStatus)
        {
            try
            {
                int adminUserId = GetUserId();
                var result = await _orderService.UpdateOrderStatusAsync(orderId, newStatus, adminUserId);

                if (result.Success)
                    TempData["Success"] = result.Message;
                else
                    TempData["Error"] = result.Message;

                return RedirectToAction(nameof(Details), new { id = orderId });
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error updating order status: {ex.Message}";
                return RedirectToAction(nameof(Details), new { id = orderId });
            }
        }

        // GET: Order/AdminOrderDetails/{id}
        // Admin view of a single order with full details
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminOrderDetails(int id)
        {
            try
            {
                var order = await _orderService.GetOrderByIdAsync(id);

                if (order == null)
                {
                    TempData["Error"] = "Order not found";
                    return RedirectToAction(nameof(AdminDashboard));
                }

                return View(order);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error loading order details: {ex.Message}";
                return RedirectToAction(nameof(AdminDashboard));
            }
        }
    }
}
