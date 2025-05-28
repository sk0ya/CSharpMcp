# Time

[English version here](README.md)

CSharpMcpServer Timeは、Model Context Protocol (MCP) サーバーの時間関連機能を提供するモジュールです。このコンポーネントは、システム時刻の取得などの基本的な時間操作を可能にします。

## 機能
- **GetCurrentTime**: 現在のシステム時刻を取得し、フォーマットされた文字列として返す

## API詳細

### GetCurrentTime
```csharp
public static string GetCurrentTime()
```
現在のシステム時刻を取得し、フォーマットされた文字列として返します：
- **説明**: 現在のシステム時刻を取得し、フォーマットされた文字列として返します
- **戻り値**: 一般的な日時形式でフォーマットされた現在のシステム時刻（"G"形式）

## Claude Desktopでの使用方法
- claude_desktop_config.jsonに以下を追加
- dotnet 8.0以上が必要
- ビルドが必要

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

**重要**: 
- `absolute\\path\\to\\CSharpMCPServer\\Time`の部分を実際のプロジェクトパスに置き換えてください

## タイムゾーン

GetCurrentTimeは、サーバーが実行されているシステムのローカルタイムゾーンを使用します。UTC時間が必要な場合は、このモジュールの拡張が必要になる場合があります。

## フォーマット

時間は標準的な日時フォーマットで返されます。これにより、Claude AIによる時間情報の取得と処理が容易になります。