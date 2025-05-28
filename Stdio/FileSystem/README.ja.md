# FileSystem

[English version here](README.md)

CSharpMcpServer FileSystem は、Model Context Protocol (MCP) サーバーのファイルシステム操作ツールを提供するモジュールです。このコンポーネントは、ファイルの読み込み、編集、削除、ディレクトリ構造の取得などの基本的なファイルシステム操作を可能にします。

## 機能
- **GetFileInfo**: ファイルの情報（パス、行数、内容）を取得
- **WriteFile**: ファイルの内容を書き込み
- **EditFile**: 指定テキストを置換してファイルを編集
- **Delete**: ファイルまたはディレクトリを削除（ディレクトリの場合は再帰的削除オプションあり）
- **CreateDirectory**: ディレクトリを作成
- **Move**: ファイルまたはディレクトリを移動
- **GetFolderStructure**: 指定されたディレクトリの階層構造をYAML形式で取得（.gitignoreに基づく除外処理付き）
- **Zip**: ディレクトリまたはファイルを圧縮してZIPファイルを作成
- **Unzip**: ZIPファイルを展開
- **OpenWithApplication**: ファイルまたはフォルダを規定のアプリケーションで開く

## API詳細

### GetFileInfo
```csharp
public static async Task<string> GetFileInfoAsync(string filePath, string encodingName = "utf-8", bool includeContent = true)
```
指定されたファイルの情報を取得します：
- **説明**: ファイル情報（パス、行数、内容など）を取得します
- **filePath**: 読み込むファイルの完全パス
- **encodingName**: 使用するエンコーディング（utf-8, shift-jis, etc.）、デフォルトはutf-8
- **includeContent**: 結果にファイル内容を含めるかどうか。大きいファイルの場合はfalseを推奨
- **戻り値**: ファイルパス、行数、内容を含むJSON形式の情報

### WriteFile
```csharp
public static string WriteFile(string filePath, string content, string encodingName = "utf-8")
```
ファイルに内容を書き込みます：
- **説明**: ファイルを書き込みます
- **filePath**: 編集するファイルのパス
- **content**: ファイルに書き込む内容
- **encodingName**: 使用するエンコーディング（utf-8, shift-jis, etc.）、デフォルトはutf-8
- **戻り値**: 処理結果

### EditFile
```csharp
public static async Task<string> EditFileAsync(string filePath, string oldString, string newString, string encodingName = "utf-8", int replacementCount = 1)
```
指定テキストを置換してファイルを編集します：
- **説明**: 指定テキストを置換してファイルを編集します
- **filePath**: 編集するファイルのパス
- **oldString**: 置換対象のテキスト
- **newString**: 置換後のテキスト
- **encodingName**: 使用するエンコーディング（utf-8, shift-jis, etc.）、デフォルトはutf-8
- **replacementCount**: 実行する置換の回数（デフォルト: 1）
- **戻り値**: 処理結果

### Delete
```csharp
public static string Delete(string fullPath, bool recursive = false)
```
ファイルまたはディレクトリを削除します：
- **説明**: ファイルまたはディレクトリをファイルシステムから削除します
- **fullPath**: 削除するファイルまたはディレクトリの完全パス
- **recursive**: ディレクトリ内のすべてのコンテンツを削除するかどうか（ファイルの場合は無視、デフォルトはfalse）
- **戻り値**: 処理結果のJSONメッセージ

### CreateDirectory
```csharp
public static string CreateDirectory(string directoryPath)
```
ディレクトリを作成します：
- **説明**: ディレクトリを作成します
- **directoryPath**: 作成するディレクトリのパス
- **戻り値**: 処理結果のJSONメッセージ

### Move
```csharp
public static string Move(string sourcePath, string destinationPath, bool overwrite = false)
```
ファイルまたはディレクトリを移動します：
- **説明**: ファイルまたはディレクトリを新しい場所に移動します
- **sourcePath**: 移動するファイルまたはディレクトリのパス
- **destinationPath**: 移動先のパス
- **overwrite**: 既存ファイルを上書きするかどうか（ディレクトリの場合は無視、デフォルトはfalse）
- **戻り値**: 処理結果のJSONメッセージ

