# Rss

[日本語版はこちら](README.ja.md)

The CSharpMcpServer Rss is a module that provides RSS feed processing tools for the Model Context Protocol (MCP) server. This component enables retrieval of items from multiple RSS feeds and outputs them as markdown-formatted links.

## Features
- **ParseRssFeeds**: Processes multiple RSS feeds from command line arguments and formats their content as markdown links
- Capability to skip the first item of each feed
- Error handling functionality

## API Details

### ParseRssFeeds
```csharp
public static async Task<string> ParseRssFeeds()
```
Processes multiple RSS feeds and outputs titles and URLs as markdown-formatted links:
- **Description**: Processes multiple RSS feeds from command line arguments and formats their content as markdown links, ignoring the first item of each feed
- **Arguments**: RSS URLs provided through command line arguments
- **Returns**: Markdown-formatted list of links

## Usage

### Compilation and Building
- Requires dotnet 8.0 or higher
- Run the following command from the repository root directory:

```bash
dotnet build CSharpMcp/Stdio/Rss
```

### Integration with Claude Desktop
To use with Claude Desktop, add the following configuration to your `claude_desktop_config.json`:

```json
{
    "mcpServers": {
        "Rss": {
            "command": "dotnet",
            "args": [
                "run",
                "--project",
                "absolute\\path\\to\\CSharpMCP\\Stdio\\Rss",
                "--no-build",
                "--",
                "https://example.com/rss"
            ]
        }
    }
}
```

**Important**: 
- Replace `absolute\\path\\to\\CSharpMCP\\Stdio\\Rss` with your actual project path
- You can specify one or more RSS feed URLs after the `--` argument

## Security

This server only accesses the specified RSS feeds and retrieves content while skipping the first item of each feed.
