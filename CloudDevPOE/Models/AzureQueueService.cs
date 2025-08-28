using Azure.Storage.Queues;
using CloudDevPOE.Models;
using System.Text.Json;

namespace CloudDevPOE.Services
{
    public class AzureQueueService
    {
        // Client for interacting with Azure Queues
        private readonly QueueServiceClient _queueServiceClient;

        // Queue names
        private readonly string _orderQueueName = "order-processing";
        private readonly string _inventoryQueueName = "inventory-management";
        private readonly string _imageQueueName = "image-processing";

        // Constructor initializes QueueServiceClient using connection string
        public AzureQueueService(string connectionString)
        {
            _queueServiceClient = new QueueServiceClient(connectionString);
        }

        // ===================== Order Processing Queue Operations =====================

        // Send a message to the order-processing queue
        public async Task SendOrderMessageAsync(OrderMessage orderMessage)
        {
            var queueClient = _queueServiceClient.GetQueueClient(_orderQueueName);
            await queueClient.CreateIfNotExistsAsync(); // Create queue if it doesn't exist

            var messageJson = JsonSerializer.Serialize(orderMessage); // Serialize object to JSON
            await queueClient.SendMessageAsync(messageJson); // Send message
        }

        // Receive and remove a message from the order-processing queue
        public async Task<OrderMessage?> ReceiveOrderMessageAsync()
        {
            var queueClient = _queueServiceClient.GetQueueClient(_orderQueueName);
            var response = await queueClient.ReceiveMessageAsync();

            if (response.Value != null)
            {
                // Deserialize message to OrderMessage object
                var orderMessage = JsonSerializer.Deserialize<OrderMessage>(response.Value.Body.ToString());
                // Delete message after processing
                await queueClient.DeleteMessageAsync(response.Value.MessageId, response.Value.PopReceipt);
                return orderMessage;
            }

            return null; // Return null if no messages
        }

        // Peek messages in the order-processing queue without deleting them
        public async Task<List<OrderMessage>> PeekOrderMessagesAsync(int maxMessages = 10)
        {
            var queueClient = _queueServiceClient.GetQueueClient(_orderQueueName);
            var messages = new List<OrderMessage>();

            var response = await queueClient.PeekMessagesAsync(maxMessages);

            foreach (var message in response.Value)
            {
                var orderMessage = JsonSerializer.Deserialize<OrderMessage>(message.Body.ToString());
                if (orderMessage != null)
                {
                    messages.Add(orderMessage);
                }
            }

            return messages; // Return list of peeked messages
        }

        // ===================== Inventory Management Queue Operations =====================

        // Send a message to the inventory-management queue
        public async Task SendInventoryMessageAsync(InventoryMessage inventoryMessage)
        {
            var queueClient = _queueServiceClient.GetQueueClient(_inventoryQueueName);
            await queueClient.CreateIfNotExistsAsync();

            var messageJson = JsonSerializer.Serialize(inventoryMessage);
            await queueClient.SendMessageAsync(messageJson);
        }

        // Receive and remove a message from the inventory-management queue
        public async Task<InventoryMessage?> ReceiveInventoryMessageAsync()
        {
            var queueClient = _queueServiceClient.GetQueueClient(_inventoryQueueName);
            var response = await queueClient.ReceiveMessageAsync();

            if (response.Value != null)
            {
                var inventoryMessage = JsonSerializer.Deserialize<InventoryMessage>(response.Value.Body.ToString());
                await queueClient.DeleteMessageAsync(response.Value.MessageId, response.Value.PopReceipt);
                return inventoryMessage;
            }

            return null;
        }

        // ===================== Image Processing Queue Operations =====================

        // Send a message to the image-processing queue
        public async Task SendImageProcessingMessageAsync(ImageProcessingMessage imageMessage)
        {
            var queueClient = _queueServiceClient.GetQueueClient(_imageQueueName);
            await queueClient.CreateIfNotExistsAsync();

            var messageJson = JsonSerializer.Serialize(imageMessage);
            await queueClient.SendMessageAsync(messageJson);
        }

        // Receive and remove a message from the image-processing queue
        public async Task<ImageProcessingMessage?> ReceiveImageProcessingMessageAsync()
        {
            var queueClient = _queueServiceClient.GetQueueClient(_imageQueueName);
            var response = await queueClient.ReceiveMessageAsync();

            if (response.Value != null)
            {
                var imageMessage = JsonSerializer.Deserialize<ImageProcessingMessage>(response.Value.Body.ToString());
                await queueClient.DeleteMessageAsync(response.Value.MessageId, response.Value.PopReceipt);
                return imageMessage;
            }

            return null;
        }

        // ===================== General Queue Operations =====================

        // Clears all messages from a specified queue
        public async Task ClearQueueAsync(string queueName)
        {
            var queueClient = _queueServiceClient.GetQueueClient(queueName);
            await queueClient.ClearMessagesAsync();
        }
    }
}
