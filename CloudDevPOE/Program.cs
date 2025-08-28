using CloudDevPOE.Services;

var builder = WebApplication.CreateBuilder(args);

// Add MVC controllers with views
builder.Services.AddControllersWithViews();

// Get Azure Storage connection string from appsettings.json
var connectionString = builder.Configuration.GetConnectionString("AzureStorage");

// Ensure the connection string is set; otherwise, throw an exception
if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException(
        "Azure Storage connection string is not configured. " +
        "Please set 'ConnectionStrings:AzureStorage' in your appsettings.json file."
    );
}

// Register Azure Storage services as singletons
// These services will be available via dependency injection in controllers
builder.Services.AddSingleton(provider => new AzureTableService(connectionString));
builder.Services.AddSingleton(provider => new AzureBlobService(connectionString));
builder.Services.AddSingleton(provider => new AzureQueueService(connectionString));
builder.Services.AddSingleton(provider => new AzureFileService(connectionString));

var app = builder.Build();

// Configure the HTTP request pipeline

if (!app.Environment.IsDevelopment())
{
    // Use custom error page in production
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts(); // Enable HTTP Strict Transport Security
}

app.UseHttpsRedirection(); // Redirect HTTP requests to HTTPS
app.UseStaticFiles();      // Serve static files (wwwroot)

app.UseRouting();           // Enable routing
app.UseAuthorization();     // Enable authorization

// Define default route pattern for MVC
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

// Start the web application
app.Run();

//////////////////////////////////////////////////////////////////////////////////////////
//REFERENCES//
//Azure Storage Services Overview: https://docs.microsoft.com/en-us/azure/storage/
//Azure Table Storage Documentation: https://docs.microsoft.com/en-us/azure/storage/tables/
//Azure.Data.Tables Client Library: https://docs.microsoft.com/en-us/dotnet/api/overview/azure/data.tables-readme
//Azure.Storage.Blobs Client Library: https://docs.microsoft.com/en-us/dotnet/api/overview/azure/storage.blobs-readme
//Azure.Storage.Queues Client Library: https://docs.microsoft.com/en-us/dotnet/api/overview/azure/storage.queues-readme
//Azure.Storage.Files.Shares Client Library: https://docs.microsoft.com/en-us/dotnet/api/overview/azure/storage.files.shares-readme
//ChatGPT - https://chatgpt.com/share/68b0aa50-64bc-8001-9693-da178269ab1f