using System.ComponentModel.DataAnnotations;
namespace CloudDevPOE.Models
{
    public class Contract
    {
        public string ContractId { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string ContractName { get; set; } = string.Empty;

        [Required]
        public string CustomerId { get; set; } = string.Empty;

        [Required]
        public string ContractType { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? ExpiryDate { get; set; }
        public string Status { get; set; } = "Draft";
        public string FilePath { get; set; } = string.Empty;
    }
}
