using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CloudDevPOE.Models
{
    [Table("Cart")]
    public class Cart
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CartId { get; set; }

        [Required]
        public int CustomerId { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedDate { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("CustomerId")]
        public virtual Customer? Customer { get; set; }

        public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

        // Calculated property
        [NotMapped]
        public decimal TotalAmount => CartItems.Sum(item => item.Price * item.Quantity);

        [NotMapped]
        public int TotalItems => CartItems.Sum(item => item.Quantity);
    }
}
