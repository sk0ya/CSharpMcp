using FileSystem.Common;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Diagnostics;
using System.Text.Json;

namespace FileSystem.Tools;

public static partial class FileSystemTools
{
    [McpServerTool, Description("ファイルまたはフォルダを規定のアプリケーションで開く")]
    public static string OpenWithApplication(
        [Description("ファイルまたはフォルダのパス")] string path,
        [Description("使用する動詞（open, edit, print など）")] string verb = "open")
    {
        try
        {
            // セキュリティチェック
            Security.ValidateIsAllowedDirectory(path);
            
            if (!File.Exists(path) && !Directory.Exists(path))
            {
                return JsonSerializer.Serialize(new
                {
                    Status = "Error",
                    Message = $"指定されたパスが見つかりません: {path}"
                });
            }
            
            // 実行可能ファイルの場合はセキュリティ確認
            if (File.Exists(path) && !Security.IsNonExecutableFile(path))
            {
                return JsonSerializer.Serialize(new
                {
                    Status = "Error",
                    Message = $"セキュリティ上の理由により、実行可能ファイルは直接開けません: {path}"
                });
            }

            // ProcessStartInfoの設定
            var processStartInfo = new ProcessStartInfo
            {
                FileName = path,
                UseShellExecute = true,
                Verb = verb
            };
            
            // プロセス開始
            Process.Start(processStartInfo);
            
            return JsonSerializer.Serialize(new
            {
                Status = "Success",
                Path = path,
                Verb = verb,
                Message = $"'{path}' を規定のアプリケーションで開きました。"
            });
        }
        catch (Win32Exception ex) when (ex.NativeErrorCode == 1155) // ERROR_NO_ASSOCIATION
        {
            return JsonSerializer.Serialize(new
            {
                Status = "Error",
                ErrorCode = ex.NativeErrorCode,
                Message = $"このファイル形式に関連付けられたアプリケーションがありません: {path}"
            });
        }
        catch (Exception ex)
        {
            return ExceptionHandling.FormatExceptionAsJson(ex, "アプリケーションで開く");
        }
    }
    
    //[McpServerTool, Description("ファイルに関連付けられたアプリケーション情報を取得します")]
    public static string GetFileAssociation(
        [Description("ファイルパス")] string path)
    {
        try
        {
            // セキュリティチェック
            Security.ValidateIsAllowedDirectory(path);
            
            if (!File.Exists(path))
            {
                return JsonSerializer.Serialize(new
                {
                    Status = "Error",
                    Message = $"指定されたファイルが見つかりません: {path}"
                });
            }

            // ファイル拡張子を取得
            string extension = Path.GetExtension(path).ToLowerInvariant();
            if (string.IsNullOrEmpty(extension))
            {
                return JsonSerializer.Serialize(new
                {
                    Status = "Warning",
                    Path = path,
                    Message = "このファイルには拡張子がないため、関連付け情報を取得できません。"
                });
            }

            // 拡張子の関連付け情報を取得
            // Windows以外のプラットフォームでは限定的な情報のみ
            string fileType = "不明";
            string applicationPath = "不明";
            
            try
            {
                if (OperatingSystem.IsWindows())
                {
                    // Windowsの場合はレジストリ情報も取得できる
                    using (var process = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = "cmd.exe",
                            Arguments = $"/c assoc {extension}",
                            UseShellExecute = false,
                            RedirectStandardOutput = true,
                            CreateNoWindow = true
                        }
                    })
                    {
                        process.Start();
                        string output = process.StandardOutput.ReadToEnd();
                        process.WaitForExit();
                        
                        if (!string.IsNullOrEmpty(output) && output.Contains("="))
                        {
                            fileType = output.Split('=')[1].Trim();
                        }
                    }
                }
                else
                {
                    // 非Windowsプラットフォームの場合は簡易判定
                    fileType = extension.TrimStart('.');
                }
            }
            catch
            {
                // 失敗した場合はデフォルトの値を使用
            }

            return JsonSerializer.Serialize(new
            {
                Status = "Success",
                Path = path,
                FileName = Path.GetFileName(path),
                Extension = extension,
                FileType = fileType,
                DefaultApplication = applicationPath,
                Message = $"ファイル '{path}' の関連付け情報を取得しました。"
            });
        }
        catch (Exception ex)
        {
            return ExceptionHandling.FormatExceptionAsJson(ex, "関連付け情報取得");
        }
    }
    
    //[McpServerTool, Description("指定したプログラムでファイルを開きます")]
    public static string OpenWithSpecificApplication(
        [Description("開くファイルのパス")] string filePath,
        [Description("使用するアプリケーションのパス")] string applicationPath,
        [Description("追加のコマンドライン引数")] string arguments = "")
    {
        try
        {
            // セキュリティチェック
            Security.ValidateIsAllowedDirectory(filePath);
            Security.ValidateIsAllowedDirectory(applicationPath);
            
            if (!File.Exists(filePath))
            {
                return JsonSerializer.Serialize(new
                {
                    Status = "Error",
                    Message = $"指定されたファイルが見つかりません: {filePath}"
                });
            }
            
            if (!File.Exists(applicationPath))
            {
                return JsonSerializer.Serialize(new
                {
                    Status = "Error",
                    Message = $"指定されたアプリケーションが見つかりません: {applicationPath}"
                });
            }

            // アプリケーションが実行可能ファイルかチェック
            if (!applicationPath.EndsWith(".exe") && !applicationPath.EndsWith(".com") && 
                !applicationPath.EndsWith(".bat") && !applicationPath.EndsWith(".cmd"))
            {
                if (OperatingSystem.IsWindows())
                {
                    return JsonSerializer.Serialize(new
                    {
                        Status = "Error",
                        Message = $"指定されたファイルは実行可能ファイルではありません: {applicationPath}"
                    });
                }
            }

            // 引数の構築
            string args = string.IsNullOrWhiteSpace(arguments)
                ? $"\"{filePath}\""
                : $"{arguments} \"{filePath}\"";

            // ProcessStartInfoの設定
            var processStartInfo = new ProcessStartInfo
            {
                FileName = applicationPath,
                Arguments = args,
                UseShellExecute = true
            };
            
            // プロセス開始
            Process.Start(processStartInfo);
            
            return JsonSerializer.Serialize(new
            {
                Status = "Success",
                FilePath = filePath,
                ApplicationPath = applicationPath,
                Arguments = args,
                Message = $"'{applicationPath}' で '{filePath}' を開きました。"
            });
        }
        catch (Exception ex)
        {
            return ExceptionHandling.FormatExceptionAsJson(ex, "指定アプリケーションで開く");
        }
    }
}
