using Azure.Data.Tables;
using CloudDevPOE.Models;

namespace CloudDevPOE.Services
{
    public class AzureTableService
    {
        private readonly TableServiceClient _tableServiceClient;
        private readonly string _customerTableName = "CustomerProfiles";
        private readonly string _productTableName = "Products";

        public AzureTableService(string connectionString)
        {
            _tableServiceClient = new TableServiceClient(connectionString);
        }

        // Customer Operations (unchanged)
        public async Task<CustomerProfile> CreateCustomerAsync(CustomerProfile customer)
        {
            var tableClient = _tableServiceClient.GetTableClient(_customerTableName);
            await tableClient.CreateIfNotExistsAsync();

            customer.RowKey = Guid.NewGuid().ToString();
            var response = await tableClient.AddEntityAsync(customer);
            return customer;
        }

        public async Task<CustomerProfile> GetCustomerAsync(string customerId)
        {
            var tableClient = _tableServiceClient.GetTableClient(_customerTableName);
            var response = await tableClient.GetEntityAsync<CustomerProfile>("Customer", customerId);
            return response.Value;
        }

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

        public async Task<CustomerProfile> UpdateCustomerAsync(CustomerProfile customer)
        {
            var tableClient = _tableServiceClient.GetTableClient(_customerTableName);
            var response = await tableClient.UpdateEntityAsync(customer, customer.ETag);
            return customer;
        }

        public async Task DeleteCustomerAsync(string customerId)
        {
            var tableClient = _tableServiceClient.GetTableClient(_customerTableName);
            await tableClient.DeleteEntityAsync("Customer", customerId);
        }

        // FIXED Product Operations - Using TableEntity to handle type conversions
        public async Task<Product> CreateProductAsync(Product product)
        {
            var tableClient = _tableServiceClient.GetTableClient(_productTableName);
            await tableClient.CreateIfNotExistsAsync();

            product.RowKey = Guid.NewGuid().ToString();
            var response = await tableClient.AddEntityAsync(product);
            return product;
        }

        public async Task<Product> GetProductAsync(string productId)
        {
            var tableClient = _tableServiceClient.GetTableClient(_productTableName);

            try
            {
                // First try to get as Product directly
                var response = await tableClient.GetEntityAsync<Product>("Product", productId);
                return response.Value;
            }
            catch (InvalidCastException)
            {
                // If that fails, get as TableEntity and convert manually
                var entityResponse = await tableClient.GetEntityAsync<TableEntity>("Product", productId);
                return ConvertTableEntityToProduct(entityResponse.Value);
            }
        }

        public async Task<List<Product>> GetAllProductsAsync()
        {
            var tableClient = _tableServiceClient.GetTableClient(_productTableName);
            var products = new List<Product>();

            try
            {
                // First try to query as Product directly
                await foreach (var product in tableClient.QueryAsync<Product>())
                {
                    products.Add(product);
                }
            }
            catch (InvalidCastException)
            {
                // If that fails, query as TableEntity and convert manually
                await foreach (var entity in tableClient.QueryAsync<TableEntity>())
                {
                    products.Add(ConvertTableEntityToProduct(entity));
                }
            }

            return products;
        }

        public async Task<List<Product>> GetProductsByCategoryAsync(string category)
        {
            var tableClient = _tableServiceClient.GetTableClient(_productTableName);
            var products = new List<Product>();

            try
            {
                // First try to query as Product directly
                await foreach (var product in tableClient.QueryAsync<Product>(p => p.Category == category))
                {
                    products.Add(product);
                }
            }
            catch (InvalidCastException)
            {
                // If that fails, get all entities and filter manually
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

        public async Task<Product> UpdateProductAsync(Product product)
        {
            var tableClient = _tableServiceClient.GetTableClient(_productTableName);
            var response = await tableClient.UpdateEntityAsync(product, product.ETag);
            return product;
        }

        public async Task DeleteProductAsync(string productId)
        {
            var tableClient = _tableServiceClient.GetTableClient(_productTableName);
            await tableClient.DeleteEntityAsync("Product", productId);
        }

        // Helper method to convert TableEntity to Product with proper type handling
        private Product ConvertTableEntityToProduct(TableEntity entity)
        {
            var product = new Product
            {
                PartitionKey = entity.PartitionKey,
                RowKey = entity.RowKey,
                Timestamp = entity.Timestamp,
                ETag = entity.ETag
            };

            // Handle ProductName
            if (entity.ContainsKey("ProductName"))
                product.ProductName = entity["ProductName"]?.ToString() ?? string.Empty;

            // Handle Description
            if (entity.ContainsKey("Description"))
                product.Description = entity["Description"]?.ToString() ?? string.Empty;

            // Handle Price with type conversion
            if (entity.ContainsKey("Price"))
            {
                var priceValue = entity["Price"];
                if (priceValue is string priceStr && double.TryParse(priceStr, out double priceFromString))
                {
                    product.Price = priceFromString;
                }
                else if (priceValue is double priceDouble)
                {
                    product.Price = priceDouble;
                }
                else if (priceValue is decimal priceDecimal)
                {
                    product.Price = (double)priceDecimal;
                }
            }

            // Handle StockQuantity with type conversion
            if (entity.ContainsKey("StockQuantity"))
            {
                var stockValue = entity["StockQuantity"];
                if (stockValue is string stockStr && int.TryParse(stockStr, out int stockFromString))
                {
                    product.StockQuantity = stockFromString;
                }
                else if (stockValue is int stockInt)
                {
                    product.StockQuantity = stockInt;
                }
                else if (stockValue is long stockLong)
                {
                    product.StockQuantity = (int)stockLong;
                }
            }

            // Handle Category
            if (entity.ContainsKey("Category"))
                product.Category = entity["Category"]?.ToString() ?? string.Empty;

            // Handle ImageUrl
            if (entity.ContainsKey("ImageUrl"))
                product.ImageUrl = entity["ImageUrl"]?.ToString() ?? string.Empty;

            // Handle CreatedDate
            if (entity.ContainsKey("CreatedDate") && entity["CreatedDate"] is DateTime createdDate)
                product.CreatedDate = createdDate;

            // Handle IsAvailable
            if (entity.ContainsKey("IsAvailable"))
            {
                var isAvailableValue = entity["IsAvailable"];
                if (isAvailableValue is bool isAvailable)
                {
                    product.IsAvailable = isAvailable;
                }
                else if (isAvailableValue is string isAvailableStr && bool.TryParse(isAvailableStr, out bool isAvailableFromString))
                {
                    product.IsAvailable = isAvailableFromString;
                }
            }

            return product;
        }
    }
}