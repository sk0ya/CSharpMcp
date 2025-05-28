using FileSystem.Common;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace FileSystem.Tools;

/// <summary>
/// フォルダ構造の取得と関連機能を提供します
/// </summary>
public static partial class FileSystemTools
{
    /// <summary>
    /// 指定したディレクトリの階層構造をYAML形式で取得します
    /// </summary>
    /// <param name="fullPath">フォルダ構造を取得するディレクトリのパス</param>
    /// <param name="recursive">サブディレクトリも再帰的に取得するかどうか</param>
    /// <param name="format">出力形式（yaml または json）</param>
    /// <param name="excludePattern">除外するファイル/ディレクトリのパターン（正規表現）</param>
    /// <returns>フォルダ構造を表現するYAMLまたはJSON文字列</returns>
    [McpServerTool, Description("Retrieves the hierarchical folder structure in YAML format from a specified directory path.")]
    public static async Task<string> GetFolderStructureAsync(
        [Description("Absolute path to the root directory whose folder structure should be retrieved.")] string fullPath,
        [Description("Specifies whether to include subdirectories recursively in the folder structure. If set to true, the function will traverse through all nested directories. If false, only the immediate children of the root directory will be included.")] bool recursive = true,
        [Description("Output format (yaml or json)")] string format = "yaml",
        [Description("Regex pattern for files/directories to exclude")] string excludePattern = "")
    {
        try
        {
            // セキュリティチェック
            Security.ValidateIsAllowedDirectory(fullPath);
            
            if (!Directory.Exists(fullPath))
            {
                var errorResult = new
                {
                    Status = "Error",
                    Message = $"ディレクトリが存在しません: {fullPath}"
                };
                
                return format.ToLowerInvariant() == "json" 
                    ? JsonSerializer.Serialize(errorResult, new JsonSerializerOptions { WriteIndented = true })
                    : $"Error: ディレクトリが存在しません: {fullPath}";
            }
            
            if (!Security.HasReadPermission(fullPath))
            {
                var errorResult = new
                {
                    Status = "Error",
                    Message = $"読み取り権限がありません: {fullPath}"
                };
                
                return format.ToLowerInvariant() == "json" 
                    ? JsonSerializer.Serialize(errorResult, new JsonSerializerOptions { WriteIndented = true })
                    : $"Error: 読み取り権限がありません: {fullPath}";
            }

            // 除外パターンの準備
            Regex excludeRegex = null;
            if (!string.IsNullOrWhiteSpace(excludePattern))
            {
                try
                {
                    excludeRegex = new Regex(excludePattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
                }
                catch (ArgumentException)
                {
                    var errorResult = new
                    {
                        Status = "Error",
                        Message = $"無効な正規表現パターンです: {excludePattern}"
                    };
                    
                    return format.ToLowerInvariant() == "json" 
                        ? JsonSerializer.Serialize(errorResult, new JsonSerializerOptions { WriteIndented = true })
                        : $"Error: 無効な正規表現パターンです: {excludePattern}";
                }
            }

            // .gitignoreパターンの読み込み
            var ignorePatterns = await Task.Run(() => GitIgnoreParser.LoadIgnorePatterns(fullPath));

            // フォルダ構造の構築
            var result = await Task.Run(() => 
            {
                var sb = new StringBuilder();

                var rootName = Path.GetFileName(fullPath);
                sb.AppendLine($"{rootName}:");

                TraverseDirectoryYaml(fullPath, sb, "  ", ignorePatterns, fullPath, recursive, excludeRegex);

                return sb.ToString();
            });

            // 要求された形式で返す
            if (format.ToLowerInvariant() == "json")
            {
                // YAMLからJSONに変換
                var yamlStructure = result;
                var jsonStructure = ConvertYamlToJson(yamlStructure);
                return jsonStructure;
            }
            
            return result;
        }
        catch (Exception ex)
        {
            return format.ToLowerInvariant() == "json" 
                ? ExceptionHandling.FormatExceptionAsJson(ex, "フォルダ構造取得")
                : $"Error: {ex.Message}";
        }
    }

    #region Private Methods

    /// <summary>
    /// ディレクトリを再帰的に走査してYAML形式でフォルダ構造を構築します
    /// </summary>
    private static void TraverseDirectoryYaml(
        string path,
        StringBuilder sb,
        string indent,
        List<Regex> ignorePatterns,
        string rootPath,
        bool recursive,
        Regex excludeRegex = null)
    {
        // ファイルとディレクトリを取得（フィルタリング済み）
        var (filteredFiles, filteredDirs) = GetFilteredItems(path, ignorePatterns, rootPath, excludeRegex);

        // ファイルを追加
        foreach (var file in filteredFiles)
        {
            sb.AppendLine($"{indent}- {Path.GetFileName(file)}");
        }

        // サブディレクトリを追加
        foreach (var dir in filteredDirs)
        {
            var dirName = Path.GetFileName(dir);
            sb.AppendLine($"{indent}{dirName}:");

            if (!recursive) continue;

            // サブディレクトリの.gitignoreを処理
            var childIgnorePatterns = new List<Regex>(ignorePatterns);
            string gitignorePath = Path.Combine(dir, ".gitignore");
            if (File.Exists(gitignorePath))
            {
                childIgnorePatterns.AddRange(GitIgnoreParser.ParseGitIgnore(gitignorePath, dir, rootPath));
            }

            // 再帰的に処理
            TraverseDirectoryYaml(
                dir,
                sb,
                indent + "  ",
                childIgnorePatterns,
                rootPath,
                recursive,
                excludeRegex
            );
        }
    }

    /// <summary>
    /// 指定したパスのファイルとディレクトリをフィルタリングして取得します
    /// </summary>
    private static (string[] files, string[] dirs) GetFilteredItems(
        string path, 
        List<Regex> ignorePatterns, 
        string rootPath,
        Regex excludeRegex = null)
    {
        // 相対パスを正規化
        string relativePath = GetNormalizedRelativePath(path, rootPath);
        
        // このディレクトリが無視対象なら空を返す
        if (path != rootPath && GitIgnoreParser.IsIgnored(relativePath, ignorePatterns))
        {
            return (Array.Empty<string>(), Array.Empty<string>());
        }

        // 安全に取得
        string[] files;
        string[] directories;
        try
        {
            files = Directory.GetFiles(path);
            directories = Directory.GetDirectories(path);
        }
        catch (UnauthorizedAccessException)
        {
            // アクセス拒否の場合は空を返す
            return (Array.Empty<string>(), Array.Empty<string>());
        }

        // gitignoreでのフィルタリング
        var filteredFiles = files
            .Where(file => !GitIgnoreParser.IsIgnored(GetNormalizedRelativePath(file, rootPath), ignorePatterns))
            .Where(file => excludeRegex == null || !excludeRegex.IsMatch(file))
            .OrderBy(file => Path.GetFileName(file))
            .ToArray();

        var filteredDirs = directories
            .Where(dir => !GitIgnoreParser.IsIgnored(GetNormalizedRelativePath(dir, rootPath), ignorePatterns))
            .Where(dir => excludeRegex == null || !excludeRegex.IsMatch(dir))
            .OrderBy(dir => Path.GetFileName(dir))
            .ToArray();

        return (filteredFiles, filteredDirs);
    }

    /// <summary>
    /// パスを正規化して相対パスにします
    /// </summary>
    private static string GetNormalizedRelativePath(string path, string rootPath)
    {
        return Path.GetRelativePath(rootPath, path).Replace("\\", "/");
    }

    /// <summary>
    /// YAMLをJSON形式に変換します（簡易版）
    /// </summary>
    private static string ConvertYamlToJson(string yaml)
    {
        var jsonObj = new Dictionary<string, object>();
        var currentObj = jsonObj;
        var stack = new Stack<Dictionary<string, object>>();
        var currentIndent = 0;
        
        var lines = yaml.Split('\n');
        foreach (var rawLine in lines)
        {
            var line = rawLine.TrimEnd();
            if (string.IsNullOrWhiteSpace(line)) continue;
            
            // インデントを取得
            int indent = line.TakeWhile(char.IsWhiteSpace).Count();
            string content = line.Trim();
            
            // ルートレベルに戻る処理
            while (indent < currentIndent && stack.Count > 0)
            {
                currentObj = stack.Pop();
                currentIndent -= 2;
            }
            
            // コンテンツを解析
            if (content.EndsWith(':'))
            {
                // ディレクトリ行
                string dirName = content.TrimEnd(':');
                var newObj = new Dictionary<string, object>();
                currentObj[dirName] = newObj;
                
                stack.Push(currentObj);
                currentObj = newObj;
                currentIndent = indent;
            }
            else if (content.StartsWith('-'))
            {
                // ファイル行
                string fileName = content.Substring(1).Trim();
                
                // 配列を取得または作成
                if (!currentObj.ContainsKey("files"))
                {
                    currentObj["files"] = new List<string>();
                }
                
                var filesList = (List<string>)currentObj["files"];
                filesList.Add(fileName);
            }
        }
        
        return JsonSerializer.Serialize(jsonObj, new JsonSerializerOptions
        {
            WriteIndented = true
        });
    }

    #endregion
}