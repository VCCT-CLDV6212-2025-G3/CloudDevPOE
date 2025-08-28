using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace CloudDevPOE.Services
{
    public class AzureBlobService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string _containerName = "multimedia";

        public AzureBlobService(string connectionString)
        {
            _blobServiceClient = new BlobServiceClient(connectionString);
        }

        public async Task<string> UploadImageAsync(Stream imageStream, string fileName, string contentType = "image/jpeg")
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

            var blobName = $"images/{Guid.NewGuid()}_{fileName}";
            var blobClient = containerClient.GetBlobClient(blobName);

            var blobHeaders = new BlobHttpHeaders
            {
                ContentType = contentType
            };

            await blobClient.UploadAsync(imageStream, new BlobUploadOptions
            {
                HttpHeaders = blobHeaders
            });

            return blobClient.Uri.ToString();
        }

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

        public async Task<Stream> DownloadBlobAsync(string blobUrl)
        {
            var uri = new Uri(blobUrl);
            var blobName = uri.Segments[^1];

            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blobClient = containerClient.GetBlobClient(blobName);

            var response = await blobClient.DownloadStreamingAsync();
            return response.Value.Content;
        }

        public async Task<List<string>> GetAllBlobUrlsAsync()
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blobUrls = new List<string>();

            await foreach (var blobItem in containerClient.GetBlobsAsync())
            {
                var blobClient = containerClient.GetBlobClient(blobItem.Name);
                blobUrls.Add(blobClient.Uri.ToString());
            }

            return blobUrls;
        }

        public async Task DeleteBlobAsync(string blobUrl)
        {
            var uri = new Uri(blobUrl);
            var blobName = uri.Segments[^1];

            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blobClient = containerClient.GetBlobClient(blobName);

            await blobClient.DeleteIfExistsAsync();
        }
    }
}