### GetFolderStructure
```csharp
public static async Task<string> GetFolderStructureAsync(string fullPath, bool recursive = true, string format = "yaml", string excludePattern = "")
```
指定されたディレクトリの階層構造をYAML形式で取得します：
- **説明**: 指定されたディレクトリパスから階層的なフォルダ構造をYAML形式で取得します
- **fullPath**: フォルダ構造を取得するルートディレクトリの絶対パス
- **recursive**: フォルダ構造にサブディレクトリを再帰的に含めるかどうかを指定（trueの場合、すべてのネストされたディレクトリを走査、falseの場合はルートディレクトリの直接の子のみ含む、デフォルトはtrue）
- **format**: 出力形式（yaml または json）
- **excludePattern**: 除外するファイル/ディレクトリのパターン（正規表現）
- **戻り値**: YAMLまたはJSON形式のフォルダ構造

### Zip
```csharp
public static async Task<string> ZipAsync(string path, string outputPath = "", string compressionLevel = "Optimal")
```
指定されたディレクトリまたはファイルをZIP形式に圧縮します：
- **説明**: 圧縮ファイルを作成します
- **path**: 圧縮するディレクトリまたはファイルのパス
- **outputPath**: 出力するZIPファイルのパス（省略時は自動生成）
- **compressionLevel**: 圧縮レベル（Fastest, Optimal, NoCompression）、デフォルトは"Optimal"
- **戻り値**: 処理結果のJSONメッセージ

### Unzip
```csharp
public static async Task<string> UnzipAsync(string filePath, string outputPath = "", bool overwrite = false)
```
ZIPファイルを展開します：
- **説明**: 圧縮ファイルを展開します
- **filePath**: 展開するZIPファイルのパス
- **outputPath**: 展開先ディレクトリのパス（省略時は自動生成）
- **overwrite**: 既存ファイルを上書きするかどうか（デフォルトはfalse）
- **戻り値**: 処理結果のJSONメッセージ

### OpenWithApplication
```csharp
public static string OpenWithApplication(string path, string verb = "open")
```
ファイルまたはフォルダを規定のアプリケーションで開きます：
- **説明**: ファイルまたはフォルダを規定のアプリケーションで開く
- **path**: ファイルまたはフォルダのパス
- **verb**: 使用する動詞（open, edit, print など）、デフォルトは"open"
- **戻り値**: 処理結果のJSONメッセージ

### OpenWithSpecificApplication
```csharp
public static string OpenWithSpecificApplication(string filePath, string applicationPath, string arguments = "")
```
ファイルを特定のアプリケーションで開きます：
- **説明**: ファイルを指定されたプログラムで開く
- **filePath**: 開くファイルのパス
- **applicationPath**: 使用するアプリケーションのパス
- **arguments**: 追加のコマンドライン引数
- **戻り値**: 処理結果のJSONメッセージ

## 使用方法

### コンパイルとビルド
- dotnet 8.0以上が必要
- リポジトリのルートディレクトリから以下のコマンドを実行:

```bash
dotnet build CSharpMcp/Stdio/FileSystem
```

### Claude Desktopとの連携
Claude Desktopで使用するには、以下の設定を`claude_desktop_config.json`に追加します:

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

**重要**: 
- `absolute\\path\\to\\CSharpMCP\\Stdio\\FileSystem`の部分を実際のプロジェクトパスに置き換えてください
- 必要に応じて`/path/to/other/allowed/dir`にアクセスを許可したい追加のディレクトリを指定できます

## セキュリティ

このサーバーは指定されたディレクトリとその子ディレクトリのみにアクセスを制限します。

## .gitignore対応

GetFolderStructureには、.gitignoreファイルを解析する機能が含まれています：

- ルートディレクトリの.gitignoreを読み込み
- サブディレクトリの.gitignoreも適切に処理
- 絶対パスと相対パスのパターン対応
- ワイルドカード（*、**、？）の変換
- ディレクトリ専用パターン（末尾の/）対応

さらに、以下のような一般的なファイル/ディレクトリが自動的に除外されます：

- .git、.nextディレクトリ
- bin、obj、target、distなどのビルド出力
- .vs、*.user、*.suoなどのVisual Studioファイル
- node_modules、packagesなどのパッケージディレクトリ
- ログファイル、バックアップファイル、キャッシュファイル
