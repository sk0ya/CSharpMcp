# CSharpMcp

[日本語版はこちら](README.ja.md)

The C# implementation of Model Context Protocol (MCP) servers provides extensions for the Claude Desktop API. This project offers various tools for file system operations, PowerShell command execution, RSS feed processing, time retrieval, and MCP server development utilities.

## Usage

```Powershell
git clone https://github.com/sk0ya/CSharpMcp
cd CSharpMcp
dotnet build
```

Each MCP server tool is implemented as an independent dotnet project and can be built and used separately. For detailed usage instructions, please refer to the README of each tool.

## Project Structure

This project is organized into three main categories:

### Utilities

- [McpInsight](McpInsight/README.md) - Debug and monitoring tool for MCP servers
  - Real-time monitoring of communication between MCP clients and servers
  - Interactive testing of MCP server commands
  - Message display in an easy-to-analyze format
  - Usable in all phases of stdio-based MCP server development

### Stdio Servers

The following servers use standard input/output (stdio) communication:

- [FileSystem](Stdio/FileSystem/README.md) - Provides file system operation functionality
  - File reading, writing, editing, and deletion
  - Directory creation and folder structure retrieval
  - ZIP compression and extraction
  - Opening files/folders with default applications

- [PowerShell](Stdio/PowerShell/README.md) - Provides a secure interface for PowerShell command execution
  - Secure command execution with allowlist validation
  - Constrained language mode for security
  - Support for parameterized commands

- [Time](Stdio/Time/README.md) - Retrieves the current time
  - Current date and time information
  - Multiple timezone support

- [Rss](Stdio/Rss/README.md) - Processes RSS feeds
  - RSS feed parsing and content extraction
  - Multiple feed processing support

### SSE (Server-Sent Events) Servers

The following servers use HTTP-based Server-Sent Events communication:

- [Dotnet](Sse/Dotnet/README.md) - .NET project development tools
  - **CreateMcpServer** - Tool for creating MCP server projects
  - **DotnetBuild** - Tool for building and analyzing .NET projects

## Getting Started

### Stdio Servers
For stdio-based servers, you can run them directly or integrate them with Claude Desktop by adding configuration to your `claude_desktop_config.json`.

Example configuration for FileSystem server:
```json
{
    "mcpServers": {
        "FileSystem": {
            "command": "dotnet",
            "args": [
                "run",
                "--project",
                "absolute\\path\\to\\CSharpMCP\\Stdio\\FileSystem",
                "--no-build",
                "--",
                "/path/to/allowed/dir"
            ]
        }
    }
}
```

### SSE Servers
For SSE-based servers, start the HTTP server and connect via the configured URL:

```bash
cd Sse/Dotnet
dotnet run
```

The server will be available at `http://localhost:7001` by default.

## Development

This project provides tools for MCP server development:

- Use **McpInsight** for debugging and testing stdio-based MCP servers
- Use **CreateMcpServer** tool to generate new MCP server project templates
- Use **DotnetBuild** tool for automated building and analysis of .NET projects

## License
This project is licensed under the [MIT License](LICENSE.txt).
