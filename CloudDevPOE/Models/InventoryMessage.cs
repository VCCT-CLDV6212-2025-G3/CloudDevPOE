namespace CloudDevPOE.Models
{
    public class InventoryMessage
    {
        // ID of the product associated with this message
        public string ProductId { get; set; } = string.Empty;

        // Type of action: "UPDATE_STOCK", "LOW_STOCK_ALERT", "REORDER", etc.
        public string Action { get; set; } = string.Empty;

        // Quantity relevant to the action (e.g., updated stock, reorder quantity)
        public int Quantity { get; set; }

        // Optional message or description for the action
        public string Message { get; set; } = string.Empty;

        // Timestamp when the message was created
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
