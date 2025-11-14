using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CloudDevPOE.Models
{
    [Table("Customers")] // Maps this class to the "Customers" table in the database
    public class Customer
    {
        [Key] // Primary key for the Customers table
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // Auto-increment identity column
        public int CustomerId { get; set; }

        [Required] // Foreign key linking to the Users table
        public int UserId { get; set; }

        [Required] // First name is required
        [StringLength(50)] // Maximum length of 50 characters
        public string FirstName { get; set; } = string.Empty;

        [Required] // Last name is required
        [StringLength(50)] // Maximum length of 50 characters
        public string LastName { get; set; } = string.Empty;

        [Phone] // Ensures valid phone number format
        [StringLength(20)] // Max length of 20 characters
        public string? PhoneNumber { get; set; }

        [StringLength(500)] // Max length of 500 characters
        public string? Address { get; set; }

        [StringLength(100)] // Max length of 100 characters
        public string? City { get; set; }

        [StringLength(20)] // Max length of 20 characters
        public string? PostalCode { get; set; }

        [StringLength(100)] // Max length of 100 characters
        public string? Country { get; set; }

        // Navigation properties

        [ForeignKey("UserId")] // Specifies relationship between Customer and User
        public virtual User? User { get; set; } // One-to-one link to User

        public virtual Cart? Cart { get; set; } // One-to-one link to Customer's shopping cart

        public virtual ICollection<Order> Orders { get; set; } = new List<Order>(); // One-to-many relationship: A customer can have many orders
    }
}
