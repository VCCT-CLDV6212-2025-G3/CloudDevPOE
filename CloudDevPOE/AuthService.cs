using System;
using System.Linq;
using System.Threading.Tasks;
using CloudDevPOE.Data;
using CloudDevPOE.Models;
using CloudDevPOE.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;
using Org.BouncyCastle.Crypto.Generators;

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
            try
            {
                // Check if email already exists
                if (await _context.Users.AnyAsync(u => u.Email == model.Email))
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

                // Create user with hashed password
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

                // Create customer profile
                var customer = new Customer
                {
                    UserId = user.UserId,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    PhoneNumber = model.PhoneNumber,
                    Address = model.Address,
                    City = model.City,
                    PostalCode = model.PostalCode,
                    Country = model.Country
                };

                _context.Customers.Add(customer);
                await _context.SaveChangesAsync();

                // Create empty cart for customer
                var cart = new Cart
                {
                    CustomerId = customer.CustomerId,
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow
                };

                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();

                return (true, "Registration successful", user);
            }
            catch (Exception ex)
            {
                return (false, $"Registration failed: {ex.Message}", null);
            }
        }

        // Authenticate user login
        public async Task<(bool Success, string Message, User? User, Customer? Customer)> LoginAsync(LoginViewModel model)
        {
            try
            {
                // Find user by email
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == model.Email && u.IsActive);

                if (user == null)
                {
                    return (false, "Invalid email or password", null, null);
                }

                // Verify password using BCrypt
                if (!BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
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
                }

                return (true, "Login successful", user, customer);
            }
            catch (Exception ex)
            {
                return (false, $"Login failed: {ex.Message}", null, null);
            }
        }

        // Create admin user (for initial setup or manual admin creation)
        public async Task<(bool Success, string Message)> CreateAdminAsync(string email, string password, string firstName, string lastName)
        {
            try
            {
                // Check if email already exists
                if (await _context.Users.AnyAsync(u => u.Email == email))
                {
                    return (false, "Email is already registered");
                }

                // Create username from email
                string username = email.Split('@')[0] + "_admin";

                // Create admin user with hashed password
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

                // Create customer profile for admin (optional, for consistency)
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