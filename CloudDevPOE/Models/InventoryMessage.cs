namespace CloudDevPOE.Models
{
    public class InventoryMessage
    {
        public string ProductId { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty; // "UPDATE_STOCK", "LOW_STOCK_ALERT", "REORDER"
        public int Quantity { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
