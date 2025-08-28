using Microsoft.AspNetCore.Mvc;
using CloudDevPOE.Models;
using Azure.Storage.Queues;
using System.Text.Json;

namespace CloudDevPOE.Controllers
{
    public class QueueController : Controller
    {
        // Azure Storage connection string
        private readonly string _connectionString;

        // Constructor injects IConfiguration to get the Azure Storage connection string
        public QueueController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("AzureStorage") ?? "";
        }

        // GET: Queue
        // Displays messages from queues and prepares new message objects for the view
        public async Task<IActionResult> Index()
        {
            try
            {
                // Connect to the "order-processing" queue
                var orderQueueClient = new QueueClient(_connectionString, "order-processing");
                var orderMessages = new List<OrderMessage>();

                if (await orderQueueClient.ExistsAsync())
                {
                    // Peek at up to 10 messages without dequeuing
                    var peekedMessages = await orderQueueClient.PeekMessagesAsync(10);
                    foreach (var message in peekedMessages.Value)
                    {
                        try
                        {
                            // Deserialize the message body to an OrderMessage object
                            var orderMessage = JsonSerializer.Deserialize<OrderMessage>(message.Body.ToString());
                            if (orderMessage != null)
                                orderMessages.Add(orderMessage);
                        }
                        catch
                        {
                            // Skip invalid messages that cannot be deserialized
                        }
                    }
                }

                // Pass messages and new empty message objects to the view
                ViewBag.OrderMessages = orderMessages;
                ViewBag.NewOrderMessage = new OrderMessage { OrderId = Guid.NewGuid().ToString() };
                ViewBag.NewInventoryMessage = new InventoryMessage();
                ViewBag.NewImageMessage = new ImageProcessingMessage();

                return View();
            }
            catch (Exception ex)
            {
                // Handle errors and provide empty/default messages
                ViewBag.ErrorMessage = $"Error loading queue data: {ex.Message}";
                ViewBag.OrderMessages = new List<OrderMessage>();
                ViewBag.NewOrderMessage = new OrderMessage { OrderId = Guid.NewGuid().ToString() };
                ViewBag.NewInventoryMessage = new InventoryMessage();
                ViewBag.NewImageMessage = new ImageProcessingMessage();

                return View();
            }
        }

        // POST: Queue/SendOrderMessage
        // Sends a new order message to the "order-processing" queue
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendOrderMessage(OrderMessage orderMessage)
        {
            try
            {
                var queueClient = new QueueClient(_connectionString, "order-processing");
                await queueClient.CreateIfNotExistsAsync(); // Create queue if it doesn't exist

                var messageJson = JsonSerializer.Serialize(orderMessage); // Convert object to JSON
                await queueClient.SendMessageAsync(messageJson); // Send message to queue

                TempData["Success"] = "Order message sent successfully!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error sending order message: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Queue/SendInventoryMessage
        // Sends a new inventory message to the "inventory-management" queue
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendInventoryMessage(InventoryMessage inventoryMessage)
        {
            try
            {
                var queueClient = new QueueClient(_connectionString, "inventory-management");
                await queueClient.CreateIfNotExistsAsync(); // Create queue if needed

                var messageJson = JsonSerializer.Serialize(inventoryMessage); // Convert object to JSON
                await queueClient.SendMessageAsync(messageJson); // Send message

                TempData["Success"] = "Inventory message sent successfully!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error sending inventory message: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Queue/SendImageMessage
        // Sends a new image processing message to the "image-processing" queue
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendImageMessage(ImageProcessingMessage imageMessage)
        {
            try
            {
                var queueClient = new QueueClient(_connectionString, "image-processing");
                await queueClient.CreateIfNotExistsAsync(); // Ensure queue exists

                var messageJson = JsonSerializer.Serialize(imageMessage); // Serialize message
                await queueClient.SendMessageAsync(messageJson); // Send message

                TempData["Success"] = "Image processing message sent successfully!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error sending image message: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Queue/ProcessOrderMessage
        // Retrieves and processes a single order message from the queue
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessOrderMessage()
        {
            try
            {
                var queueClient = new QueueClient(_connectionString, "order-processing");
                var response = await queueClient.ReceiveMessageAsync(); // Receive one message

                if (response.Value != null)
                {
                    var message = response.Value;
                    var orderMessage = JsonSerializer.Deserialize<OrderMessage>(message.Body.ToString());

                    // Delete the message after processing
                    await queueClient.DeleteMessageAsync(message.MessageId, message.PopReceipt);

                    TempData["Success"] = $"Processed order message: {orderMessage?.OrderId ?? "Unknown"}";
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
        // Retrieves and processes a single inventory message
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessInventoryMessage()
        {
            try
            {
                var queueClient = new QueueClient(_connectionString, "inventory-management");
                var response = await queueClient.ReceiveMessageAsync(); // Receive one message

                if (response.Value != null)
                {
                    var message = response.Value;
                    var inventoryMessage = JsonSerializer.Deserialize<InventoryMessage>(message.Body.ToString());

                    // Delete after processing
                    await queueClient.DeleteMessageAsync(message.MessageId, message.PopReceipt);

                    TempData["Success"] = $"Processed inventory message for product: {inventoryMessage?.ProductId ?? "Unknown"}";
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
        // Retrieves and processes a single image processing message
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessImageMessage()
        {
            try
            {
                var queueClient = new QueueClient(_connectionString, "image-processing");
                var response = await queueClient.ReceiveMessageAsync(); // Receive one message

                if (response.Value != null)
                {
                    var message = response.Value;
                    var imageMessage = JsonSerializer.Deserialize<ImageProcessingMessage>(message.Body.ToString());

                    // Delete the message after processing
                    await queueClient.DeleteMessageAsync(message.MessageId, message.PopReceipt);

                    TempData["Success"] = $"Processed image message: {imageMessage?.ImageName ?? "Unknown"}";
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
