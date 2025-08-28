using Microsoft.AspNetCore.Mvc;
using CloudDevPOE.Models;
using CloudDevPOE.Services;

namespace CloudDevPOE.Controllers
{
    public class ProductController : Controller
    {
        private readonly AzureTableService _tableService;
        private readonly AzureBlobService _blobService;

        public ProductController(AzureTableService tableService, AzureBlobService blobService)
        {
            _tableService = tableService;
            _blobService = blobService;
        }

        // GET: Product
        public async Task<IActionResult> Index(string category = "")
        {
            List<Product> product;

            if (string.IsNullOrEmpty(category))
            {
                product = await _tableService.GetAllProductsAsync();
            }
            else
            {
                product = await _tableService.GetProductsByCategoryAsync(category);
            }

            ViewBag.SelectedCategory = category;
            ViewBag.Categories = (await _tableService.GetAllProductsAsync())
                .Select(p => p.Category).Distinct().ToList();

            return View(product);
        }

        // GET: Product/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            try
            {
                var product = await _tableService.GetProductAsync(id);
                return View(product);
            }
            catch
            {
                return NotFound();
            }
        }

        // GET: Product/Create
        public IActionResult Create()
        {
            return View(new Product());
        }

        // POST: Product/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product, IFormFile? imageFile)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Upload image if provided
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        using var stream = imageFile.OpenReadStream();
                        product.ImageUrl = await _blobService.UploadImageAsync(stream, imageFile.FileName, imageFile.ContentType);
                    }

                    await _tableService.CreateProductAsync(product);
                    TempData["Success"] = "Product created successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error creating product: {ex.Message}");
                }
            }
            return View(product);
        }

        // GET: Product/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            try
            {
                var product = await _tableService.GetProductAsync(id);
                return View(product);
            }
            catch
            {
                return NotFound();
            }
        }

        // POST: Product/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, Product product, IFormFile? imageFile)
        {
            if (id != product.RowKey)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    // Upload new image if provided
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        using var stream = imageFile.OpenReadStream();
                        product.ImageUrl = await _blobService.UploadImageAsync(stream, imageFile.FileName, imageFile.ContentType);
                    }

                    await _tableService.UpdateProductAsync(product);
                    TempData["Success"] = "Product updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error updating product: {ex.Message}");
                }
            }
            return View(product);
        }

        // GET: Product/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            try
            {
                var product = await _tableService.GetProductAsync(id);
                return View(product);
            }
            catch
            {
                return NotFound();
            }
        }

        // POST: Product/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            try
            {
                await _tableService.DeleteProductAsync(id);
                TempData["Success"] = "Product deleted successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error deleting product: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
