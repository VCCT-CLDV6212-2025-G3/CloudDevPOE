namespace CloudDevPOE.Models
{
    public class ImageProcessingMessage
    {
        // Name of the image file
        public string ImageName { get; set; } = string.Empty;

        // URL where the image is stored (Azure Blob or other storage)
        public string ImageUrl { get; set; } = string.Empty;

        // Type of processing to perform: "RESIZE", "COMPRESS", "WATERMARK", etc.
        public string ProcessingType { get; set; } = string.Empty;

        // Current status of the processing (default "Processing")
        public string Status { get; set; } = "Processing";

        // Timestamp when the message was created
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}