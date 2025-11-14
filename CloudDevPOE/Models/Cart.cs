using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CloudDevPOE.Models
{
    [Table("Cart")] // Maps this class to the "Cart" table
    public class Cart
    {
        [Key] // Primary key for the Cart table
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // Auto-increment identity
        public int CartId { get; set; }

        [Required] // Foreign key referencing Customer
        public int CustomerId { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow; // Date the cart was created

        public DateTime UpdatedDate { get; set; } = DateTime.UtcNow; // Last time the cart was updated

        // Navigation properties

        [ForeignKey("CustomerId")] // Links this cart to a specific Customer
        public virtual Customer? Customer { get; set; }

        public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>(); // Collection of items in the cart

        // Calculated property - not stored in the database
        [NotMapped]
        public decimal TotalAmount => CartItems.Sum(item => item.Price * item.Quantity); // Calculates total cart cost

        [NotMapped]
        public int TotalItems => CartItems.Sum(item => item.Quantity); // Calculates total number of items in the cart
    }
}
