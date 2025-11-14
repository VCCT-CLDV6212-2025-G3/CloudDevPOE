using CloudDevPOE.Data;
using CloudDevPOE.Models;
using CloudDevPOE.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace CloudDevPOE.Services
{
    public class AuthService
    {
        private readonly ApplicationDbContext _context;

        public AuthService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Register a new customer
        public async Task<(bool Success, string Message, User? User)> RegisterCustomerAsync(RegisterViewModel model)
        {
            // Use a transaction to ensure all operations succeed or fail together
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                Console.WriteLine($"[DEBUG] Starting registration for: {model.Email}");

                // Check if email already exists
                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
                if (existingUser != null)
                {
                    return (false, "Email is already registered", null);
                }

                // Create username from email
                string username = model.Email.Split('@')[0];
                int counter = 1;
                string baseUsername = username;

                // Ensure username is unique
                while (await _context.Users.AnyAsync(u => u.Username == username))
                {
                    username = $"{baseUsername}{counter}";
                    counter++;
                }

                Console.WriteLine($"[DEBUG] Creating user: {username}");

                // Step 1: Create user
                var user = new User
                {
                    Username = username,
                    Email = model.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                    Role = "Customer",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                Console.WriteLine($"[DEBUG] ✅ User created with UserId: {user.UserId}");

                // Step 2: Create customer profile
                var customer = new Customer
                {
                    UserId = user.UserId,
                    FirstName = string.IsNullOrWhiteSpace(model.FirstName) ? "Customer" : model.FirstName,
                    LastName = string.IsNullOrWhiteSpace(model.LastName) ? "User" : model.LastName,
                    PhoneNumber = model.PhoneNumber,
                    Address = model.Address,
                    City = model.City,
                    PostalCode = model.PostalCode,
                    Country = model.Country
                };

                _context.Customers.Add(customer);
                await _context.SaveChangesAsync();

                Console.WriteLine($"[DEBUG] ✅ Customer created with CustomerId: {customer.CustomerId}");

                // Step 3: Create empty cart
                var cart = new Cart
                {
                    CustomerId = customer.CustomerId,
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow
                };

                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();

                Console.WriteLine($"[DEBUG] ✅ Cart created with CartId: {cart.CartId}");

                // Commit transaction
                await transaction.CommitAsync();

                Console.WriteLine($"[DEBUG] ✅ Registration complete! UserId={user.UserId}, CustomerId={customer.CustomerId}, CartId={cart.CartId}");

                return (true, "Registration successful", user);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Console.WriteLine($"[ERROR] Registration failed: {ex.Message}");
                Console.WriteLine($"[ERROR] Inner: {ex.InnerException?.Message}");
                return (false, $"Registration failed: {ex.Message}", null);
            }
        }

        // Authenticate user login
        public async Task<(bool Success, string Message, User? User, Customer? Customer)> LoginAsync(LoginViewModel model)
        {
            try
            {
                Console.WriteLine($"[DEBUG] Login attempt for: {model.Email}");

                // Find user by email
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == model.Email && u.IsActive);

                if (user == null)
                {
                    Console.WriteLine($"[DEBUG] User not found: {model.Email}");
                    return (false, "Invalid email or password", null, null);
                }

                Console.WriteLine($"[DEBUG] User found. Verifying password...");

                // Verify password
                bool isPasswordValid = BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash);

                Console.WriteLine($"[DEBUG] Password valid: {isPasswordValid}");

                if (!isPasswordValid)
                {
                    return (false, "Invalid email or password", null, null);
                }

                // Update last login date
                user.LastLoginDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                // Get customer profile if user is a customer
                Customer? customer = null;
                if (user.Role == "Customer")
                {
                    customer = await _context.Customers
                        .FirstOrDefaultAsync(c => c.UserId == user.UserId);

                    Console.WriteLine($"[DEBUG] Customer profile loaded: CustomerId={customer?.CustomerId}");
                }

                Console.WriteLine($"[DEBUG] ✅ Login successful for {user.Email}");
                return (true, "Login successful", user, customer);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Login failed: {ex.Message}");
                return (false, $"Login failed: {ex.Message}", null, null);
            }
        }

        // Create admin user
        public async Task<(bool Success, string Message)> CreateAdminAsync(string email, string password, string firstName, string lastName)
        {
            try
            {
                if (await _context.Users.AnyAsync(u => u.Email == email))
                {
                    return (false, "Email is already registered");
                }

                string username = email.Split('@')[0] + "_admin";

                var user = new User
                {
                    Username = username,
                    Email = email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                    Role = "Admin",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                var customer = new Customer
                {
                    UserId = user.UserId,
                    FirstName = firstName,
                    LastName = lastName
                };

                _context.Customers.Add(customer);
                await _context.SaveChangesAsync();

                return (true, "Admin created successfully");
            }
            catch (Exception ex)
            {
                return (false, $"Failed to create admin: {ex.Message}");
            }
        }

        // Get user by ID
        public async Task<User?> GetUserByIdAsync(int userId)
        {
            return await _context.Users.FindAsync(userId);
        }

        // Get customer by user ID
        public async Task<Customer?> GetCustomerByUserIdAsync(int userId)
        {
            return await _context.Customers
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.UserId == userId);
        }
    }
}