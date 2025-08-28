using Microsoft.AspNetCore.Mvc;
using CloudDevPOE.Models;
using Azure.Storage.Files.Shares;

namespace CloudDevPOE.Controllers
{
    public class ContractController : Controller
    {
        // Azure Storage connection string
        private readonly string _connectionString;
        // Name of the Azure file share
        private readonly string _shareName = "contracts";
        // Directory inside the file share to store customer contracts
        private readonly string _contractsDirectory = "customer-contracts";

        // Constructor injects IConfiguration to get Azure Storage connection string
        public ContractController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("AzureStorage") ?? "";
        }

        // GET: Contract
        // Displays a list of uploaded contracts
        public async Task<IActionResult> Index()
        {
            try
            {
                var shareClient = new ShareClient(_connectionString, _shareName);
                var files = new List<string>();

                // Check if the file share exists
                if (await shareClient.ExistsAsync())
                {
                    var directoryClient = shareClient.GetDirectoryClient(_contractsDirectory);

                    // Create directory if it doesn't exist
                    await directoryClient.CreateIfNotExistsAsync();

                    // Enumerate files in the directory
                    await foreach (var item in directoryClient.GetFilesAndDirectoriesAsync())
                    {
                        if (!item.IsDirectory)
                        {
                            files.Add(item.Name); // Add file name to list
                        }
                    }
                }
                else
                {
                    // Share doesn't exist yet, show info message
                    ViewBag.InfoMessage = "File share will be created when you upload your first contract.";
                }

                return View(files); // Return view with list of files
            }
            catch (Exception ex)
            {
                // Catch errors and display empty list with error message
                ViewBag.ErrorMessage = $"Error loading contracts: {ex.Message}";
                return View(new List<string>());
            }
        }

        // GET: Contract/Upload
        // Returns the upload view with a new Contract object
        public IActionResult Upload()
        {
            var contract = new Contract
            {
                ContractId = Guid.NewGuid().ToString("N")[..8].ToUpper(), // Generate short unique ID
                Status = "Draft" // Set initial status
            };

            ViewBag.Contract = contract; // Pass contract to view
            return View();
        }

        // POST: Contract/Upload
        // Handles file upload and saves it to Azure File Share
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(Contract contract, IFormFile? contractFile)
        {
            // Validate that a file was selected
            if (contractFile == null || contractFile.Length == 0)
            {
                ModelState.AddModelError("contractFile", "Please select a file to upload.");
                ViewBag.Contract = contract;
                return View();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var shareClient = new ShareClient(_connectionString, _shareName);

                    // Create file share if it doesn't exist
                    await shareClient.CreateIfNotExistsAsync();

                    var directoryClient = shareClient.GetDirectoryClient(_contractsDirectory);

                    // Create directory if it doesn't exist
                    await directoryClient.CreateIfNotExistsAsync();

                    // Create a unique file name using contract ID and original file name
                    var fileName = $"{contract.ContractId}_{contractFile.FileName}";
                    var fileClient = directoryClient.GetFileClient(fileName);

                    // Upload file to Azure File Share
                    using var stream = contractFile.OpenReadStream();
                    await fileClient.CreateAsync(stream.Length); // Create file with specified size
                    await fileClient.UploadAsync(stream); // Upload content

                    // Save the file path to the contract
                    contract.FilePath = $"{_shareName}/{_contractsDirectory}/{fileName}";

                    TempData["Success"] = $"Contract '{contract.ContractName}' uploaded successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    // Show error if upload fails
                    ModelState.AddModelError("", $"Error uploading contract: {ex.Message}");
                }
            }

            ViewBag.Contract = contract;
            return View();
        }

        // GET: Contract/Download
        // Downloads a file from Azure File Share
        public async Task<IActionResult> Download(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return BadRequest("File name is required"); // Validate input

            try
            {
                var shareClient = new ShareClient(_connectionString, _shareName);
                var directoryClient = shareClient.GetDirectoryClient(_contractsDirectory);
                var fileClient = directoryClient.GetFileClient(fileName);

                if (await fileClient.ExistsAsync())
                {
                    var response = await fileClient.DownloadAsync();
                    var contentType = "application/octet-stream"; // Default content type

                    // Set proper content type based on file extension
                    var extension = Path.GetExtension(fileName).ToLowerInvariant();
                    contentType = extension switch
                    {
                        ".pdf" => "application/pdf",
                        ".doc" => "application/msword",
                        ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                        ".txt" => "text/plain",
                        _ => "application/octet-stream"
                    };

                    return File(response.Value.Content, contentType, fileName);
                }
                else
                {
                    TempData["Error"] = "File not found.";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error downloading contract: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Contract/Delete
        // Deletes a file from Azure File Share
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return BadRequest("File name is required"); // Validate input

            try
            {
                var shareClient = new ShareClient(_connectionString, _shareName);
                var directoryClient = shareClient.GetDirectoryClient(_contractsDirectory);
                var fileClient = directoryClient.GetFileClient(fileName);

                await fileClient.DeleteIfExistsAsync(); // Delete file if exists
                TempData["Success"] = "Contract deleted successfully!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error deleting contract: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Contract/CreateFileShare (for testing)
        // Creates the file share and directory manually
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateFileShare()
        {
            try
            {
                var shareClient = new ShareClient(_connectionString, _shareName);
                await shareClient.CreateIfNotExistsAsync(); // Create share if it doesn't exist

                var directoryClient = shareClient.GetDirectoryClient(_contractsDirectory);
                await directoryClient.CreateIfNotExistsAsync(); // Create directory if it doesn't exist

                TempData["Success"] = "File share and directory created successfully!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error creating file share: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
