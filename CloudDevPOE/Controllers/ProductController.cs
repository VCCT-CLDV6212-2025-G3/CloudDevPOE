using Microsoft.AspNetCore.Mvc;
using CloudDevPOE.Models;
using CloudDevPOE.Services;

namespace CloudDevPOE.Controllers
{
    public class ProductController : Controller
    {
        // Services for Azure Table Storage and Blob Storage
        private readonly AzureTableService _tableService;
        private readonly AzureBlobService _blobService;

        // Constructor injects both services
        public ProductController(AzureTableService tableService, AzureBlobService blobService)
        {
            _tableService = tableService;
            _blobService = blobService;
        }

        // GET: Product
        // Displays all products, optionally filtered by category
        public async Task<IActionResult> Index(string category = "")
        {
            try
            {
                List<Product> products; // List to hold products

                // Load products based on category filter
                if (string.IsNullOrEmpty(category))
                {
                    products = await _tableService.GetAllProductsAsync();
                }
                else
                {
                    products = await _tableService.GetProductsByCategoryAsync(category);
                }

                // Pass selected category and all categories to the view
                ViewBag.SelectedCategory = category;
                ViewBag.Categories = products.Select(p => p.Category).Distinct().ToList();

                return View(products); // Pass products to view
            }
            catch (Exception ex)
            {
                // Display error and return empty list
                ViewBag.ErrorMessage = $"Error loading products: {ex.Message}";
                return View(new List<Product>());
            }
        }

        // GET: Product/Details/5
        // Displays details of a single product
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound(); // Validate input

            try
            {
                var product = await _tableService.GetProductAsync(id);
                return View(product);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Product not found: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Product/Create
        // Returns the product creation view
        public IActionResult Create()
        {
            return View(new Product());
        }

        // POST: Product/Create
        // Creates a new product and uploads image if provided
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product, IFormFile? imageFile)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Upload image to Blob Storage if file is provided
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        using var stream = imageFile.OpenReadStream();
                        product.ImageUrl = await _blobService.UploadImageAsync(stream, imageFile.FileName, imageFile.ContentType);
                    }

                    await _tableService.CreateProductAsync(product); // Create product in Table Storage
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
        // Returns the product edit view
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            try
            {
                var product = await _tableService.GetProductAsync(id);
                return View(product);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Product not found: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Product/Edit/5
        // Updates an existing product and optionally updates its image
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, Product product, IFormFile? imageFile)
        {
            if (id != product.RowKey)
                return NotFound(); // Ensure product IDs match

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

                    await _tableService.UpdateProductAsync(product); // Update product in Table Storage
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
        // Returns the product delete confirmation view
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            try
            {
                var product = await _tableService.GetProductAsync(id);
                return View(product);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Product not found: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Product/Delete/5
        // Deletes a product from Table Storage
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            try
            {
                await _tableService.DeleteProductAsync(id); // Delete product
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
