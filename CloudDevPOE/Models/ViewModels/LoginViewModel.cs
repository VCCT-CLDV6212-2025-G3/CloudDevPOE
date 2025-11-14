using System.ComponentModel.DataAnnotations;

namespace CloudDevPOE.Models.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Email is required")] // Email is mandatory
        [EmailAddress(ErrorMessage = "Invalid email address")] // Must be in valid email format
        [Display(Name = "Email Address")] // Label used in forms/views
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")] // Password is mandatory
        [DataType(DataType.Password)] // Input should be treated as a password (masked in forms)
        [Display(Name = "Password")] // Label used in forms/views
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Remember Me")] // Label for the checkbox in login forms
        public bool RememberMe { get; set; } // Indicates whether to keep the user logged in
    }
}
