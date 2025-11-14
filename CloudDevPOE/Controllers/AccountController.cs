using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using CloudDevPOE.Models.ViewModels;
using CloudDevPOE.Services;

namespace CloudDevPOE.Controllers
{
    public class AccountController : Controller
    {
        private readonly AuthService _authService;

        // Constructor injects AuthService for user-related operations
        public AccountController(AuthService authService)
        {
            _authService = authService;
        }

        // GET: Account/Login
        // Allows anonymous users to access the login page
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            // Store returnUrl to redirect after successful login
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST: Account/Login
        // Handles login form submission
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {
                // Attempt to login using AuthService
                var result = await _authService.LoginAsync(model);

                if (result.Success && result.User != null)
                {
                    // Create user claims for authentication
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, result.User.UserId.ToString()),
                        new Claim(ClaimTypes.Name, result.User.Username),
                        new Claim(ClaimTypes.Email, result.User.Email),
                        new Claim(ClaimTypes.Role, result.User.Role)
                    };

                    // Add CustomerId claim if the user is a customer
                    if (result.Customer != null)
                    {
                        claims.Add(new Claim("CustomerId", result.Customer.CustomerId.ToString()));
                    }

                    // Create a claims identity using cookie authentication
                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    // Set authentication cookie properties
                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = model.RememberMe, // persistent login if checkbox checked
                        ExpiresUtc = model.RememberMe
                            ? DateTimeOffset.UtcNow.AddDays(30) // remember me duration
                            : DateTimeOffset.UtcNow.AddHours(2)  // default session duration
                    };

                    // Sign in the user with cookies
                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity),
                        authProperties);

                    TempData["Success"] = $"Welcome back, {result.User.Username}!";

                    // Redirect admin users to admin dashboard
                    if (result.User.Role == "Admin")
                    {
                        return RedirectToAction("AdminDashboard", "Order");
                    }

                    // Redirect to returnUrl if valid, otherwise go to home page
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }

                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    // Show login error message
                    ModelState.AddModelError(string.Empty, result.Message);
                }
            }

            // Return to login view if validation fails
            return View(model);
        }

        // GET: Account/Register
        // Displays registration form to anonymous users
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View();
        }

        // POST: Account/Register
        // Handles registration form submission
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Register customer using AuthService
                var result = await _authService.RegisterCustomerAsync(model);

                if (result.Success && result.User != null)
                {
                    // Registration successful, redirect to login page
                    TempData["Success"] = "Registration successful! Please log in.";
                    return RedirectToAction(nameof(Login));
                }
                else
                {
                    // Show registration error message
                    ModelState.AddModelError(string.Empty, result.Message);
                }
            }

            // Return to registration view if validation fails
            return View(model);
        }

        // POST: Account/Logout
        // Logs out the current authenticated user
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize] // Only logged-in users can logout
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            TempData["Success"] = "You have been logged out successfully.";
            return RedirectToAction("Login", "Account");
        }

        // GET: Account/AccessDenied
        // Displays access denied page for unauthorized users
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }

        // GET: Account/Profile
        // Displays profile information for logged-in users
        [Authorize] // Only accessible to authenticated users
        public async Task<IActionResult> Profile()
        {
            // Get UserId from claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                // Redirect to login if UserId is missing
                return RedirectToAction(nameof(Login));
            }

            int userId = int.Parse(userIdClaim);

            // Get customer profile from AuthService
            var customer = await _authService.GetCustomerByUserIdAsync(userId);

            if (customer == null)
            {
                // Return 404 if customer profile not found
                return NotFound();
            }

            return View(customer);
        }
    }
}
