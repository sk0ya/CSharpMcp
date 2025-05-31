using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;

namespace McpSseProxy;

class Program
{
    static async Task Main(string[] args)
    {
        // SSE MCPサーバーのURLを引数から取得、デフォルトはlocalhost:5000
            var sseServerUrl = args.FirstOrDefault(arg => 
        !string.IsNullOrWhiteSpace(arg) && 
        Uri.IsWellFormedUriString(arg, UriKind.Absolute)) 
        ?? "http://localhost:5000/sse";
        
        SseClientTransportOptions sseClientTransportOptions = new()
        {
            Endpoint = new Uri(sseServerUrl)
        };

        SseClientTransport clientTransport = new(sseClientTransportOptions);
        
        using var cts = new CancellationTokenSource();
        var mcpClient = await McpClientFactory.CreateAsync(clientTransport, cancellationToken:cts.Token);
        var tools = await mcpClient.ListToolsAsync();
        var prompts = await mcpClient.ListPromptsAsync();

        var builder = Host.CreateEmptyApplicationBuilder(settings: null);
        builder.Services
            .AddLogging(logging => logging.SetMinimumLevel(LogLevel.None))
            .AddMcpServer(options =>
            {
                options.ServerInfo = new Implementation
                {
                    Name = "SSE Proxy Server",
                    Version = "1.0.0"
                };

                options.Capabilities = new ServerCapabilities
                {
                    Tools = new ToolsCapability
                    {
                        ListToolsHandler = (_, cancellationToken) =>
                            ValueTask.FromResult(new ListToolsResult()
                            {
                                Tools = tools.Select(x => new Tool()
                                        {Name = x.Name, Description = x.Description, InputSchema = x.JsonSchema,})
                                    .ToList(),
                            }),

                        // ツールの実行を動的にプロキシ
                        CallToolHandler = (request, cancellationToken) =>
                        {
                            if (request.Params?.Name == null)
                            {
                                throw new McpException("Tool name is required");
                            }

                            var dic = request.Params.Arguments
                                ?.ToDictionary<KeyValuePair<string, JsonElement>, string, object?>(
                                    argument => argument.Key, argument => argument.Value);

                            return mcpClient.CallToolAsync(request.Params.Name, dic,
                                cancellationToken: cancellationToken);
                        }
                    },
                    Prompts = new PromptsCapability()
                    {
                        ListPromptsHandler = (_, cancellationToken) =>
                            ValueTask.FromResult(new ListPromptsResult()
                            {
                                Prompts = prompts.Select(x => new Prompt()
                                {
                                    Name = x.ProtocolPrompt.Name, Description = x.ProtocolPrompt.Description,
                                    Arguments = x.ProtocolPrompt.Arguments
                                }).ToList(),
                            }),

                        GetPromptHandler = (request, cancellationToken) =>
                        {
                            var name = request.Params?.Name?? string.Empty;
                            var dic = request.Params?.Arguments
                                ?.ToDictionary<KeyValuePair<string, JsonElement>, string, object?>(
                                    argument => argument.Key, argument => argument.Value);
                            return mcpClient.GetPromptAsync(name, dic,
                                cancellationToken: cancellationToken);
                            
                        }
                    }
                };
            })
            .WithStdioServerTransport();

        var host = builder.Build();
        await host.RunAsync();
    }
}