using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CloudDevPOE.Services;
using System.Security.Claims;

namespace CloudDevPOE.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly OrderService _orderService;
        private readonly CartService _cartService;

        public OrderController(OrderService orderService, CartService cartService)
        {
            _orderService = orderService;
            _cartService = cartService;
        }

        // Helper method to get current customer ID
        private int GetCustomerId()
        {
            var customerIdClaim = User.FindFirst("CustomerId")?.Value;
            return int.Parse(customerIdClaim ?? "0");
        }

        // Helper method to get current user ID
        private int GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.Parse(userIdClaim ?? "0");
        }

        // Helper method to check if user is admin
        private bool IsAdmin()
        {
            return User.IsInRole("Admin");
        }

        // GET: Order/Index - Customer's order history
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

        // GET: Order/Details/5
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

                // Check if user has permission to view this order
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
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> PlaceOrder(string? shippingAddress, string? notes)
        {
            try
            {
                int customerId = GetCustomerId();

                // Validate cart is not empty
                var cart = await _cartService.GetCartWithItemsAsync(customerId);
                if (cart == null || !cart.CartItems.Any())
                {
                    TempData["Error"] = "Your cart is empty";
                    return RedirectToAction("Index", "Cart");
                }

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

        // GET: Order/OrderConfirmation/5
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

                // Verify this order belongs to the current customer
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

        // POST: Order/CancelOrder/5
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
                {
                    TempData["Success"] = result.Message;
                }
                else
                {
                    TempData["Error"] = result.Message;
                }

                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error cancelling order: {ex.Message}";
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        // ==================== ADMIN ACTIONS ====================

        // GET: Order/AdminDashboard - Admin view of all orders
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminDashboard(string? status = null)
        {
            try
            {
                List<CloudDevPOE.Models.Order> orders;

                if (string.IsNullOrEmpty(status))
                {
                    orders = await _orderService.GetAllOrdersAsync();
                }
                else
                {
                    orders = await _orderService.GetOrdersByStatusAsync(status);
                }

                // Get statistics
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
                {
                    TempData["Success"] = result.Message;
                }
                else
                {
                    TempData["Error"] = result.Message;
                }

                return RedirectToAction(nameof(Details), new { id = orderId });
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error updating order status: {ex.Message}";
                return RedirectToAction(nameof(Details), new { id = orderId });
            }
        }

        // GET: Order/AdminOrderDetails/5 - Detailed admin view
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