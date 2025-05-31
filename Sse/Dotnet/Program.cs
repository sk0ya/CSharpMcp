using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using CreateMcpServer;
using Dotnet.CreatMcpServer;
using Dotnet.MsBuild;
using Microsoft.AspNetCore.Hosting;

internal class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var httpAddress = string.Empty;
        try
        {
            var configuration = builder.Configuration;
            httpAddress = configuration["McpServer:LocalAddress"];
    
            if (!string.IsNullOrEmpty(httpAddress))
            {
                Console.WriteLine($"Server will start on:");
                Console.WriteLine($"  HTTP:  {httpAddress}");
            }
            else
            {
                Console.WriteLine("LocalAddress or LocalHttpsAddress not found in configuration. Using default URLs.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Configuration error: {ex.Message}");
            Console.WriteLine("Using default URLs: http://localhost:7001");
        }

        builder.Services
            .AddMcpServer()
            .WithHttpTransport()
            .WithPrompts<CreateMcpServerPrompts>()
            .WithTools<CreateMcpServerTools>()
            .WithTools<DotnetBuildTools>();

        var app = builder.Build();

        app.MapMcp();

        app.Run(httpAddress);
    }
}
