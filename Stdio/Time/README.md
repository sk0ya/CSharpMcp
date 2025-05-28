# Time

[日本語版はこちら](README.ja.md)

The CSharpMcpServer Time is a module that provides time-related functionality for the Model Context Protocol (MCP) server. This component enables basic time operations such as retrieving the current system time.

## Features
- **GetCurrentTime**: Gets the current system time and returns it as a formatted string

## API Details

### GetCurrentTime
```csharp
public static string GetCurrentTime()
```
Gets the current system time and returns it as a formatted string:
- **Description**: Gets the current system time and returns it as a formatted string
- **Returns**: The current system time formatted in the general date/time format ("G" format)

## Usage with Claude Desktop
- Add this to your claude_desktop_config.json
- Requires dotnet 8.0 or higher
- Build is required

```json
{
    "mcpServers": {
        "Time": {
            "command": "dotnet",
            "args": [
                "run",
                "--project",
                "absolute\\path\\to\\CSharpMCPServer\\Servers\\Time",
                "--no-build"
            ]
        }
    }
}
```

**Important**: 
- Replace `absolute\\path\\to\\CSharpMCPServer\\Time` with your actual project path

## Time Zone

GetCurrentTime uses the local time zone of the system where the server is running. If you need UTC time, you may need to extend this module.

## Format

The time is returned in a standard date/time format. This makes it easy for Claude AI to retrieve and process time information.