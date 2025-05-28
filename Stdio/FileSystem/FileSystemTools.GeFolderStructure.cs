using System.ComponentModel;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using FileSystem.Common;
using ModelContextProtocol.Server;

namespace FileSystem;

public static partial class FileSystemTools
{
    [McpServerTool, Description("Retrieves the hierarchical folder structure in YAML format from a specified directory path.")]
    public static async Task<string> GetFolderStructureAsync(
        [Description("Absolute path to the root directory whose folder structure should be retrieved.")] string fullPath,
        [Description("Specifies whether to include subdirectories recursively in the folder structure. If set to true, the function will traverse through all nested directories. If false, only the immediate children of the root directory will be included.")] bool recursive = true,
        [Description("Output format (yaml or json)")] string format = "yaml",
        [Description("Regex pattern for files/directories to exclude")] string excludePattern = "")
    {
        try
        {
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

            var ignorePatterns = await Task.Run(() => GitIgnoreParser.LoadIgnorePatterns(fullPath));

            var result = await Task.Run(() => 
            {
                var sb = new StringBuilder();

                var rootName = Path.GetFileName(fullPath);
                sb.AppendLine($"{rootName}:");

                TraverseDirectoryYaml(fullPath, sb, "  ", ignorePatterns, fullPath, recursive, excludeRegex);

                return sb.ToString();
            });

            if (format.ToLowerInvariant() == "json")
            {
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

    private static void TraverseDirectoryYaml(
        string path,
        StringBuilder sb,
        string indent,
        List<Regex> ignorePatterns,
        string rootPath,
        bool recursive,
        Regex excludeRegex = null)
    {
        var (filteredFiles, filteredDirs) = GetFilteredItems(path, ignorePatterns, rootPath, excludeRegex);

        foreach (var file in filteredFiles)
        {
            sb.AppendLine($"{indent}- {Path.GetFileName(file)}");
        }

        foreach (var dir in filteredDirs)
        {
            var dirName = Path.GetFileName(dir);
            sb.AppendLine($"{indent}{dirName}:");

            if (!recursive) continue;

            var childIgnorePatterns = new List<Regex>(ignorePatterns);
            string gitignorePath = Path.Combine(dir, ".gitignore");
            if (File.Exists(gitignorePath))
            {
                childIgnorePatterns.AddRange(GitIgnoreParser.ParseGitIgnore(gitignorePath, dir, rootPath));
            }

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

    private static (string[] files, string[] dirs) GetFilteredItems(
        string path, 
        List<Regex> ignorePatterns, 
        string rootPath,
        Regex excludeRegex = null)
    {
        string relativePath = GetNormalizedRelativePath(path, rootPath);
        
        if (path != rootPath && GitIgnoreParser.IsIgnored(relativePath, ignorePatterns))
        {
            return (Array.Empty<string>(), Array.Empty<string>());
        }

        string[] files;
        string[] directories;
        try
        {
            files = Directory.GetFiles(path);
            directories = Directory.GetDirectories(path);
        }
        catch (UnauthorizedAccessException)
        {
            return (Array.Empty<string>(), Array.Empty<string>());
        }

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

    private static string GetNormalizedRelativePath(string path, string rootPath)
    {
        return Path.GetRelativePath(rootPath, path).Replace("\\", "/");
    }

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
            
            int indent = line.TakeWhile(char.IsWhiteSpace).Count();
            string content = line.Trim();
            
            while (indent < currentIndent && stack.Count > 0)
            {
                currentObj = stack.Pop();
                currentIndent -= 2;
            }
            
            if (content.EndsWith(':'))
            {
                string dirName = content.TrimEnd(':');
                var newObj = new Dictionary<string, object>();
                currentObj[dirName] = newObj;
                
                stack.Push(currentObj);
                currentObj = newObj;
                currentIndent = indent;
            }
            else if (content.StartsWith('-'))
            {
                string fileName = content.Substring(1).Trim();
                
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
