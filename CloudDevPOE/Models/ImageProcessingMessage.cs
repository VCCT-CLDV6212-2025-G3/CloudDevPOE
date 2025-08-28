namespace CloudDevPOE.Models
{
    public class ImageProcessingMessage
    {
        public string ImageName { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string ProcessingType { get; set; } = string.Empty; // "RESIZE", "COMPRESS", "WATERMARK"
        public string Status { get; set; } = "Processing";
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
