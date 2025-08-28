using Microsoft.AspNetCore.Mvc;
using CloudDevPOE.Services;

namespace CloudDevPOE.Controllers
{
    public class BlobController : Controller
    {
        private readonly AzureBlobService _blobService;

        public BlobController(AzureBlobService blobService)
        {
            _blobService = blobService;
        }

        // GET: Blob
        public async Task<IActionResult> Index()
        {
            var allUrls = await _blobService.GetAllBlobUrlsAsync();

            ViewBag.AllBlobUrls = allUrls;
            ViewBag.ImageUrls = allUrls.Where(url => url.Contains("/images/")).ToList();
            ViewBag.VideoUrls = allUrls.Where(url => url.Contains("/videos/")).ToList();
            ViewBag.DocumentUrls = allUrls.Where(url => url.Contains("/documents/")).ToList();

            return View();
        }

        // GET: Blob/Upload
        public IActionResult Upload()
        {
            return View();
        }

        // POST: Blob/Upload
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
                    TempData["Error"] = $"Error uploading file: {ex.Message}";
                }
            }
            else
            {
                TempData["Error"] = "Please select a file to upload.";
            }

            return View();
        }

        // GET: Blob/Download
        public async Task<IActionResult> Download(string url)
        {
            if (string.IsNullOrEmpty(url))
                return BadRequest();

            try
            {
                var stream = await _blobService.DownloadBlobAsync(url);
                var uri = new Uri(url);
                var fileName = uri.Segments[^1]; // Get the last segment (filename)

                return File(stream, "application/octet-stream", fileName);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error downloading file: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Blob/Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string url)
        {
            if (string.IsNullOrEmpty(url))
                return BadRequest();

            try
            {
                await _blobService.DeleteBlobAsync(url);
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