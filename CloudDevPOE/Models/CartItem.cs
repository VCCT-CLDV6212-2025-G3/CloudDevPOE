using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CloudDevPOE.Models
{
    [Table("CartItems")] // Maps this model to the "CartItems" table
    public class CartItem
    {
        [Key] // Primary key for CartItems
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // Auto-increment identity
        public int CartItemId { get; set; }

        [Required] // Foreign key linking to Cart
        public int CartId { get; set; }

        [Required]
        [StringLength(100)] // Max length of 100 characters
        public string ProductId { get; set; } = string.Empty; // Stores Product RowKey from Azure Table Storage

        [Required]
        [StringLength(200)] // Max length of 200 characters
        public string ProductName { get; set; } = string.Empty; // Name of the product

        [Required]
        [Column(TypeName = "decimal(18,2)")] // Ensures proper precision/scale in SQL
        public decimal Price { get; set; } // Price of one unit of the product

        [Required]
        [Range(1, int.MaxValue)] // Quantity must be at least 1
        public int Quantity { get; set; }

        [StringLength(500)] // Max length of 500 characters
        public string? ImageUrl { get; set; } // Optional image URL for the product

        public DateTime AddedDate { get; set; } = DateTime.UtcNow; // Date item was added to the cart

        // Navigation properties

        [ForeignKey("CartId")] // Links this CartItem to a specific Cart
        public virtual Cart? Cart { get; set; }

        // Calculated property (not stored in DB)
        [NotMapped]
        public decimal Subtotal => Price * Quantity; // Total price for this item (Price × Quantity)
    }
}
