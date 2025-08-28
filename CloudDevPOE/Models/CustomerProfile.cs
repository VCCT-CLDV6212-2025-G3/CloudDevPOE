using Azure;
using Azure.Data.Tables;
using System.ComponentModel.DataAnnotations;

namespace CloudDevPOE.Models
{
    public class CustomerProfile : ITableEntity
    {
        // Partition key used by Azure Table Storage
        public string PartitionKey { get; set; } = "Customer";

        // Row key (unique identifier for each customer)
        public string RowKey { get; set; } = string.Empty;

        // Customer's first name (required, max length 50)
        [Required]
        [StringLength(50)]
        public string FirstName { get; set; } = string.Empty;

        // Customer's last name (required, max length 50)
        [Required]
        [StringLength(50)]
        public string LastName { get; set; } = string.Empty;

        // Customer email address (required, must be valid email format)
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        // Customer phone number (optional, must be valid phone format if provided)
        [Phone]
        public string PhoneNumber { get; set; } = string.Empty;

        // Customer physical address (optional)
        public string Address { get; set; } = string.Empty;

        // Date the customer profile was created
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // Whether the customer is currently active
        public bool IsActive { get; set; } = true;

        // Timestamp managed by Azure Table Storage
        public DateTimeOffset? Timestamp { get; set; }

        // ETag for concurrency control in Azure Table Storage
        public ETag ETag { get; set; }
    }
}
