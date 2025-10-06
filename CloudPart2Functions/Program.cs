using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .Build();

host.Run();

////////////////////////////////////////////////////////
//References
//Functions - https://www.thoughtco.com/introduction-to-functions-in-c-958367
//Azure Functions - https://learn.microsoft.com/en-us/azure/azure-functions/functions-overview?tabs=dotnet
//Azure Functions with .NET - https://learn.microsoft.com/en-us/azure/azure-functions/functions-dotnet-class-library?tabs=cmd
//Dependency Injection in Azure Functions - https://learn.microsoft.com/en-us/azure/azure-functions/functions-dotnet-dependency-injection
//Functions - https://dotnettutorials.net/lesson/functions-in-csharp/

