using System.ComponentModel.DataAnnotations;

namespace CloudDevPOE.Models
{
    public class Contract
    {
        // Unique identifier for the contract
        public string ContractId { get; set; } = string.Empty;

        // Name of the contract (required, max length 100)
        [Required]
        [StringLength(100)]
        public string ContractName { get; set; } = string.Empty;

        // Associated customer ID (required)
        [Required]
        public string CustomerId { get; set; } = string.Empty;

        // Type of contract (e.g., Service, NDA) - required
        [Required]
        public string ContractType { get; set; } = string.Empty;

        // Date when the contract was created (default to current UTC time)
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // Optional expiry date
        public DateTime? ExpiryDate { get; set; }

        // Contract status (default "Draft")
        public string Status { get; set; } = "Draft";

        // File path in Azure File Storage or other storage location
        public string FilePath { get; set; } = string.Empty;
    }
}
