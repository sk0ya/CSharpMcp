# MCP SSE Proxy Server

このプロジェクトは、SSE (Server-Sent Events) で動作するMCPサーバーを、Stdio接続のMCPクライアント（Claude DesktopやVS Codeなど）から利用できるようにするプロキシサーバーです。

## 機能

- SSE MCPサーバーのツールを動的に取得してstdioクライアントに公開
- ツール実行のプロキシ機能
- レスポンス形式の変換（SSE → stdio）
- エラーハンドリングと適切なMCPレスポンス形式への変換

## 使用方法

### 1. ビルドと実行

```bash
dotnet build
dotnet run [SSE_SERVER_URL]
```

例:
```bash
dotnet run http://localhost:5000/mcp
```

### 2. Claude Desktop / VS Codeでの設定

`.vscode/mcp.json` または `~/.config/claude-desktop/claude_desktop_config.json` に追加:

```json
{
  "mcpServers": {
    "sse-proxy": {
      "command": "dotnet",
       "args": ["run", "--project", "absolute\\path\\to\\CSharpMcp\\Stdio\\McpSseProxy","--", "http://localhost:5000/sse"]
    }
  }
}
```

### 3. 動作確認

1. SSE MCPサーバーを起動
2. このプロキシサーバーを起動
3. Claude DesktopまたはVS CodeでMCPサーバーに接続
4. SSEサーバーのツールがstdioクライアントから利用可能になります

## 動作の流れ

1. **ツール一覧取得時**: 
   - stdioクライアントが`tools/list`を要求
   - プロキシがSSEサーバーに`tools/list`をHTTP POSTで送信
   - SSEサーバーからのレスポンスをMCP Toolフォーマットに変換してstdioクライアントに返送

2. **ツール実行時**:
   - stdioクライアントが`tools/call`を要求  
   - プロキシがSSEサーバーに同じツール名と引数でHTTP POSTを送信
   - SSEサーバーからのレスポンスをMCP CallToolResultフォーマットに変換してstdioクライアントに返送

## 依存関係

- .NET 8.0
- ModelContextProtocol (preview)
- Microsoft.Extensions.Hosting
- System.Text.Json

## 注意事項

- SSEサーバーのツール構成が変更されると、動的に反映されます
- stdioクライアントからはSSEサーバーのツールを直接使用しているように見えます
- エラーハンドリングにより、接続エラーやツール実行エラーが適切に処理されます
