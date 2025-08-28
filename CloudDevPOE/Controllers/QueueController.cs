using Microsoft.AspNetCore.Mvc;
using CloudDevPOE.Models;
using CloudDevPOE.Services;

namespace CloudDevPOE.Controllers
{
    public class QueueController : Controller
    {
        private readonly AzureQueueService _queueService;

        public QueueController(AzureQueueService queueService)
        {
            _queueService = queueService;
        }

        // GET: Queue
        public async Task<IActionResult> Index()
        {
            var orderMessages = await _queueService.PeekOrderMessagesAsync(10);

            ViewBag.OrderMessages = orderMessages;
            ViewBag.NewOrderMessage = new OrderMessage { OrderId = Guid.NewGuid().ToString() };
            ViewBag.NewInventoryMessage = new InventoryMessage();
            ViewBag.NewImageMessage = new ImageProcessingMessage();

            return View();
        }

        // POST: Queue/SendOrderMessage
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendOrderMessage(OrderMessage orderMessage)
        {
            try
            {
                await _queueService.SendOrderMessageAsync(orderMessage);
                TempData["Success"] = "Order message sent successfully!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error sending order message: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Queue/SendInventoryMessage
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendInventoryMessage(InventoryMessage inventoryMessage)
        {
            try
            {
                await _queueService.SendInventoryMessageAsync(inventoryMessage);
                TempData["Success"] = "Inventory message sent successfully!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error sending inventory message: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Queue/SendImageMessage
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendImageMessage(ImageProcessingMessage imageMessage)
        {
            try
            {
                await _queueService.SendImageProcessingMessageAsync(imageMessage);
                TempData["Success"] = "Image processing message sent successfully!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error sending image message: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Queue/ProcessOrderMessage
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessOrderMessage()
        {
            try
            {
                var message = await _queueService.ReceiveOrderMessageAsync();
                if (message != null)
                {
                    TempData["Success"] = $"Processed order message: {message.OrderId}";
                }
                else
                {
                    TempData["Info"] = "No order messages to process.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error processing order message: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Queue/ProcessInventoryMessage
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessInventoryMessage()
        {
            try
            {
                var message = await _queueService.ReceiveInventoryMessageAsync();
                if (message != null)
                {
                    TempData["Success"] = $"Processed inventory message for product: {message.ProductId}";
                }
                else
                {
                    TempData["Info"] = "No inventory messages to process.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error processing inventory message: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Queue/ProcessImageMessage
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessImageMessage()
        {
            try
            {
                var message = await _queueService.ReceiveImageProcessingMessageAsync();
                if (message != null)
                {
                    TempData["Success"] = $"Processed image message: {message.ImageName}";
                }
                else
                {
                    TempData["Info"] = "No image processing messages to process.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error processing image message: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
