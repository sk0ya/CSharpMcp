using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using MsBuild;
using CreateMcpServer;
using Microsoft.AspNetCore.Hosting;

var builder = WebApplication.CreateBuilder(args);

try
{
    var configuration = builder.Configuration;
    var httpAddress = configuration["McpServer:LocalAddress"];
    
    if (!string.IsNullOrEmpty(httpAddress))
    {
        builder.WebHost.UseUrls(httpAddress);
        
        Console.WriteLine($"Server will start on:");
        Console.WriteLine($"  HTTP:  {httpAddress}");
    }
    else
    {
        Console.WriteLine("LocalAddress or LocalHttpsAddress not found in configuration. Using default URLs.");
        builder.WebHost.UseUrls("http://localhost:7001");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Configuration error: {ex.Message}");
    Console.WriteLine("Using default URLs: http://localhost:7001");
    builder.WebHost.UseUrls("http://localhost:7002");
}

builder.Services
    .AddMcpServer()
    .WithHttpTransport()
    .WithPrompts<CreateMcpServerPrompts>()
    .WithTools<CreateMcpServerTools>()
    .WithTools<DotnetBuildTools>();

var app = builder.Build();

app.MapMcp();

app.Run();
