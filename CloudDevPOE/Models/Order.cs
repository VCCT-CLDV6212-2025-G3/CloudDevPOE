using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CloudDevPOE.Models
{
    [Table("Orders")] // Maps this model to the "Orders" table
    public class Order
    {
        [Key] // Primary key of the Orders table
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // Auto-increment identity column
        public int OrderId { get; set; }

        [Required]
        [StringLength(50)] // Max length of 50 characters
        public string OrderNumber { get; set; } = string.Empty; // Unique order reference number

        [Required] // Foreign key linking to Customer
        public int CustomerId { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.UtcNow; // Date the order was created

        [Required]
        [Column(TypeName = "decimal(18,2)")] // Proper decimal precision/scale in SQL
        public decimal TotalAmount { get; set; } // Total cost of the order

        [Required]
        [StringLength(50)] // Max length of 50 characters
        public string Status { get; set; } = "PENDING"; // Order status: PENDING, PROCESSED, SHIPPED, etc.

        [StringLength(500)] // Max length of 500 characters
        public string? ShippingAddress { get; set; } // Optional shipping address

        public string? Notes { get; set; } // Optional additional order notes

        public DateTime? ProcessedDate { get; set; } // When the order was processed (nullable)

        public int? ProcessedBy { get; set; } // UserId of the admin who processed the order (nullable)

        // Navigation properties

        [ForeignKey("CustomerId")] // Defines relation to Customer
        public virtual Customer? Customer { get; set; }

        [ForeignKey("ProcessedBy")] // Defines relation to User who processed the order
        public virtual User? ProcessedByUser { get; set; }

        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>(); // Collection of items in the order
    }
}
