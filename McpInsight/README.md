# McpInsight

[日本語版はこちら](README.ja.md)

## Overview
McpInsight is a debugging tool for C# MCP (Model Context Protocol) servers that communicate via standard input/output (stdio). It allows developers to test, debug, and monitor interactions between MCP clients and servers without requiring a full client implementation.

## Key Features
- **Real-time Monitoring**: Monitor MCP message traffic between clients and servers in real-time
- **Interactive Testing**: Interactive testing of MCP server commands and functionality
- **Message Analysis**: Message inspection and formatting for easy analysis and debugging
- **Protocol Support**: Full support for standard MCP protocol operations
- **Development Utilities**: Comprehensive debugging utilities for MCP server development

## Usage

### Basic Usage Steps
1. **Launch McpInsight**: Start the application
2. **Configure Server Connection**: Set up connection to your MCP server for testing
3. **Send Test Commands**: Send test commands and monitor responses
4. **Analyze Communication**: Analyze the detailed communication flow between client and server

### Supported Features
- **Tools**: Test tool functionality provided by MCP servers
- **Prompts**: Test and validate prompt functionality
- **Resources**: Verify resource access capabilities
- **Message Logging**: Detailed logging of all communication
- **Error Handling**: Detailed analysis of error conditions

## Technical Specifications

### Requirements
- **.NET 8.0 or higher**: Latest .NET runtime required
- **Windows OS**: Designed for Windows operating system
- **MCP stdio server**: stdio-based MCP server to test

### Supported MCP Servers
- FileSystem server
- PowerShell server
- Time server
- RSS server
- Other stdio-based custom MCP servers

## Developer Information

### Build and Run
```bash
cd McpInsight
dotnet build
dotnet run
```

### Debug Mode
When debugging MCP servers under development:
1. Launch McpInsight
2. Specify the path to your development server
3. Send various MCP messages for testing
4. Analyze responses and errors in detail

### Logging Features
- **Sent Messages**: Record all messages from client to server
- **Received Messages**: Record all responses from server
- **Error Tracking**: Detailed tracking of communication and protocol errors
- **Performance Measurement**: Response time measurement

## Troubleshooting

### Common Issues
- **Connection Errors**: Verify server path and arguments
- **Timeouts**: Check server startup status
- **Protocol Errors**: Validate MCP message format

### Debugging Tips
- Check server startup standard output
- Verify MCP message format is correct
- Check security restrictions (file access permissions, etc.)

## Use Cases

### Development Phase
- **Initial Development**: Test basic MCP protocol implementation
- **Feature Development**: Validate new tools and prompts
- **Integration Testing**: Test server integration with different clients
- **Performance Testing**: Monitor response times and resource usage

### Production Debugging
- **Issue Diagnosis**: Analyze communication problems in production
- **Protocol Validation**: Ensure MCP compliance
- **Performance Analysis**: Identify bottlenecks and optimization opportunities

## Architecture

McpInsight is built using:
- **Avalonia UI**: Cross-platform UI framework
- **Reactive Extensions**: For handling asynchronous message streams
- **JSON-RPC**: MCP protocol implementation
- **Process Management**: For stdio server communication

## Contributing
Contributions to McpInsight are welcome. You can contribute by:
- Submitting bug reports
- Suggesting feature improvements
- Sending pull requests
- Improving documentation

Please feel free to submit issues and pull requests to the repository.

## License
This project is licensed under the [MIT License](../LICENSE.txt).
