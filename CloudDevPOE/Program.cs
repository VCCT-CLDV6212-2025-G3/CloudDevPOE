using CloudDevPOE.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Get Azure Storage connection string
var connectionString = builder.Configuration.GetConnectionString("AzureStorage");

if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("Azure Storage connection string is not configured. " +
        "Please set 'ConnectionStrings:AzureStorage' in your appsettings.json file.");
}

// Register Azure Storage Services
builder.Services.AddSingleton(provider => new AzureTableService(connectionString));
builder.Services.AddSingleton(provider => new AzureBlobService(connectionString));
builder.Services.AddSingleton(provider => new AzureQueueService(connectionString));
builder.Services.AddSingleton(provider => new AzureFileService(connectionString));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();