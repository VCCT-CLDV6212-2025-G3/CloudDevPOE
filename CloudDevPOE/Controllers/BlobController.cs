using Microsoft.AspNetCore.Mvc;
using CloudDevPOE.Services;

namespace CloudDevPOE.Controllers
{
    public class BlobController : Controller
    {
        // Service to handle Azure Blob Storage operations
        private readonly AzureBlobService _blobService;

        // Constructor injects the blob service
        public BlobController(AzureBlobService blobService)
        {
            _blobService = blobService;
        }

        // GET: Blob
        // Displays all blob URLs categorized by type (images, videos, documents)
        public async Task<IActionResult> Index()
        {
            var allUrls = await _blobService.GetAllBlobUrlsAsync(); // Get all blob URLs

            // Pass all URLs to the view
            ViewBag.AllBlobUrls = allUrls;
            ViewBag.ImageUrls = allUrls.Where(url => url.Contains("/images/")).ToList();
            ViewBag.VideoUrls = allUrls.Where(url => url.Contains("/videos/")).ToList();
            ViewBag.DocumentUrls = allUrls.Where(url => url.Contains("/documents/")).ToList();

            return View();
        }

        // GET: Blob/Upload
        // Returns the upload view
        public IActionResult Upload()
        {
            return View();
        }

        // POST: Blob/Upload
        // Uploads a file to Azure Blob Storage based on type (image, video, document)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(IFormFile file, string uploadType = "document")
        {
            if (file != null && file.Length > 0)
            {
                try
                {
                    using var stream = file.OpenReadStream();
                    string uploadedUrl = "";

                    // Determine which type of blob to upload
                    switch (uploadType.ToLower())
                    {
                        case "image":
                            uploadedUrl = await _blobService.UploadImageAsync(stream, file.FileName, file.ContentType);
                            break;
                        case "video":
                            uploadedUrl = await _blobService.UploadVideoAsync(stream, file.FileName, file.ContentType);
                            break;
                        case "document":
                        default:
                            uploadedUrl = await _blobService.UploadDocumentAsync(stream, file.FileName, file.ContentType);
                            break;
                    }

                    TempData["Success"] = $"File uploaded successfully! URL: {uploadedUrl}";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    // Handle errors during upload
                    TempData["Error"] = $"Error uploading file: {ex.Message}";
                }
            }
            else
            {
                // No file selected
                TempData["Error"] = "Please select a file to upload.";
            }

            return View();
        }

        // GET: Blob/Download
        // Downloads a blob file from Azure Blob Storage
        public async Task<IActionResult> Download(string url)
        {
            if (string.IsNullOrEmpty(url))
                return BadRequest(); // Validate input

            try
            {
                var stream = await _blobService.DownloadBlobAsync(url); // Download blob stream
                var uri = new Uri(url);
                var fileName = uri.Segments[^1]; // Extract filename from URL

                return File(stream, "application/octet-stream", fileName); // Return file for download
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error downloading file: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Blob/Delete
        // Deletes a blob from Azure Blob Storage
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string url)
        {
            if (string.IsNullOrEmpty(url))
                return BadRequest(); // Validate input

            try
            {
                await _blobService.DeleteBlobAsync(url); // Delete the blob
                TempData["Success"] = "File deleted successfully!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error deleting file: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
