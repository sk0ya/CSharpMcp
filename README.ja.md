# CSharpMcpServer

[English version here](README.md)

C#で実装されたModel Context Protocol (MCP) サーバーは、Claude Desktop APIの拡張機能を提供します。このプロジェクトは、ファイルシステム操作、PowerShellコマンド実行、RSSフィード処理、時間取得、およびMCPサーバー開発ユーティリティなどの様々なツールを提供します。

## 使用方法

```Powershell
git clone https://github.com/sk0ya/CSharpMcp
cd CSharpMcp
dotnet build
```

MCPの各ツールは独立したdotnetプロジェクトとして実装されており、それぞれをビルドして使用できます。
詳細な使用方法は各ツールのREADMEを参照してください。

## プロジェクト構成

このプロジェクトは3つの主要なカテゴリで構成されています：

### Utilities

- [McpInsight](McpInsight/README.ja.md) - MCPサーバーのデバッグ・監視ツール
  - MCPクライアントとサーバー間の通信をリアルタイムに監視
  - MCPサーバーコマンドのインタラクティブなテスト
  - メッセージを分析しやすい形式で表示
  - stdioベースのMCPサーバー開発のあらゆるフェーズで利用可能

### Stdio サーバー

以下のサーバーは標準入出力(stdio)通信を使用します：

- [FileSystem](Stdio/FileSystem/README.ja.md) - ファイルシステム操作機能を提供
  - ファイルの読み書き、編集、削除機能
  - ディレクトリの作成、フォルダ構造の取得機能
  - ZIP圧縮・解凍機能
  - ファイル/フォルダを規定アプリで開く機能

- [PowerShell](Stdio/PowerShell/README.ja.md) - PowerShellコマンド実行のためのセキュアなインターフェースを提供
  - 許可リストによる安全なコマンド実行
  - セキュリティのための制約言語モード
  - パラメータ化されたコマンドのサポート

- [Time](Stdio/Time/README.ja.md) - 現在の時刻を取得
  - 現在の日付と時刻情報
  - 複数のタイムゾーンサポート

- [Rss](Stdio/Rss/README.ja.md) - RSSフィードを処理
  - RSSフィードの解析とコンテンツ抽出
  - 複数フィードの処理サポート

### SSE (Server-Sent Events) サーバー

以下のサーバーはHTTPベースのServer-Sent Events通信を使用します：

- [Dotnet](Sse/Dotnet/README.md) - .NETプロジェクト開発ツール
  - **CreateMcpServer** - MCPサーバープロジェクトの作成ツール
  - **DotnetBuild** - .NETプロジェクトのビルドと分析のためのツール

## はじめに

### Stdio サーバー
stdioベースのサーバーは、直接実行するか、`claude_desktop_config.json`に設定を追加してClaude Desktopと統合できます。

FileSystemサーバーの設定例：
```json
{
    "mcpServers": {
        "FileSystem": {
            "command": "dotnet",
            "args": [
                "run",
                "--project",
                "absolute\\path\\to\\CSharpMCPServer\\Stdio\\FileSystem",
                "--no-build",
                "--",
                "/path/to/allowed/dir"
            ]
        }
    }
}
```

### SSE サーバー
SSEベースのサーバーの場合、HTTPサーバーを起動し、設定されたURLを介して接続します：

```bash
cd Sse/Dotnet
dotnet run
```

サーバーはデフォルトで`http://localhost:7001`で利用可能です。

## 開発

このプロジェクトはMCPサーバー開発のためのツールを提供します：

- **McpInsight**を使用してstdioベースのMCPサーバーのデバッグとテストを行う
- **CreateMcpServer**ツールを使用して新しいMCPサーバープロジェクトテンプレートを生成する
- **DotnetBuild**ツールを使用して.NETプロジェクトの自動ビルドと分析を行う

## ライセンス
このプロジェクトは[MITライセンス](LICENSE.txt)の下でライセンスされています。
