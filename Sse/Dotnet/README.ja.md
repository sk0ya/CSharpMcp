# Dotnet SSE Server

[English version here](README.md)

Dotnet SSE サーバーは、Server-Sent Events を使用したHTTPベースのMCP（Model Context Protocol）サーバー機能を提供します。このサーバーには .NET プロジェクトの開発と管理のためのツールが含まれています。

## 機能

このサーバーは .NET 開発のための複数のツールを組み合わせています：

### CreateMcpServer
- **CreateMcpServerProject**: 新しいMCPサーバープロジェクトテンプレートを作成
- **目的**: 開発者が適切な構造と依存関係を持つ新しいMCPサーバープロジェクトを迅速に構築するのを支援

### DotnetBuild  
- **BuildDotnetProject**: .NETプロジェクトをビルドし、詳細なビルド分析を提供
- **目的**: 包括的なエラーレポートを含む .NET プロジェクトの自動ビルドと分析

## 設定

サーバーは `appsettings.json` を使用して設定できます：

```json
{
  "McpServer": {
    "LocalAddress": "http://localhost:7001"
  }
}
```

## 使用方法

### サーバーの起動

```bash
cd Sse/Dotnet
dotnet run
```

サーバーは設定されたアドレス（デフォルト: `http://localhost:7001`）で起動します。

### MCPクライアントとの統合

これはSSEベースのサーバーなので、stdioではなくHTTP経由で通信します。MCPクライアントをサーバーのHTTPエンドポイントに接続するように設定してください。

Claude Desktop の設定例：
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

## 利用可能なツール

### CreateMcpServerProject
指定された機能セットを持つ新しいMCPサーバープロジェクトを作成します。

**パラメータ:**
- `feature`: 新しいMCPサーバープロジェクトに含める機能/機能性

**使用例:**
```javascript
// MCPクライアント経由の使用例
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
.NETプロジェクトをビルドし、詳細なビルド情報を返します。

**パラメータ:**
- プロジェクトパスとビルド設定パラメータ

**使用例:**
```javascript
// MCPクライアント経由の使用例
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

## 開発

### 要件
- .NET 8.0 以上
- ASP.NET Core ランタイム

### ビルド
```bash
dotnet build
```

### 開発モードでの実行
```bash
dotnet run --environment Development
```

## アーキテクチャ

このサーバーは以下を使用します：
- **ASP.NET Core** HTTPホスティング用
- **Server-Sent Events (SSE)** リアルタイム通信用
- **MCP Protocol** 標準化されたツールとプロンプトインターフェース用
- **Dependency Injection** サービス管理用

## ライセンス
このプロジェクトは[MITライセンス](../../LICENSE.txt)の下でライセンスされています。
