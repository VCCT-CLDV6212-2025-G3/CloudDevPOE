using CloudDevPOE.Services;
using CloudDevPOE.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Add MVC controllers with views
builder.Services.AddControllersWithViews();

// ==================== AZURE SQL DATABASE CONFIGURATION ====================
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
{
    options.UseSqlServer(sqlConnectionString);
    // Enable detailed errors in development
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});

// ==================== AZURE STORAGE CONFIGURATION ====================
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
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<CartService>();
builder.Services.AddScoped<OrderService>();

// ==================== SESSION CONFIGURATION ====================
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// ==================== DATABASE CONNECTION TEST ====================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();

    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();

        // Test database connection
        if (context.Database.CanConnect())
        {
            logger.LogInformation("✅ Database connection successful!");
            logger.LogInformation("✅ Application ready to use");
        }
        else
        {
            logger.LogError("❌ Cannot connect to database. Please check your connection string.");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "❌ Database connection failed");
        logger.LogError($"Make sure you have:");
        logger.LogError($"1. Created the database in Azure");
        logger.LogError($"2. Run DatabaseSetup_FIXED.sql");
        logger.LogError($"3. Updated connection string in appsettings.json");
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

app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

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