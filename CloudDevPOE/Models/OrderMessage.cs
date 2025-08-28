namespace CloudDevPOE.Models
{
    public class OrderMessage
    {
        public string OrderId { get; set; } = string.Empty;
        public string CustomerId { get; set; } = string.Empty;
        public List<string> ProductIds { get; set; } = new();
        public decimal TotalAmount { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public string Status { get; set; } = "Pending";
        public string Message { get; set; } = string.Empty;
    }
}
