using Azure.Storage.Files.Shares;
using Azure.Storage.Files.Shares.Models;
using CloudDevPOE.Models;

namespace CloudDevPOE.Services
{
    public class AzureFileService
    {
        // Client for interacting with Azure File Shares
        private readonly ShareServiceClient _shareServiceClient;

        // Default file share and directory names
        private readonly string _shareName = "contracts";
        private readonly string _contractsDirectory = "customer-contracts";

        // Constructor initializes ShareServiceClient using connection string
        public AzureFileService(string connectionString)
        {
            _shareServiceClient = new ShareServiceClient(connectionString);
        }

        // Uploads a contract file to the default contracts directory
        public async Task<string> UploadContractAsync(Stream contractStream, string fileName, Contract contractInfo)
        {
            var shareClient = _shareServiceClient.GetShareClient(_shareName);
            await shareClient.CreateIfNotExistsAsync(); // Create share if it doesn't exist

            var directoryClient = shareClient.GetDirectoryClient(_contractsDirectory);
            await directoryClient.CreateIfNotExistsAsync(); // Create directory if it doesn't exist

            var fileClient = directoryClient.GetFileClient(fileName);
            await fileClient.CreateAsync(contractStream.Length); // Create file with correct length
            await fileClient.UploadAsync(contractStream); // Upload content

            return $"{_shareName}/{_contractsDirectory}/{fileName}"; // Return file path
        }

        // Downloads a contract file from the default contracts directory
        public async Task<Stream> DownloadContractAsync(string filePath)
        {
            var shareClient = _shareServiceClient.GetShareClient(_shareName);
            var directoryClient = shareClient.GetDirectoryClient(_contractsDirectory);

            // Extract just the filename from the path
            var fileName = filePath.Contains("/") ? filePath.Split('/').Last() : filePath;
            var fileClient = directoryClient.GetFileClient(fileName);

            var response = await fileClient.DownloadAsync(); // Download file
            return response.Value.Content;
        }

        // Retrieves a list of all files in the default contracts directory
        public async Task<List<string>> GetAllContractFilesAsync()
        {
            var shareClient = _shareServiceClient.GetShareClient(_shareName);
            var directoryClient = shareClient.GetDirectoryClient(_contractsDirectory);
            var files = new List<string>();

            await foreach (var item in directoryClient.GetFilesAndDirectoriesAsync())
            {
                if (!item.IsDirectory)
                {
                    files.Add(item.Name); // Add only files (skip subdirectories)
                }
            }

            return files;
        }

        // Deletes a specific contract file
        public async Task DeleteContractAsync(string fileName)
        {
            var shareClient = _shareServiceClient.GetShareClient(_shareName);
            var directoryClient = shareClient.GetDirectoryClient(_contractsDirectory);
            var fileClient = directoryClient.GetFileClient(fileName);

            await fileClient.DeleteIfExistsAsync(); // Delete file if it exists
        }

        // Retrieves properties (metadata) of a specific contract file
        public async Task<ShareFileProperties> GetContractPropertiesAsync(string fileName)
        {
            var shareClient = _shareServiceClient.GetShareClient(_shareName);
            var directoryClient = shareClient.GetDirectoryClient(_contractsDirectory);
            var fileClient = directoryClient.GetFileClient(fileName);

            var response = await fileClient.GetPropertiesAsync();
            return response.Value; // Return file properties
        }

        // Creates a directory in the default share
        public async Task CreateDirectoryAsync(string directoryName)
        {
            var shareClient = _shareServiceClient.GetShareClient(_shareName);
            await shareClient.CreateIfNotExistsAsync(); // Ensure share exists

            var directoryClient = shareClient.GetDirectoryClient(directoryName);
            await directoryClient.CreateIfNotExistsAsync(); // Create the directory if needed
        }

        // Uploads a file to a specified directory within the share
        public async Task<string> UploadFileToDirectoryAsync(Stream fileStream, string directoryName, string fileName)
        {
            var shareClient = _shareServiceClient.GetShareClient(_shareName);
            await shareClient.CreateIfNotExistsAsync(); // Ensure share exists

            var directoryClient = shareClient.GetDirectoryClient(directoryName);
            await directoryClient.CreateIfNotExistsAsync(); // Ensure directory exists

            var fileClient = directoryClient.GetFileClient(fileName);
            await fileClient.CreateAsync(fileStream.Length); // Create file
            await fileClient.UploadAsync(fileStream); // Upload content

            return $"{_shareName}/{directoryName}/{fileName}"; // Return path
        }

        // Retrieves all files in a specified directory
        public async Task<List<string>> GetFilesInDirectoryAsync(string directoryName)
        {
            var shareClient = _shareServiceClient.GetShareClient(_shareName);
            var directoryClient = shareClient.GetDirectoryClient(directoryName);
            var files = new List<string>();

            await foreach (var item in directoryClient.GetFilesAndDirectoriesAsync())
            {
                if (!item.IsDirectory)
                {
                    files.Add(item.Name); // Add only files
                }
            }

            return files;
        }
    }
}
