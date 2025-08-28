using Azure;
using Azure.Data.Tables;
using System.ComponentModel.DataAnnotations;

namespace CloudDevPOE.Models
{
    public class Product : ITableEntity
    {
        // Partition key used by Azure Table Storage
        public string PartitionKey { get; set; } = "Product";

        // Row key (unique identifier for each product)
        public string RowKey { get; set; } = string.Empty;

        // Name of the product (required, max length 100)
        [Required]
        [StringLength(100)]
        public string ProductName { get; set; } = string.Empty;

        // Optional product description
        public string Description { get; set; } = string.Empty;

        // Price of the product (required, must be greater than 0)
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public double Price { get; set; }

        // Stock quantity (required, cannot be negative)
        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Stock quantity cannot be negative")]
        public int StockQuantity { get; set; }

        // Category of the product (required)
        [Required]
        public string Category { get; set; } = string.Empty;

        // Optional URL for product image
        public string ImageUrl { get; set; } = string.Empty;

        // Date the product was created
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // Whether the product is currently available
        public bool IsAvailable { get; set; } = true;

        // Timestamp managed by Azure Table Storage
        public DateTimeOffset? Timestamp { get; set; }

        // ETag for concurrency control in Azure Table Storage
        public ETag ETag { get; set; }

        // Helper property for displaying price as decimal
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