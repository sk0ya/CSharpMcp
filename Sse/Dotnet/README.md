# Dotnet SSE Server

[日本語版はこちら](README.ja.md)

The Dotnet SSE Server provides HTTP-based MCP (Model Context Protocol) server functionality using Server-Sent Events. This server includes tools for .NET project development and management.

## Features

This server combines multiple tools for .NET development:

### CreateMcpServer
- **CreateMcpServerProject**: Creates new MCP server project templates
- **Purpose**: Helps developers quickly scaffold new MCP server projects with proper structure and dependencies

### DotnetBuild  
- **BuildDotnetProject**: Builds .NET projects and provides detailed build analysis
- **Purpose**: Automated building and analysis of .NET projects with comprehensive error reporting

## Configuration

The server can be configured using `appsettings.json`:

```json
{
  "McpServer": {
    "LocalAddress": "http://localhost:7001"
  }
}
```

## Usage

### Starting the Server

```bash
cd Sse/Dotnet
dotnet run
```

The server will start on the configured address (default: `http://localhost:7001`).

### Integration with MCP Clients

Since this is an SSE-based server, it communicates over HTTP rather than stdio. Configure your MCP client to connect to the server's HTTP endpoint.

Example configuration for Claude Desktop:
```json
{
    "mcpServers": {
        "DotnetTools": {
            "command": "node",
            "args": [
                "path/to/sse-client.js",
                "http://localhost:7001"
            ]
        }
    }
}
```

## Available Tools

### CreateMcpServerProject
Creates a new MCP server project with the specified feature set.

**Parameters:**
- `feature`: The feature/functionality to include in the new MCP server project

**Usage:**
```javascript
// Example usage through MCP client
{
    "method": "tools/call",
    "params": {
        "name": "CreateMcpServerProject",
        "arguments": {
            "feature": "file-operations"
        }
    }
}
```

### BuildDotnetProject
Builds a .NET project and returns detailed build information.

**Parameters:**
- Project path and build configuration parameters

**Usage:**
```javascript
// Example usage through MCP client
{
    "method": "tools/call", 
    "params": {
        "name": "BuildDotnetProject",
        "arguments": {
            "projectPath": "/path/to/project.csproj",
            "configuration": "Release"
        }
    }
}
```

## Development

### Requirements
- .NET 8.0 or higher
- ASP.NET Core runtime

### Building
```bash
dotnet build
```

### Running in Development Mode
```bash
dotnet run --environment Development
```

## Architecture

This server uses:
- **ASP.NET Core** for HTTP hosting
- **Server-Sent Events (SSE)** for real-time communication
- **MCP Protocol** for standardized tool and prompt interfaces
- **Dependency Injection** for service management

## License
This project is licensed under the [MIT License](../../LICENSE.txt).
