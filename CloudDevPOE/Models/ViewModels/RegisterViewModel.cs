using System.ComponentModel.DataAnnotations;

namespace CloudDevPOE.Models.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "First name is required")] // First name is mandatory
        [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters")] // Max 50 characters
        [Display(Name = "First Name")] // Label used in forms/views
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")] // Last name is mandatory
        [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters")] // Max 50 characters
        [Display(Name = "Last Name")] // Label used in forms/views
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")] // Email is mandatory
        [EmailAddress(ErrorMessage = "Invalid email address")] // Must be in valid email format
        [Display(Name = "Email Address")] // Label used in forms/views
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")] // Password is mandatory
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long")] // Password length constraints
        [DataType(DataType.Password)] // Input treated as password (masked)
        [Display(Name = "Password")] // Label used in forms/views
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please confirm your password")] // Confirmation password is mandatory
        [DataType(DataType.Password)] // Input treated as password (masked)
        [Display(Name = "Confirm Password")] // Label used in forms/views
        [Compare("Password", ErrorMessage = "Passwords do not match")] // Must match the Password field
        public string ConfirmPassword { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Invalid phone number")] // Optional phone number must be valid format
        [Display(Name = "Phone Number")] // Label used in forms/views
        public string? PhoneNumber { get; set; }

        [Display(Name = "Address")] // Optional address
        public string? Address { get; set; }

        [Display(Name = "City")] // Optional city
        public string? City { get; set; }

        [Display(Name = "Postal Code")] // Optional postal code
        public string? PostalCode { get; set; }

        [Display(Name = "Country")] // Optional country
        public string? Country { get; set; }
    }
}
