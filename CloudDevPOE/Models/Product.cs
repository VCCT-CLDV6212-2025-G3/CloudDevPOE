using Azure;
using Azure.Data.Tables;
using System.ComponentModel.DataAnnotations;

namespace CloudDevPOE.Models
{
    public class Product : ITableEntity
    {
        public string PartitionKey { get; set; } = "Product";
        public string RowKey { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string ProductName { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public double Price { get; set; }  // Changed to double to match Azure Table Storage

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Stock quantity cannot be negative")]
        public int StockQuantity { get; set; }

        [Required]
        public string Category { get; set; } = string.Empty;

        public string ImageUrl { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public bool IsAvailable { get; set; } = true;
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        // Helper property for display purposes (converts double to decimal for currency display)
        [NotMapped]
        public decimal PriceAsDecimal
        {
            get => (decimal)Price;
            set => Price = (double)value;
        }
    }

    // Custom attribute to ignore properties during serialization
    public class NotMappedAttribute : Attribute
    {
    }
}