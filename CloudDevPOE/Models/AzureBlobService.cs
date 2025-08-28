using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace CloudDevPOE.Services
{
    public class AzureBlobService
    {
        // Client for interacting with Azure Blob Storage
        private readonly BlobServiceClient _blobServiceClient;

        // Name of the container used to store all multimedia files
        private readonly string _containerName = "multimedia";

        // Constructor initializes the BlobServiceClient using the connection string
        public AzureBlobService(string connectionString)
        {
            _blobServiceClient = new BlobServiceClient(connectionString);
        }

        // ===================== Upload Operations =====================

        // Upload an image to the blob container
        public async Task<string> UploadImageAsync(Stream imageStream, string fileName, string contentType = "image/jpeg")
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            // Create container if it does not exist, allow public access for blobs
            await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

            // Create a unique blob name inside the 'images' folder
            var blobName = $"images/{Guid.NewGuid()}_{fileName}";
            var blobClient = containerClient.GetBlobClient(blobName);

            // Set content type for the blob
            var blobHeaders = new BlobHttpHeaders
            {
                ContentType = contentType
            };

            // Upload the stream to the blob with specified headers
            await blobClient.UploadAsync(imageStream, new BlobUploadOptions
            {
                HttpHeaders = blobHeaders
            });

            // Return the full URI of the uploaded blob
            return blobClient.Uri.ToString();
        }

        // Upload a video to the blob container
        public async Task<string> UploadVideoAsync(Stream videoStream, string fileName, string contentType = "video/mp4")
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

            var blobName = $"videos/{Guid.NewGuid()}_{fileName}";
            var blobClient = containerClient.GetBlobClient(blobName);

            var blobHeaders = new BlobHttpHeaders
            {
                ContentType = contentType
            };

            await blobClient.UploadAsync(videoStream, new BlobUploadOptions
            {
                HttpHeaders = blobHeaders
            });

            return blobClient.Uri.ToString();
        }

        // Upload a document to the blob container
        public async Task<string> UploadDocumentAsync(Stream documentStream, string fileName, string contentType)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

            var blobName = $"documents/{Guid.NewGuid()}_{fileName}";
            var blobClient = containerClient.GetBlobClient(blobName);

            var blobHeaders = new BlobHttpHeaders
            {
                ContentType = contentType
            };

            await blobClient.UploadAsync(documentStream, new BlobUploadOptions
            {
                HttpHeaders = blobHeaders
            });

            return blobClient.Uri.ToString();
        }

        // ===================== Download Operation =====================

        // Download a blob given its URL
        public async Task<Stream> DownloadBlobAsync(string blobUrl)
        {
            var uri = new Uri(blobUrl);
            var blobName = uri.Segments[^1]; // Extract the filename from the URL

            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blobClient = containerClient.GetBlobClient(blobName);

            var response = await blobClient.DownloadStreamingAsync();
            return response.Value.Content; // Return the stream content
        }

        // ===================== Listing Operations =====================

        // Retrieve URLs for all blobs in the container
        public async Task<List<string>> GetAllBlobUrlsAsync()
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blobUrls = new List<string>();

            // Iterate through all blobs in the container
            await foreach (var blobItem in containerClient.GetBlobsAsync())
            {
                var blobClient = containerClient.GetBlobClient(blobItem.Name);
                blobUrls.Add(blobClient.Uri.ToString());
            }

            return blobUrls;
        }

        // ===================== Delete Operation =====================

        // Delete a blob given its URL
        public async Task DeleteBlobAsync(string blobUrl)
        {
            var uri = new Uri(blobUrl);
            var blobName = uri.Segments[^1]; // Extract the filename from the URL

            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blobClient = containerClient.GetBlobClient(blobName);

            await blobClient.DeleteIfExistsAsync(); // Delete blob if it exists
        }
    }
}
