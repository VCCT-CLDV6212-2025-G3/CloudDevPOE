using CloudDevPOE.Data;
using CloudDevPOE.Models;
using Microsoft.EntityFrameworkCore;

namespace CloudDevPOE.Services
{
    public class CartService
    {
        private readonly ApplicationDbContext _context;

        public CartService(ApplicationDbContext context)
        {
            _context = context; // Inject DbContext for database access
        }

        // Get existing cart or create a new cart for a customer
        public async Task<Cart> GetOrCreateCartAsync(int customerId)
        {
            var cart = await _context.Carts
                .Include(c => c.CartItems) // Include cart items in query
                .FirstOrDefaultAsync(c => c.CustomerId == customerId);

            if (cart == null)
            {
                // Create a new cart if none exists
                cart = new Cart
                {
                    CustomerId = customerId,
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow
                };

                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            return cart;
        }

        // Add an item to the customer's cart
        public async Task<(bool Success, string Message)> AddToCartAsync(int customerId, string productId, string productName, decimal price, int quantity, string? imageUrl)
        {
            try
            {
                var cart = await GetOrCreateCartAsync(customerId);

                // Check if item already exists in cart
                var existingItem = await _context.CartItems
                    .FirstOrDefaultAsync(ci => ci.CartId == cart.CartId && ci.ProductId == productId);

                if (existingItem != null)
                {
                    // Update quantity if item exists
                    existingItem.Quantity += quantity;
                    cart.UpdatedDate = DateTime.UtcNow;
                }
                else
                {
                    // Add new cart item if not exists
                    var cartItem = new CartItem
                    {
                        CartId = cart.CartId,
                        ProductId = productId,
                        ProductName = productName,
                        Price = price,
                        Quantity = quantity,
                        ImageUrl = imageUrl,
                        AddedDate = DateTime.UtcNow
                    };

                    _context.CartItems.Add(cartItem);
                    cart.UpdatedDate = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();
                return (true, "Item added to cart successfully");
            }
            catch (Exception ex)
            {
                return (false, $"Failed to add item to cart: {ex.Message}");
            }
        }

        // Update quantity of a cart item
        public async Task<(bool Success, string Message)> UpdateCartItemQuantityAsync(int cartItemId, int quantity)
        {
            try
            {
                var cartItem = await _context.CartItems
                    .Include(ci => ci.Cart) // Include parent cart to update timestamp
                    .FirstOrDefaultAsync(ci => ci.CartItemId == cartItemId);

                if (cartItem == null)
                {
                    return (false, "Cart item not found");
                }

                if (quantity <= 0)
                {
                    return (false, "Quantity must be greater than 0");
                }

                cartItem.Quantity = quantity;
                if (cartItem.Cart != null)
                {
                    cartItem.Cart.UpdatedDate = DateTime.UtcNow; // Update cart timestamp
                }

                await _context.SaveChangesAsync();
                return (true, "Cart updated successfully");
            }
            catch (Exception ex)
            {
                return (false, $"Failed to update cart: {ex.Message}");
            }
        }

        // Remove an item from the cart
        public async Task<(bool Success, string Message)> RemoveFromCartAsync(int cartItemId)
        {
            try
            {
                var cartItem = await _context.CartItems
                    .Include(ci => ci.Cart) // Include parent cart
                    .FirstOrDefaultAsync(ci => ci.CartItemId == cartItemId);

                if (cartItem == null)
                {
                    return (false, "Cart item not found");
                }

                _context.CartItems.Remove(cartItem);

                if (cartItem.Cart != null)
                {
                    cartItem.Cart.UpdatedDate = DateTime.UtcNow; // Update cart timestamp
                }

                await _context.SaveChangesAsync();
                return (true, "Item removed from cart");
            }
            catch (Exception ex)
            {
                return (false, $"Failed to remove item: {ex.Message}");
            }
        }

        // Get cart along with its items for a specific customer
        public async Task<Cart?> GetCartWithItemsAsync(int customerId)
        {
            return await _context.Carts
                .Include(c => c.CartItems) // Include items
                .FirstOrDefaultAsync(c => c.CustomerId == customerId);
        }

        // Clear all items from a customer's cart
        public async Task<(bool Success, string Message)> ClearCartAsync(int customerId)
        {
            try
            {
                var cart = await _context.Carts
                    .Include(c => c.CartItems) // Include items to remove
                    .FirstOrDefaultAsync(c => c.CustomerId == customerId);

                if (cart == null)
                {
                    return (true, "Cart is already empty");
                }

                _context.CartItems.RemoveRange(cart.CartItems);
                cart.UpdatedDate = DateTime.UtcNow; // Update cart timestamp
                await _context.SaveChangesAsync();

                return (true, "Cart cleared successfully");
            }
            catch (Exception ex)
            {
                return (false, $"Failed to clear cart: {ex.Message}");
            }
        }

        // Get total quantity of items in the cart for a customer
        public async Task<int> GetCartItemCountAsync(int customerId)
        {
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.CustomerId == customerId);

            return cart?.CartItems.Sum(ci => ci.Quantity) ?? 0; // Return 0 if cart is empty
        }
    }
}
