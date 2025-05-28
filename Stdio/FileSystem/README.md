# FileSystem

[日本語版はこちら](README.ja.md)

The CSharpMcpServer FileSystem is a module that provides file system operation tools for the Model Context Protocol (MCP) server. This component enables basic file system operations such as reading files, editing, deletion, and retrieving directory structures.

## Features
- **GetFileInfo**: Get file information including path, line count, and content
- **WriteFile**: Write content to a file
- **EditFile**: Edit a file by replacing specified text
- **Delete**: Delete a file or directory (with recursive deletion option for directories)
- **CreateDirectory**: Create a directory
- **Move**: Move a file or directory to a new location
- **GetFolderStructure**: Get the hierarchical structure of a specified directory in YAML format (with exclusion processing based on .gitignore)
- **Zip**: Compress a directory or file to create a ZIP file
- **Unzip**: Extract a ZIP file
- **OpenWithApplication**: Open a file or folder with the default application

## API Details

### GetFileInfo
```csharp
public static async Task<string> GetFileInfoAsync(string filePath, string encodingName = "utf-8", bool includeContent = true)
```
Gets information about the specified file:
- **Description**: Gets file information including path, line count and content.
- **filePath**: The full path to the file to be read
- **encodingName**: The encoding to use (utf-8, shift-jis, etc.). Default is utf-8
- **includeContent**: Whether to include file content in the result. For large files, setting this to false is recommended
- **Returns**: JSON-formatted information including file path, line count, and content

### WriteFile
```csharp
public static string WriteFile(string filePath, string content, string encodingName = "utf-8")
```
Writes content to a file:
- **Description**: Write file
- **filePath**: The path to the file to edit
- **content**: The content to write to the file
- **encodingName**: The encoding to use (utf-8, shift-jis, etc.). Default is utf-8
- **Returns**: result message

### EditFile
```csharp
public static async Task<string> EditFileAsync(string filePath, string oldString, string newString, string encodingName = "utf-8", int replacementCount = 1)
```
Edits a file by replacing specified text:
- **Description**: Edit file by replacing specified text
- **filePath**: The path to the file to edit
- **oldString**: The text to replace
- **newString**: The text to replace it with
- **encodingName**: The encoding to use (utf-8, shift-jis, etc.). Default is utf-8
- **replacementCount**: The expected number of replacements to perform. Defaults to 1 if not specified.
- **Returns**: result message

### Delete
```csharp
public static string Delete(string fullPath, bool recursive = false)
```
Deletes a file or directory:
- **Description**: Deletes a file or directory from the file system.
- **fullPath**: The full path of the file or directory to delete
- **recursive**: Whether to delete all contents inside a directory. Ignored for files. Default is false.
- **Returns**: JSON-formatted result message

### CreateDirectory
```csharp
public static string CreateDirectory(string directoryPath)
```
Creates a directory:
- **Description**: Creates a directory.
- **directoryPath**: The path of the directory to create.
- **Returns**: JSON-formatted result message

### Move
```csharp
public static string Move(string sourcePath, string destinationPath, bool overwrite = false)
```
Moves a file or directory:
- **Description**: Moves a file or directory to a new location.
- **sourcePath**: The path of the file or directory to move.
- **destinationPath**: The path to move the file or directory to.
- **overwrite**: Whether to overwrite an existing file. Ignored for directories. Default is false.
- **Returns**: JSON-formatted result message

### GetFolderStructure
```csharp
public static async Task<string> GetFolderStructureAsync(string fullPath, bool recursive = true, string format = "yaml", string excludePattern = "")
```
Retrieves the hierarchical folder structure in YAML format:
- **Description**: Retrieves the hierarchical folder structure in YAML format from a specified directory path.
- **fullPath**: Absolute path to the root directory whose folder structure should be retrieved
- **recursive**: Specifies whether to include subdirectories recursively in the folder structure. If set to true, the function will traverse through all nested directories. If false, only the immediate children of the root directory will be included.
- **format**: Output format (yaml or json)
- **excludePattern**: Regex pattern for files/directories to exclude
- **Returns**: Folder structure in YAML or JSON format

### Zip
```csharp
public static async Task<string> ZipAsync(string path, string outputPath = "", string compressionLevel = "Optimal")
```
Compresses the specified directory or file to ZIP format:
- **Description**: Creates a compressed file
- **path**: The path to the directory or file to compress
- **outputPath**: The path for the output ZIP file (if omitted, automatically generated)
- **compressionLevel**: Compression level (Fastest, Optimal, NoCompression). Default is "Optimal"
- **Returns**: JSON-formatted result message

### Unzip
```csharp
public static async Task<string> UnzipAsync(string filePath, string outputPath = "", bool overwrite = false)
```
Extracts a ZIP file:
- **Description**: Extracts a compressed file
- **filePath**: The path to the ZIP file to extract
- **outputPath**: The path for the output directory (if omitted, automatically generated)
- **overwrite**: Whether to overwrite existing files. Default is false.
- **Returns**: JSON-formatted result message

### OpenWithApplication
```csharp
public static string OpenWithApplication(string path, string verb = "open")
```
Opens a file or folder with the default application:
- **Description**: Opens a file or folder with the default application
- **path**: The path to the file or folder
- **verb**: The verb to use (open, edit, print, etc.). Default is "open"
- **Returns**: JSON-formatted result message

### OpenWithSpecificApplication
```csharp
public static string OpenWithSpecificApplication(string filePath, string applicationPath, string arguments = "")
```
Opens a file with a specific application:
- **Description**: Opens a file with a specified program
- **filePath**: The path to the file to open
- **applicationPath**: The path to the application to use
- **arguments**: Additional command line arguments
- **Returns**: JSON-formatted result message

## Usage

### Compilation and Building
- Requires dotnet 8.0 or higher
- Run the following command from the repository root directory:

```bash
dotnet build CSharpMcpServer/FileSystem
```

### Integration with Claude Desktop
To use with Claude Desktop, add the following configuration to your `claude_desktop_config.json`:

```json
{
    "mcpServers": {
        "FileSystem": {
            "command": "dotnet",
            "args": [
                "run",
                "--project",
                "absolute\\path\\to\\CSharpMCPServer\\Servers\\FileSystem",
                "--no-build",
                "--",
                "/path/to/allowed/dir"
            ]
        }
    }
}
```

**Important**: 
- Replace `absolute\\path\\to\\CSharpMCPServer\\FileSystem` with your actual project path
- You can specify additional allowed directories with `/path/to/other/allowed/dir` as needed

## Security

This server restricts access to only the specified directories and their subdirectories. 

## .gitignore Support

GetFolderStructure includes functionality to parse .gitignore files:

- Reads the .gitignore from the root directory
- Properly processes .gitignore files in subdirectories
- Supports absolute and relative path patterns
- Converts wildcards (*, **, ?)
- Handles directory-specific patterns (trailing /)

Additionally, the following common files/directories are automatically excluded:

- .git, .next directories
- Build outputs like bin, obj, target, dist
- Visual Studio files like .vs, *.user, *.suo
- Package directories like node_modules, packages
- Log files, backup files, cache files