namespace CloudDevPOE.Models
{
    public class OrderMessage
    {
        // Unique identifier for the order
        public string OrderId { get; set; } = string.Empty;

        // ID of the customer who placed the order
        public string CustomerId { get; set; } = string.Empty;

        // List of product IDs included in the order
        public List<string> ProductIds { get; set; } = new();

        // Total amount for the order
        public decimal TotalAmount { get; set; }

        // Date and time when the order was created
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        // Current status of the order (e.g., "Pending", "Processed", "Shipped")
        public string Status { get; set; } = "Pending";

        // Optional message or notes about the order
        public string Message { get; set; } = string.Empty;
    }
}