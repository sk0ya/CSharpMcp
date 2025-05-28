using System.ComponentModel;
using System.Diagnostics;
using System.Text;
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

        var arguments = $"build \"{projectPath}\" --configuration {configuration}";

        if (!string.IsNullOrEmpty(framework))
        {
            arguments += $" --framework {framework}";
        }

        arguments += " -v:q";

        var processStartInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        var process = new Process
        {
            StartInfo = processStartInfo
        };

        var outputBuilder = new StringBuilder();
        var errorBuilder = new StringBuilder();

        process.OutputDataReceived += (sender, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                outputBuilder.AppendLine(e.Data);
            }
        };

        process.ErrorDataReceived += (sender, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                errorBuilder.AppendLine(e.Data);
            }
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        process.WaitForExit();

        return new BuildResult
        {
            Success = process.ExitCode == 0,
            Output = outputBuilder.ToString(),
            ErrorOutput = errorBuilder.ToString(),
            ExitCode = process.ExitCode
        };
    }
}