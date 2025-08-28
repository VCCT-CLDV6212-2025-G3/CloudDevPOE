using Azure.Data.Tables;
using CloudDevPOE.Models;

namespace CloudDevPOE.Services
{
    public class AzureTableService
    {
        private readonly TableServiceClient _tableServiceClient;

        // Table names
        private readonly string _customerTableName = "CustomerProfiles";
        private readonly string _productTableName = "Products";

        // Constructor initializes the TableServiceClient using the connection string
        public AzureTableService(string connectionString)
        {
            _tableServiceClient = new TableServiceClient(connectionString);
        }

        // ===================== CUSTOMER OPERATIONS =====================

        // Create a new customer in the table
        public async Task<CustomerProfile> CreateCustomerAsync(CustomerProfile customer)
        {
            var tableClient = _tableServiceClient.GetTableClient(_customerTableName);
            await tableClient.CreateIfNotExistsAsync();

            // Assign a unique RowKey for the customer
            customer.RowKey = Guid.NewGuid().ToString();

            var response = await tableClient.AddEntityAsync(customer);
            return customer;
        }

        // Retrieve a single customer by ID
        public async Task<CustomerProfile> GetCustomerAsync(string customerId)
        {
            var tableClient = _tableServiceClient.GetTableClient(_customerTableName);
            var response = await tableClient.GetEntityAsync<CustomerProfile>("Customer", customerId);
            return response.Value;
        }

        // Get all customers from the table
        public async Task<List<CustomerProfile>> GetAllCustomersAsync()
        {
            var tableClient = _tableServiceClient.GetTableClient(_customerTableName);
            var customers = new List<CustomerProfile>();

            await foreach (var customer in tableClient.QueryAsync<CustomerProfile>())
            {
                customers.Add(customer);
            }

            return customers;
        }

        // Update an existing customer
        public async Task<CustomerProfile> UpdateCustomerAsync(CustomerProfile customer)
        {
            var tableClient = _tableServiceClient.GetTableClient(_customerTableName);
            var response = await tableClient.UpdateEntityAsync(customer, customer.ETag);
            return customer;
        }

        // Delete a customer by ID
        public async Task DeleteCustomerAsync(string customerId)
        {
            var tableClient = _tableServiceClient.GetTableClient(_customerTableName);
            await tableClient.DeleteEntityAsync("Customer", customerId);
        }

        // ===================== PRODUCT OPERATIONS =====================

        // Create a new product in the table
        public async Task<Product> CreateProductAsync(Product product)
        {
            var tableClient = _tableServiceClient.GetTableClient(_productTableName);
            await tableClient.CreateIfNotExistsAsync();

            product.RowKey = Guid.NewGuid().ToString();
            var response = await tableClient.AddEntityAsync(product);
            return product;
        }

        // Get a product by ID, handling possible type conversion issues
        public async Task<Product> GetProductAsync(string productId)
        {
            var tableClient = _tableServiceClient.GetTableClient(_productTableName);

            try
            {
                // Attempt to get as strongly typed Product
                var response = await tableClient.GetEntityAsync<Product>("Product", productId);
                return response.Value;
            }
            catch (InvalidCastException)
            {
                // If cast fails, get as TableEntity and convert manually
                var entityResponse = await tableClient.GetEntityAsync<TableEntity>("Product", productId);
                return ConvertTableEntityToProduct(entityResponse.Value);
            }
        }

        // Retrieve all products
        public async Task<List<Product>> GetAllProductsAsync()
        {
            var tableClient = _tableServiceClient.GetTableClient(_productTableName);
            var products = new List<Product>();

            try
            {
                await foreach (var product in tableClient.QueryAsync<Product>())
                {
                    products.Add(product);
                }
            }
            catch (InvalidCastException)
            {
                // If direct cast fails, convert from TableEntity
                await foreach (var entity in tableClient.QueryAsync<TableEntity>())
                {
                    products.Add(ConvertTableEntityToProduct(entity));
                }
            }

            return products;
        }

        // Retrieve products by category
        public async Task<List<Product>> GetProductsByCategoryAsync(string category)
        {
            var tableClient = _tableServiceClient.GetTableClient(_productTableName);
            var products = new List<Product>();

            try
            {
                await foreach (var product in tableClient.QueryAsync<Product>(p => p.Category == category))
                {
                    products.Add(product);
                }
            }
            catch (InvalidCastException)
            {
                // If direct cast fails, manually filter TableEntity results
                await foreach (var entity in tableClient.QueryAsync<TableEntity>())
                {
                    var product = ConvertTableEntityToProduct(entity);
                    if (product.Category == category)
                    {
                        products.Add(product);
                    }
                }
            }

            return products;
        }

        // Update an existing product
        public async Task<Product> UpdateProductAsync(Product product)
        {
            var tableClient = _tableServiceClient.GetTableClient(_productTableName);
            var response = await tableClient.UpdateEntityAsync(product, product.ETag);
            return product;
        }

        // Delete a product by ID
        public async Task DeleteProductAsync(string productId)
        {
            var tableClient = _tableServiceClient.GetTableClient(_productTableName);
            await tableClient.DeleteEntityAsync("Product", productId);
        }

        // ===================== HELPER METHODS =====================

        // Converts a TableEntity to a strongly-typed Product with correct type handling
        private Product ConvertTableEntityToProduct(TableEntity entity)
        {
            var product = new Product
            {
                PartitionKey = entity.PartitionKey,
                RowKey = entity.RowKey,
                Timestamp = entity.Timestamp,
                ETag = entity.ETag
            };

            // Convert fields safely with type checks
            if (entity.ContainsKey("ProductName"))
                product.ProductName = entity["ProductName"]?.ToString() ?? string.Empty;

            if (entity.ContainsKey("Description"))
                product.Description = entity["Description"]?.ToString() ?? string.Empty;

            if (entity.ContainsKey("Price"))
            {
                var priceValue = entity["Price"];
                if (priceValue is string priceStr && double.TryParse(priceStr, out double priceFromString))
                    product.Price = priceFromString;
                else if (priceValue is double priceDouble)
                    product.Price = priceDouble;
                else if (priceValue is decimal priceDecimal)
                    product.Price = (double)priceDecimal;
            }

            if (entity.ContainsKey("StockQuantity"))
            {
                var stockValue = entity["StockQuantity"];
                if (stockValue is string stockStr && int.TryParse(stockStr, out int stockFromString))
                    product.StockQuantity = stockFromString;
                else if (stockValue is int stockInt)
                    product.StockQuantity = stockInt;
                else if (stockValue is long stockLong)
                    product.StockQuantity = (int)stockLong;
            }

            if (entity.ContainsKey("Category"))
                product.Category = entity["Category"]?.ToString() ?? string.Empty;

            if (entity.ContainsKey("ImageUrl"))
                product.ImageUrl = entity["ImageUrl"]?.ToString() ?? string.Empty;

            if (entity.ContainsKey("CreatedDate") && entity["CreatedDate"] is DateTime createdDate)
                product.CreatedDate = createdDate;

            if (entity.ContainsKey("IsAvailable"))
            {
                var isAvailableValue = entity["IsAvailable"];
                if (isAvailableValue is bool isAvailable)
                    product.IsAvailable = isAvailable;
                else if (isAvailableValue is string isAvailableStr && bool.TryParse(isAvailableStr, out bool isAvailableFromString))
                    product.IsAvailable = isAvailableFromString;
            }

            return product;
        }
    }
}
