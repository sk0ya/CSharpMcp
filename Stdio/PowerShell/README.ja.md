# PowerShell

[English version here](README.md)

CSharpMcpServer PowerShellモジュールは、MCP（Model Context Protocol）環境内でPowerShellコマンドを安全に実行するためのインターフェースを提供します。事前に定義されたPowerShellコマンドレットのセットを制御された方法で実行でき、不正な操作を防ぐためのセキュリティ対策を実装しています。

## 機能

- **GetAvailableCommands**: 実行が許可されているPowerShellコマンドのリストを取得します
  - 許可されたコマンドをJSON形式で返します

- **ExecuteCommand**: セキュリティ制約の下でPowerShellコマンドを安全に実行します
  - コマンドを許可リストと照合して検証します
  - 制約付き言語モードでPowerShellを実行します
  - 構造化されたJSON入力によるコマンドパラメータの処理をサポートします

## セキュリティ対策

PowerShellモジュールは複数のセキュリティメカニズムを実装しています：

- **コマンド許可リスト**: 明示的に許可されたコマンドのみが実行可能
- **制約付き言語モード**: PowerShellの言語機能を制限されたサブセットに限定
- **Just Enough Administration (JEA)**: コマンドの可視性と実行を制御
- **パラメータのサニタイズ**: パラメータが適切にフォーマットされエスケープされることを保証

## API詳細

### GetAvailableCommands

```csharp
public static string GetAvailableCommands()
```

利用可能なPowerShellコマンドのリストを取得します：
- **説明**: 利用可能なPowerShellコマンド一覧を取得します
- **戻り値**: 許可されたコマンドのリストを含むJSON形式の文字列

### ExecuteCommand

```csharp
public static string ExecuteCommand(string command, string parameters = "{}")
```

PowerShellコマンドを安全に実行します：
- **説明**: PowerShellコマンドを安全に実行します
- **command**: 実行するPowerShellコマンド
- **parameters**: コマンドパラメータを含むJSON形式の文字列（デフォルト: 空のオブジェクト）
- **戻り値**: コマンド実行結果を文字列として返します

## 使用例

```csharp
// 利用可能なコマンドのリストを取得
string availableCommands = GetAvailableCommands();

// パラメータなしでPowerShellコマンドを実行
string result = ExecuteCommand("Get-Process");

// パラメータ付きでPowerShellコマンドを実行
string paramResult = ExecuteCommand("Get-Process", "{ \"Name\": \"explorer\" }");
```

## Claude Desktopとの統合

Claude Desktopで使用するには、以下の設定を`claude_desktop_config.json`に追加します：

```json
{
    "mcpServers": {
        "PowerShell": {
            "command": "dotnet",
            "args": [
                "run",
                "--project",
                "absolute\\path\\to\\CSharpMCP\\Stdio\\PowerShell",
                "--no-build"
            ]
        }
    }
}
```

**重要**: 
- `absolute\\path\\to\\CSharpMCP\\Stdio\\PowerShell`の部分を実際のプロジェクトパスに置き換えてください

## デフォルトで許可されているコマンド

モジュールにはデフォルトで以下のコマンドが許可されています：
- Get-Process
- Get-Service
- Get-Item
- Get-ChildItem
- Get-Content
- Select-Object
- Where-Object
- ForEach-Object
- Sort-Object
- Format-List
- Format-Table
- Out-String

追加のコマンドは `allowed_commands.json` リソースファイルを通じて設定できます。

## コマンド実行パイプライン

1. 許可リストに対するコマンドの検証
2. パラメータの準備とフォーマット
3. 安全なPowerShell環境のセットアップ
4. エラー処理付きコマンド実行
5. 結果のフォーマットと返却

## カスタム設定

許可コマンドリストは埋め込みリソース `Resources/allowed_commands.json` を変更することでカスタマイズできます。

## ビルド

### コンパイルとビルド
- dotnet 8.0以上が必要
- リポジトリのルートディレクトリから以下のコマンドを実行:

```bash
dotnet build CSharpMcp/Stdio/PowerShell
```

## セキュリティ考慮事項

このサーバーは複数層のセキュリティを実装しています：
- 事前承認されたコマンドのみが実行可能
- PowerShellは制約言語モードで実行
- すべてのパラメータが検証・サニタイズされる
- 許可されたコマンド以外のシステムリソースへの直接アクセスなし
