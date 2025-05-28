# Rss

[English version here](README.md)

CSharpMcpServer Rss は、Model Context Protocol (MCP) サーバーのRSSフィード処理ツールを提供するモジュールです。このコンポーネントは、複数のRSSフィードからアイテムを取得し、マークダウン形式のリンクとして出力する機能を提供します。

## 機能
- **ParseRssFeeds**: コマンドライン引数から複数のRSSフィードを処理し、その内容をマークダウン形式のリンクとして出力
- 各フィードの最初のアイテムをスキップする機能
- エラーハンドリング機能付き

## API詳細

### ParseRssFeeds
```csharp
public static async Task<string> ParseRssFeeds()
```
複数のRSSフィードを処理し、タイトルとURLをマークダウン形式のリンクとして出力します：
- **説明**: コマンドライン引数から複数のRSSフィードを処理し、その内容をマークダウン形式のリンクとして出力します（各フィードの最初のアイテムはスキップ）
- **引数**: コマンドライン引数を通じてRSS URLを指定
- **戻り値**: マークダウン形式のリンクリスト

## 使用方法

### コンパイルとビルド
- dotnet 8.0以上が必要
- リポジトリのルートディレクトリから以下のコマンドを実行:

```bash
dotnet build CSharpMcp/Stdio/Rss
```

### Claude Desktopとの連携
Claude Desktopで使用するには、以下の設定を`claude_desktop_config.json`に追加します:

```json
{
    "mcpServers": {
        "Rss": {
            "command": "dotnet",
            "args": [
                "run",
                "--project",
                "absolute\\path\\to\\\\CSharpMCP\\Stdio\\Rss",
                "--no-build",
                "--",
                "https://example.com/rss"
            ]
        }
    }
}
```

**重要**: 
- `absolute\\\\path\\\\to\\\\CSharpMCPServer\\\\Stdio\\\\Rss`の部分を実際のプロジェクトパスに置き換えてください
- URLの部分にRSSフィードのURLを指定できます（複数指定可能）

## セキュリティ

このサーバーは指定されたRSSフィードのみにアクセスし、最初のアイテムをスキップしてコンテンツを取得します。
