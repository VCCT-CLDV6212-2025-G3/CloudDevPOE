using Microsoft.AspNetCore.Mvc;
using CloudDevPOE.Models;
using CloudDevPOE.Services;

namespace CloudDevPOE.Controllers
{
    public class CustomerController : Controller
    {
        private readonly AzureTableService _tableService;

        public CustomerController(AzureTableService tableService)
        {
            _tableService = tableService;
        }

        // GET: Customer
        public async Task<IActionResult> Index()
        {
            var customers = await _tableService.GetAllCustomersAsync();
            return View(customers);
        }

        // GET: Customer/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            try
            {
                var customer = await _tableService.GetCustomerAsync(id);
                return View(customer);
            }
            catch
            {
                return NotFound();
            }
        }

        // GET: Customer/Create
        public IActionResult Create()
        {
            return View(new CustomerProfile());
        }

        // POST: Customer/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CustomerProfile customer)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _tableService.CreateCustomerAsync(customer);
                    TempData["Success"] = "Customer created successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error creating customer: {ex.Message}");
                }
            }
            return View(customer);
        }

        // GET: Customer/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            try
            {
                var customer = await _tableService.GetCustomerAsync(id);
                return View(customer);
            }
            catch
            {
                return NotFound();
            }
        }

        // POST: Customer/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, CustomerProfile customer)
        {
            if (id != customer.RowKey)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    await _tableService.UpdateCustomerAsync(customer);
                    TempData["Success"] = "Customer updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error updating customer: {ex.Message}");
                }
            }
            return View(customer);
        }

        // GET: Customer/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            try
            {
                var customer = await _tableService.GetCustomerAsync(id);
                return View(customer);
            }
            catch
            {
                return NotFound();
            }
        }

        // POST: Customer/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            try
            {
                await _tableService.DeleteCustomerAsync(id);
                TempData["Success"] = "Customer deleted successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error deleting customer: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
