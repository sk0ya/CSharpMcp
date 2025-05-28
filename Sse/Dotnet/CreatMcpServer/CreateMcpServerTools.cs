using CreateMcpServer;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;

namespace CreateMcpServer;

[McpServerToolType]
public class CreateMcpServerTools
{
    [McpServerTool, Description("Create a new MCP Server project")]
    public static string CreateMcpServerProject(string feature)
    {
        var folderPath = Path.Combine(CreateMcpServerPath.RootFolderPath, feature);

        // フォルダが既に存在するかチェック
        if (Directory.Exists(folderPath))
        {
            return $"フォルダ '{folderPath}' は既に存在します。";
        }

        // フォルダを作成
        Directory.CreateDirectory(folderPath);

        // プロジェクトファイルを作成
        try
        {
            CreateConsoleProject(folderPath);
        }
        catch (Exception ex)
        {
            return ex.Message;
        }

        // Program.cs ファイルを作成
        CreateProgramCs(folderPath);

        // ToolsクラスファイルをFeatureNameTools.csという形で作成
        CreateToolsFile(folderPath, feature);

        // ソリューションファイルにプロジェクトを追加
        if (AddProjectToSolution(feature, out var errorMesssage))
        {
            return errorMesssage;
        }

        return $"{feature} プロジェクトの作成が完了しました。";
    }

    private static void CreateConsoleProject(string folderPath)
    {
        var scriptBlock = $@"
        Set-Location '{folderPath.Replace("'", "''")}'
        dotnet new console
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

        using (var process = Process.Start(processInfo))
        {
            var output = process.StandardOutput.ReadToEnd();
            var error = process.StandardError.ReadToEnd();
            process.WaitForExit();
            if (process.ExitCode != 0)
            {
                throw new Exception($"コンソールプロジェクトの作成に失敗しました。{output}{error}");
            }
        }

        AddProjectPackage(folderPath, "Microsoft.Extensions.Hosting");
        AddProjectPackage(folderPath, "ModelContextProtocol", prerelease: true);
    }

    private static void AddProjectPackage(string projectPath, string packageName, bool prerelease = false)
    {
        var prereleaseText = prerelease ? "--prerelease" : "";

        var scriptBlock = $@"
        Set-Location '{projectPath.Replace("'", "''")}'
        dotnet add package {packageName} {prereleaseText}
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

        using (var process = Process.Start(processInfo))
        {
            var output = process.StandardOutput.ReadToEnd();
            var error = process.StandardError.ReadToEnd();
            process.WaitForExit();
            if (process.ExitCode != 0)
            {
                Console.WriteLine(output);
                Console.WriteLine(error);
                Console.WriteLine($"プロジェクト参照 {packageName} の追加に失敗しました。");
            }
        }
    }

    private static void CreateProgramCs(string folderPath)
    {
        string programPath = Path.Combine(folderPath, "Program.cs");
        string programContent = @"using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateEmptyApplicationBuilder(settings: null);
builder.Logging.AddConsole(options =>
{
    options.LogToStandardErrorThreshold = LogLevel.Trace;
});


builder.Services.AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly()
    .WithPromptsFromAssembly();

var app = builder.Build();

await app.RunAsync();
";

        File.WriteAllText(programPath, programContent);
    }

    private static void CreateToolsFile(string folderPath, string feature)
    {
        string toolsFilePath = Path.Combine(folderPath, $"{feature}Tools.cs");
        string toolsFileContent = $@"using ModelContextProtocol.Server;
using System.ComponentModel;
namespace {feature}Tools;

[McpServerToolType]
public static class {feature}Tools
{{
    [McpServerTool, Description("""")]
    public static void {feature}()
    {{
        // ここに実装を追加
    }}
}}
";

        File.WriteAllText(toolsFilePath, toolsFileContent);
    }

    private static bool AddProjectToSolution(string feature, out string errorMessage)
    {
        errorMessage = string.Empty;
        // ルートフォルダにある .sln ファイルを検索
        string rootPath = CreateMcpServerPath.RootFolderPath;
        var parentDir = Directory.GetParent(rootPath);
        var slnFiles = parentDir.GetFiles("*.sln").Select(x => x.FullName);

        if (slnFiles.Count() == 0)
        {
            errorMessage = "ソリューションファイルが見つかりませんでした。";
            return false;
        }

        foreach (string slnFile in slnFiles)
        {
            var scriptBlock = $@"
        Set-Location '{parentDir.FullName.Replace("'", "''")}'
        dotnet sln {slnFile} add {feature}\\{feature}.csproj
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

            using (var process = Process.Start(processInfo))
            {
                process.WaitForExit();
                if (process.ExitCode != 0)
                {
                    errorMessage = $"プロジェクトをソリューション {slnFile} に追加できませんでした。";
                    return false;
                }
            }
        }

        return true;
    }
}