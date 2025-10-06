using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Azure.Storage.Queues;
using CloudDevPOE.Models;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;
using Azure.Storage.Files.Shares;

namespace CloudPart2Functions
{
    class StorageFunctions
    {
        private readonly ILogger _logger; // Logger for function execution tracking
        private readonly string _connectionString; // Azure Storage connection string

        public StorageFunctions(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<StorageFunctions>();
            _connectionString = Environment.GetEnvironmentVariable("AzureStorageConnection") ?? "";
        }

        // ========== FUNCTION 1a: Store Customer to Table ==========
        [Function("StoreCustomer")]
        public async Task<HttpResponseData> StoreCustomer(
            [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req)
        {
            _logger.LogInformation("StoreCustomer function triggered");

            try
            {
                // Read and deserialize the incoming request body as a CustomerProfile object
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var customerData = JsonSerializer.Deserialize<CustomerProfile>(requestBody);

                // Validate deserialized customer data
                if (customerData == null)
                {
                    var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                    await badResponse.WriteStringAsync("Invalid customer data");
                    return badResponse;
                }

                // Create or connect to Azure Table Storage
                var tableClient = new TableClient(_connectionString, "CustomerProfiles");
                await tableClient.CreateIfNotExistsAsync();

                // Set partition and row keys before inserting
                customerData.PartitionKey = "Customer";
                customerData.RowKey = Guid.NewGuid().ToString();

                // Add the customer entity to the table
                await tableClient.AddEntityAsync(customerData);

                // Return success response
                var response = req.CreateResponse(HttpStatusCode.OK);
                await response.WriteStringAsync($"Customer {customerData.FirstName} {customerData.LastName} stored successfully with ID: {customerData.RowKey}");
                return response;
            }
            catch (Exception ex)
            {
                // Handle and log exceptions
                _logger.LogError($"Error storing customer: {ex.Message}");
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteStringAsync($"Error: {ex.Message}");
                return errorResponse;
            }
        }

        // ========== FUNCTION 1b: Store Product to Table ==========
        [Function("StoreProduct")]
        public async Task<HttpResponseData> StoreProduct(
            [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req)
        {
            _logger.LogInformation("StoreProduct function triggered");

            try
            {
                // Read and deserialize incoming product data
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var productData = JsonSerializer.Deserialize<Product>(requestBody);

                // Validate product data
                if (productData == null)
                {
                    var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                    await badResponse.WriteStringAsync("Invalid product data");
                    return badResponse;
                }

                // Create or connect to Azure Table Storage for products
                var tableClient = new TableClient(_connectionString, "Products");
                await tableClient.CreateIfNotExistsAsync();

                // Set table entity keys and timestamp
                productData.PartitionKey = "Product";
                productData.RowKey = Guid.NewGuid().ToString();
                productData.CreatedDate = DateTime.UtcNow;

                // Insert product record
                await tableClient.AddEntityAsync(productData);

                // Return success response
                var response = req.CreateResponse(HttpStatusCode.OK);
                await response.WriteStringAsync($"Product {productData.ProductName} stored successfully with ID: {productData.RowKey}");
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error storing product: {ex.Message}");
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteStringAsync($"Error: {ex.Message}");
                return errorResponse;
            }
        }

        // ========== FUNCTION 2: Upload Image to Blob ==========
        [Function("UploadImage")]
        public async Task<HttpResponseData> UploadImage(
            [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req)
        {
            _logger.LogInformation("UploadImage function triggered");

            try
            {
                // Deserialize JSON containing base64 image data
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var uploadData = JsonSerializer.Deserialize<JsonElement>(requestBody);

                string fileName = uploadData.GetProperty("fileName").GetString() ?? "unnamed.png";
                string imageBase64 = uploadData.GetProperty("imageBase64").GetString() ?? "";
                byte[] imageBytes = Convert.FromBase64String(imageBase64);

                // Create blob service client and container
                var blobServiceClient = new BlobServiceClient(_connectionString);
                var containerClient = blobServiceClient.GetBlobContainerClient("multimedia");
                await containerClient.CreateIfNotExistsAsync(Azure.Storage.Blobs.Models.PublicAccessType.None);

                // Define blob name and upload stream
                var blobName = $"images/{Guid.NewGuid()}_{fileName}";
                var blobClient = containerClient.GetBlobClient(blobName);

                using var stream = new MemoryStream(imageBytes);
                await blobClient.UploadAsync(stream, overwrite: true);

                // Return success response with blob URL
                var response = req.CreateResponse(HttpStatusCode.OK);
                await response.WriteStringAsync($"Image uploaded successfully: {blobClient.Uri}");
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error uploading image: {ex.Message}");
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteStringAsync($"Error: {ex.Message}");
                return errorResponse;
            }
        }

        // ========== FUNCTION 3a: Send Transaction to Queue ==========
        [Function("SendTransaction")]
        public async Task<HttpResponseData> SendTransaction(
            [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req)
        {
            _logger.LogInformation("SendTransaction function triggered");

            try
            {
                // Read raw request body and send it as a queue message
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                var queueClient = new QueueClient(_connectionString, "transactions");
                await queueClient.CreateIfNotExistsAsync();
                await queueClient.SendMessageAsync(requestBody);

                var response = req.CreateResponse(HttpStatusCode.OK);
                await response.WriteStringAsync("Transaction sent to queue successfully");
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending transaction: {ex.Message}");
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteStringAsync($"Error: {ex.Message}");
                return errorResponse;
            }
        }

        // ========== FUNCTION 3b: Peek Transactions from Queue ==========
        [Function("PeekTransactions")]
        public async Task<HttpResponseData> PeekTransactions(
            [HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequestData req)
        {
            _logger.LogInformation("PeekTransactions function triggered");

            try
            {
                var queueClient = new QueueClient(_connectionString, "transactions");

                // Check if queue exists before reading
                if (!await queueClient.ExistsAsync())
                {
                    var notFoundResponse = req.CreateResponse(HttpStatusCode.OK);
                    await notFoundResponse.WriteStringAsync("Queue is empty or doesn't exist");
                    return notFoundResponse;
                }

                // Peek up to 10 messages from the queue
                var messages = await queueClient.PeekMessagesAsync(10);
                var messageList = messages.Value.Select(m => m.Body.ToString()).ToList();

                // Return JSON response with peeked messages
                var response = req.CreateResponse(HttpStatusCode.OK);
                response.Headers.Add("Content-Type", "application/json");
                await response.WriteStringAsync(JsonSerializer.Serialize(messageList));
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error peeking transactions: {ex.Message}");
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteStringAsync($"Error: {ex.Message}");
                return errorResponse;
            }
        }

        // ========== FUNCTION 4a: Upload Contract to Azure Files ==========
        [Function("UploadContract")]
        public async Task<HttpResponseData> UploadContract(
            [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req)
        {
            _logger.LogInformation("UploadContract function triggered");

            try
            {
                // Read contract data from request
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var contractData = JsonSerializer.Deserialize<JsonElement>(requestBody);

                string contractName = contractData.GetProperty("contractName").GetString() ?? "contract.txt";
                string contractContent = contractData.GetProperty("contractContent").GetString() ?? "";

                // Connect to Azure File Share
                var shareClient = new ShareClient(_connectionString, "contracts");
                await shareClient.CreateIfNotExistsAsync();

                var directoryClient = shareClient.GetDirectoryClient("customer-contracts");
                await directoryClient.CreateIfNotExistsAsync();

                // Upload contract content as a new text file
                var fileName = $"{contractName}.txt";
                var fileClient = directoryClient.GetFileClient(fileName);

                byte[] contentBytes = System.Text.Encoding.UTF8.GetBytes(contractContent);
                using var stream = new MemoryStream(contentBytes);

                await fileClient.CreateAsync(stream.Length);
                await fileClient.UploadAsync(stream);

                var response = req.CreateResponse(HttpStatusCode.OK);
                await response.WriteStringAsync($"Contract {fileName} uploaded successfully to Azure Files");
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error uploading contract: {ex.Message}");
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteStringAsync($"Error: {ex.Message}");
                return errorResponse;
            }
        }

        // ========== FUNCTION 4b: List Contracts from Azure Files ==========
        [Function("ListContracts")]
        public async Task<HttpResponseData> ListContracts(
            [HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequestData req)
        {
            _logger.LogInformation("ListContracts function triggered");

            try
            {
                // Connect to Azure File Share
                var shareClient = new ShareClient(_connectionString, "contracts");

                // Return message if share does not exist
                if (!await shareClient.ExistsAsync())
                {
                    var notFoundResponse = req.CreateResponse(HttpStatusCode.OK);
                    await notFoundResponse.WriteStringAsync("No contracts found");
                    return notFoundResponse;
                }

                // Get the directory and list all files within it
                var directoryClient = shareClient.GetDirectoryClient("customer-contracts");
                var files = new List<string>();

                await foreach (var item in directoryClient.GetFilesAndDirectoriesAsync())
                {
                    if (!item.IsDirectory)
                    {
                        files.Add(item.Name);
                    }
                }

                // Return list of contract filenames in JSON format
                var response = req.CreateResponse(HttpStatusCode.OK);
                response.Headers.Add("Content-Type", "application/json");
                await response.WriteStringAsync(JsonSerializer.Serialize(files));
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error listing contracts: {ex.Message}");
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteStringAsync($"Error: {ex.Message}");
                return errorResponse;
            }
        }
    }
}

