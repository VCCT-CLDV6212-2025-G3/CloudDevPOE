using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CloudDevPOE.Models
{
    [Table("Users")] // Maps this class to the "Users" table in the database
    public class User
    {
        [Key] // Marks this property as the primary key
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // Auto-increment identity column
        public int UserId { get; set; }

        [Required] // Field must be supplied
        [StringLength(100)] // Maximum length of 100 characters
        public string Username { get; set; } = string.Empty; // Stores the username

        [Required] // Field must be supplied
        [EmailAddress] // Ensures value is in valid email format
        [StringLength(255)] // Max length of 255 characters
        public string Email { get; set; } = string.Empty; // Stores user's email

        [Required] // Field must be supplied
        public string PasswordHash { get; set; } = string.Empty; // Stores the hashed password

        [Required] // Field must be supplied
        [StringLength(50)] // Limits role string to 50 characters
        public string Role { get; set; } = "Customer"; // User role ("Customer" or "Admin")

        public bool IsActive { get; set; } = true; // Indicates if the user account is active

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow; // Timestamp when the account was created

        public DateTime? LastLoginDate { get; set; } // Nullable - stores last login date

        // Navigation property for related Customer record (1-to-1 relationship)
        public virtual Customer? Customer { get; set; }
    }
}