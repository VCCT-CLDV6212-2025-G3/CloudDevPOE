using Microsoft.AspNetCore.Mvc;
using CloudDevPOE.Models;
using CloudDevPOE.Services;

namespace CloudDevPOE.Controllers
{
    public class CustomerController : Controller
    {
        // Service for Azure Table Storage operations related to customers
        private readonly AzureTableService _tableService;

        // Constructor injects the AzureTableService
        public CustomerController(AzureTableService tableService)
        {
            _tableService = tableService;
        }

        // GET: Customer
        // Displays a list of all customers
        public async Task<IActionResult> Index()
        {
            var customers = await _tableService.GetAllCustomersAsync(); // Get all customer profiles
            return View(customers); // Pass the list to the view
        }

        // GET: Customer/Details/5
        // Displays details of a single customer
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound(); // Validate input

            try
            {
                var customer = await _tableService.GetCustomerAsync(id); // Retrieve customer by ID
                return View(customer);
            }
            catch
            {
                return NotFound(); // Return 404 if not found or error occurs
            }
        }

        // GET: Customer/Create
        // Returns the customer creation view
        public IActionResult Create()
        {
            return View(new CustomerProfile());
        }

        // POST: Customer/Create
        // Creates a new customer profile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CustomerProfile customer)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _tableService.CreateCustomerAsync(customer); // Add customer to Table Storage
                    TempData["Success"] = "Customer created successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error creating customer: {ex.Message}"); // Display errors
                }
            }
            return View(customer);
        }

        // GET: Customer/Edit/5
        // Returns the edit view for a customer
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            try
            {
                var customer = await _tableService.GetCustomerAsync(id); // Retrieve customer by ID
                return View(customer);
            }
            catch
            {
                return NotFound();
            }
        }

        // POST: Customer/Edit/5
        // Updates an existing customer profile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, CustomerProfile customer)
        {
            if (id != customer.RowKey)
                return NotFound(); // Ensure IDs match

            if (ModelState.IsValid)
            {
                try
                {
                    await _tableService.UpdateCustomerAsync(customer); // Update customer in Table Storage
                    TempData["Success"] = "Customer updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error updating customer: {ex.Message}"); // Display errors
                }
            }
            return View(customer);
        }

        // GET: Customer/Delete/5
        // Returns the delete confirmation view for a customer
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            try
            {
                var customer = await _tableService.GetCustomerAsync(id); // Retrieve customer by ID
                return View(customer);
            }
            catch
            {
                return NotFound();
            }
        }

        // POST: Customer/Delete/5
        // Deletes a customer profile
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            try
            {
                await _tableService.DeleteCustomerAsync(id); // Delete customer from Table Storage
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
