using Azure.Storage.Files.Shares;
using Azure.Storage.Files.Shares.Models;
using CloudDevPOE.Models;

namespace CloudDevPOE.Services
{
    public class AzureFileService
    {
        private readonly ShareServiceClient _shareServiceClient;
        private readonly string _shareName = "contracts";
        private readonly string _contractsDirectory = "customer-contracts";

        public AzureFileService(string connectionString)
        {
            _shareServiceClient = new ShareServiceClient(connectionString);
        }

        public async Task<string> UploadContractAsync(Stream contractStream, string fileName, Contract contractInfo)
        {
            var shareClient = _shareServiceClient.GetShareClient(_shareName);
            await shareClient.CreateIfNotExistsAsync();

            var directoryClient = shareClient.GetDirectoryClient(_contractsDirectory);
            await directoryClient.CreateIfNotExistsAsync();

            var fileClient = directoryClient.GetFileClient(fileName);
            await fileClient.CreateAsync(contractStream.Length);
            await fileClient.UploadAsync(contractStream);

            return $"{_shareName}/{_contractsDirectory}/{fileName}";
        }

        public async Task<Stream> DownloadContractAsync(string filePath)
        {
            var shareClient = _shareServiceClient.GetShareClient(_shareName);
            var directoryClient = shareClient.GetDirectoryClient(_contractsDirectory);

            // Extract just the filename from the path
            var fileName = filePath.Contains("/") ? filePath.Split('/').Last() : filePath;
            var fileClient = directoryClient.GetFileClient(fileName);

            var response = await fileClient.DownloadAsync();
            return response.Value.Content;
        }

        public async Task<List<string>> GetAllContractFilesAsync()
        {
            var shareClient = _shareServiceClient.GetShareClient(_shareName);
            var directoryClient = shareClient.GetDirectoryClient(_contractsDirectory);
            var files = new List<string>();

            await foreach (var item in directoryClient.GetFilesAndDirectoriesAsync())
            {
                if (!item.IsDirectory)
                {
                    files.Add(item.Name);
                }
            }

            return files;
        }

        public async Task DeleteContractAsync(string fileName)
        {
            var shareClient = _shareServiceClient.GetShareClient(_shareName);
            var directoryClient = shareClient.GetDirectoryClient(_contractsDirectory);
            var fileClient = directoryClient.GetFileClient(fileName);

            await fileClient.DeleteIfExistsAsync();
        }

        public async Task<ShareFileProperties> GetContractPropertiesAsync(string fileName)
        {
            var shareClient = _shareServiceClient.GetShareClient(_shareName);
            var directoryClient = shareClient.GetDirectoryClient(_contractsDirectory);
            var fileClient = directoryClient.GetFileClient(fileName);

            var response = await fileClient.GetPropertiesAsync();
            return response.Value;
        }

        public async Task CreateDirectoryAsync(string directoryName)
        {
            var shareClient = _shareServiceClient.GetShareClient(_shareName);
            await shareClient.CreateIfNotExistsAsync();

            var directoryClient = shareClient.GetDirectoryClient(directoryName);
            await directoryClient.CreateIfNotExistsAsync();
        }

        public async Task<string> UploadFileToDirectoryAsync(Stream fileStream, string directoryName, string fileName)
        {
            var shareClient = _shareServiceClient.GetShareClient(_shareName);
            await shareClient.CreateIfNotExistsAsync();

            var directoryClient = shareClient.GetDirectoryClient(directoryName);
            await directoryClient.CreateIfNotExistsAsync();

            var fileClient = directoryClient.GetFileClient(fileName);
            await fileClient.CreateAsync(fileStream.Length);
            await fileClient.UploadAsync(fileStream);

            return $"{_shareName}/{directoryName}/{fileName}";
        }

        public async Task<List<string>> GetFilesInDirectoryAsync(string directoryName)
        {
            var shareClient = _shareServiceClient.GetShareClient(_shareName);
            var directoryClient = shareClient.GetDirectoryClient(directoryName);
            var files = new List<string>();

            await foreach (var item in directoryClient.GetFilesAndDirectoriesAsync())
            {
                if (!item.IsDirectory)
                {
                    files.Add(item.Name);
                }
            }

            return files;
        }
    }
}