using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CloudDevPOE.Models
{
    [Table("OrderItems")] // Maps this class to the "OrderItems" table
    public class OrderItem
    {
        [Key] // Primary key for the OrderItems table
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // Auto-increment identity column
        public int OrderItemId { get; set; }

        [Required] // Foreign key linking to the parent Order
        public int OrderId { get; set; }

        [Required]
        [StringLength(100)] // Max length of 100 characters
        public string ProductId { get; set; } = string.Empty; // Identifier of the product

        [Required]
        [StringLength(200)] // Max length of 200 characters
        public string ProductName { get; set; } = string.Empty; // Name of the product

        [Required]
        [Column(TypeName = "decimal(18,2)")] // Ensures proper decimal precision in SQL
        public decimal Price { get; set; } // Price per unit of the product

        [Required] // Quantity of the product ordered
        public int Quantity { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")] // Ensures proper decimal precision in SQL
        public decimal Subtotal { get; set; } // Total for this item (Price × Quantity)

        // Navigation properties
        [ForeignKey("OrderId")] // Links this OrderItem to its parent Order
        public virtual Order? Order { get; set; }
    }
}
