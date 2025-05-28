# PowerShell

The CSharpMcpServer PowerShell module provides a secure interface for executing PowerShell commands within the MCP (Model Context Protocol) environment. It allows for controlled execution of a predefined set of PowerShell cmdlets while implementing security measures to prevent unauthorized operations.

## Features

- **GetAvailableCommands**: Retrieves a list of PowerShell commands that are allowed to be executed
  - Returns a JSON-formatted list of permitted commands

- **ExecuteCommand**: Safely executes PowerShell commands with security constraints
  - Validates commands against the allowed command list
  - Runs PowerShell in constrained language mode
  - Handles command parameters through structured JSON input

## Security Measures

The PowerShell module implements several security mechanisms:

- **Command Allowlist**: Only explicitly permitted commands can be executed
- **Constrained Language Mode**: Limits PowerShell to a restricted subset of language features
- **Just Enough Administration (JEA)**: Controls command visibility and execution
- **Parameter Sanitization**: Ensures parameters are properly formatted and escaped

## API Details

### GetAvailableCommands

```csharp
public static string GetAvailableCommands()
```

Retrieves a list of available PowerShell commands:
- **Description**: 利用可能なPowerShellコマンド一覧を取得します
- **Returns**: JSON-formatted string containing the list of allowed commands

### ExecuteCommand

```csharp
public static string ExecuteCommand(string command, string parameters = "{}")
```

Safely executes a PowerShell command:
- **Description**: PowerShellコマンドを安全に実行します
- **command**: The PowerShell command to execute
- **parameters**: JSON-formatted string containing command parameters (default: empty object)
- **Returns**: Command execution results as a string

## Usage

```csharp
// Get list of available commands
string availableCommands = GetAvailableCommands();

// Execute a PowerShell command without parameters
string result = ExecuteCommand("Get-Process");

// Execute a PowerShell command with parameters
string paramResult = ExecuteCommand("Get-Process", "{ \"Name\": \"explorer\" }");
```

## Default Allowed Commands

The module comes with a default set of allowed commands:
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

Additional commands can be configured through the `allowed_commands.json` resource file.

## Command Execution Pipeline

1. Command validation against the allowlist
2. Parameter preparation and formatting
3. Secure PowerShell environment setup
4. Command execution with error handling
5. Result formatting and return

## Custom Configuration

The allowed command list can be customized by modifying the embedded resource `Resources/allowed_commands.json`.