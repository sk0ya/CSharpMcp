using System.ComponentModel;
using System.Diagnostics;
using ModelContextProtocol.Server;

namespace Dotnet.MsBuild;

[McpServerToolType]
public class DotnetBuildTools
{
    [McpServerTool, Description("指定されたプロジェクトやソリューションをdotnetコマンドを使用してビルドします")]
    public BuildResult BuildProject(string projectPath, string configuration = "Debug", string framework = "")
    {
        
        if (string.IsNullOrEmpty(projectPath))
        {
            return new BuildResult
            {
                Success = false,
                Output = "プロジェクトパスが指定されていません",
                ErrorOutput = "プロジェクトパスは必須パラメーターです"
            };
        }

        var arguments = $"dotnet build \"{projectPath}\" --configuration {configuration}";

        if (!string.IsNullOrEmpty(framework))
        {
            arguments += $" --framework {framework}";
        }
        arguments += " -v:q";
        var scriptBlock = $@"
        {arguments}
    ";
        
        var processInfo = new ProcessStartInfo
        {
            FileName = "powershell.exe",
            Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{scriptBlock.Replace("\"", "\\\"")}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(processInfo);
        var output = process?.StandardOutput.ReadToEnd()??string.Empty;
        var error = process?.StandardError.ReadToEnd()??string.Empty;
        process?.WaitForExit();
        
        return new BuildResult
        {
            Success = process?.ExitCode == 0,
            Output = output.ToString(),
            ErrorOutput = error.ToString(),
            ExitCode = process.ExitCode
        };
    }
}