using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CloudDevPOE.Services;
using System.Security.Claims;

namespace CloudDevPOE.Controllers
{
    // Only authenticated users with the "Customer" role can access this controller
    [Authorize(Roles = "Customer")]
    public class CartController : Controller
    {
        private readonly CartService _cartService;
        private readonly AzureTableService _tableService;

        // Constructor injection of required services
        public CartController(CartService cartService, AzureTableService tableService)
        {
            _cartService = cartService;
            _tableService = tableService;
        }

        // Helper method to retrieve the logged-in customer's ID from claims
        private int GetCustomerId()
        {
            var customerIdClaim = User.FindFirst("CustomerId")?.Value;
            return int.Parse(customerIdClaim ?? "0"); // default to 0 if claim not found
        }

        // GET: Cart
        // Display the customer's cart with all cart items
        public async Task<IActionResult> Index()
        {
            try
            {
                int customerId = GetCustomerId();
                var cart = await _cartService.GetCartWithItemsAsync(customerId);

                return View(cart);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error loading cart: {ex.Message}";
                return View(null);
            }
        }

        // POST: Cart/AddToCart
        // Adds a product to the customer's cart
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart(string productId, int quantity = 1)
        {
            try
            {
                int customerId = GetCustomerId();

                // Get product details from Azure Table Storage
                var product = await _tableService.GetProductAsync(productId);

                if (product == null)
                {
                    TempData["Error"] = "Product not found";
                    return RedirectToAction("Index", "Product");
                }

                // Check if the product is available
                if (!product.IsAvailable)
                {
                    TempData["Error"] = "Product is not available";
                    return RedirectToAction("Index", "Product");
                }

                // Check if the requested quantity is within stock limits
                if (product.StockQuantity < quantity)
                {
                    TempData["Error"] = $"Only {product.StockQuantity} items available in stock";
                    return RedirectToAction("Index", "Product");
                }

                // Add the product to the cart via CartService
                var result = await _cartService.AddToCartAsync(
                    customerId,
                    product.RowKey,
                    product.ProductName,
                    (decimal)product.Price,
                    quantity,
                    product.ImageUrl
                );

                // Display success or error messages
                if (result.Success)
                    TempData["Success"] = result.Message;
                else
                    TempData["Error"] = result.Message;

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error adding to cart: {ex.Message}";
                return RedirectToAction("Index", "Product");
            }
        }

        // POST: Cart/UpdateQuantity
        // Updates the quantity of a specific cart item
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateQuantity(int cartItemId, int quantity)
        {
            try
            {
                var result = await _cartService.UpdateCartItemQuantityAsync(cartItemId, quantity);

                // Show success or error messages
                if (result.Success)
                    TempData["Success"] = result.Message;
                else
                    TempData["Error"] = result.Message;

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error updating cart: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Cart/RemoveItem
        // Removes a specific item from the cart
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveItem(int cartItemId)
        {
            try
            {
                var result = await _cartService.RemoveFromCartAsync(cartItemId);

                if (result.Success)
                    TempData["Success"] = result.Message;
                else
                    TempData["Error"] = result.Message;

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error removing item: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Cart/Clear
        // Clears all items in the customer's cart
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Clear()
        {
            try
            {
                int customerId = GetCustomerId();
                var result = await _cartService.ClearCartAsync(customerId);

                if (result.Success)
                    TempData["Success"] = result.Message;
                else
                    TempData["Error"] = result.Message;

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error clearing cart: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Cart/Checkout
        // Displays the checkout page with cart details
        public async Task<IActionResult> Checkout()
        {
            try
            {
                int customerId = GetCustomerId();
                var cart = await _cartService.GetCartWithItemsAsync(customerId);

                // Ensure the cart has items before checkout
                if (cart == null || !cart.CartItems.Any())
                {
                    TempData["Error"] = "Your cart is empty";
                    return RedirectToAction(nameof(Index));
                }

                return View(cart);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error loading checkout: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Cart/GetCartItemCount (AJAX)
        // Returns the total number of items in the cart as JSON
        [HttpGet]
        public async Task<IActionResult> GetCartItemCount()
        {
            try
            {
                int customerId = GetCustomerId();
                int count = await _cartService.GetCartItemCountAsync(customerId);
                return Json(new { count });
            }
            catch
            {
                return Json(new { count = 0 });
            }
        }
    }
}
