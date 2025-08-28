using Microsoft.AspNetCore.Mvc;
using CloudDevPOE.Services;
using CloudDevPOE.Models;

namespace CloudDevPOE.Controllers
{
    public class ContractController : Controller
    {
        private readonly AzureFileService _fileService;

        public ContractController(AzureFileService fileService)
        {
            _fileService = fileService;
        }

        // GET: Contract
        public async Task<IActionResult> Index()
        {
            var contractFiles = await _fileService.GetAllContractFilesAsync();
            return View(contractFiles);
        }

        // GET: Contract/Upload
        public IActionResult Upload()
        {
            var contract = new Contract();
            contract.ContractId = Guid.NewGuid().ToString("N")[..8].ToUpper();
            ViewBag.Contract = contract;
            return View();
        }

        // POST: Contract/Upload
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(Contract contract, IFormFile contractFile)
        {
            if (ModelState.IsValid && contractFile != null && contractFile.Length > 0)
            {
                try
                {
                    using var stream = contractFile.OpenReadStream();
                    var fileName = $"{contract.ContractId}_{contractFile.FileName}";

                    var filePath = await _fileService.UploadContractAsync(stream, fileName, contract);
                    contract.FilePath = filePath;

                    TempData["Success"] = $"Contract uploaded successfully! Path: {filePath}";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error uploading contract: {ex.Message}");
                }
            }

            if (contractFile == null || contractFile.Length == 0)
            {
                ModelState.AddModelError("contractFile", "Please select a file to upload.");
            }

            ViewBag.Contract = contract;
            return View();
        }

        // GET: Contract/Download
        public async Task<IActionResult> Download(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return BadRequest();

            try
            {
                var stream = await _fileService.DownloadContractAsync(fileName);
                return File(stream, "application/octet-stream", fileName);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error downloading contract: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Contract/Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return BadRequest();

            try
            {
                await _fileService.DeleteContractAsync(fileName);
                TempData["Success"] = "Contract deleted successfully!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error deleting contract: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
