using CloudDevPOE.Services;
using CloudDevPOE.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Add MVC controllers with views
builder.Services.AddControllersWithViews();

// ==================== AZURE SQL DATABASE CONFIGURATION ====================
// Get Azure SQL connection string from appsettings.json
var sqlConnectionString = builder.Configuration.GetConnectionString("AzureSqlDatabase");

if (string.IsNullOrEmpty(sqlConnectionString))
{
    throw new InvalidOperationException(
        "Azure SQL Database connection string is not configured. " +
        "Please set 'ConnectionStrings:AzureSqlDatabase' in your appsettings.json file."
    );
}

// Register Entity Framework DbContext with Azure SQL Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(sqlConnectionString));

// ==================== AZURE STORAGE CONFIGURATION ====================
// Get Azure Storage connection string from appsettings.json
var storageConnectionString = builder.Configuration.GetConnectionString("AzureStorage");

if (string.IsNullOrEmpty(storageConnectionString))
{
    throw new InvalidOperationException(
        "Azure Storage connection string is not configured. " +
        "Please set 'ConnectionStrings:AzureStorage' in your appsettings.json file."
    );
}

// Register Azure Storage services as singletons
builder.Services.AddSingleton(provider => new AzureTableService(storageConnectionString));
builder.Services.AddSingleton(provider => new AzureBlobService(storageConnectionString));
builder.Services.AddSingleton(provider => new AzureQueueService(storageConnectionString));
builder.Services.AddSingleton(provider => new AzureFileService(storageConnectionString));

// ==================== AUTHENTICATION CONFIGURATION ====================
// Configure Cookie Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(2);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Lax;
    });

builder.Services.AddAuthorization();

// ==================== APPLICATION SERVICES ====================
// Register scoped services for SQL Database operations
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<CartService>();
builder.Services.AddScoped<OrderService>();

// ==================== SESSION CONFIGURATION (Optional) ====================
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// ==================== DATABASE INITIALIZATION ====================
// Ensure database is created and migrations are applied
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        context.Database.EnsureCreated(); // Creates database if it doesn't exist
        // For production, use: context.Database.Migrate(); // Applies pending migrations
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while creating/migrating the database.");
    }
}

// ==================== HTTP REQUEST PIPELINE ====================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Authentication & Authorization middleware (ORDER MATTERS!)
app.UseAuthentication();
app.UseAuthorization();

// Session middleware (if using session)
app.UseSession();

// Define default route pattern for MVC
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

// Start the web application
app.Run();


///////////////////////////////////////////////////////////////////////////////////////////
//REFERENCES//
//Azure Storage Services Overview: https://docs.microsoft.com/en-us/azure/storage/
//Azure Table Storage Documentation: https://docs.microsoft.com/en-us/azure/storage/tables/
//Azure.Data.Tables Client Library: https://docs.microsoft.com/en-us/dotnet/api/overview/azure/data.tables-readme
//Azure.Storage.Blobs Client Library: https://docs.microsoft.com/en-us/dotnet/api/overview/azure/storage.blobs-readme
//Azure.Storage.Queues Client Library: https://docs.microsoft.com/en-us/dotnet/api/overview/azure/storage.queues-readme
//Azure.Storage.Files.Shares Client Library: https://docs.microsoft.com/en-us/dotnet/api/overview/azure/storage.files.shares-readme
//ChatGPT - https://chatgpt.com/share/68b0aa50-64bc-8001-9693-da178269ab1f