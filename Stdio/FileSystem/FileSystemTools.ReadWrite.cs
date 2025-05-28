using System.ComponentModel;
using System.Text;
using System.Text.Json;
using FileSystem.Common;
using ModelContextProtocol.Server;

namespace FileSystem;

[McpServerToolType]
public static partial class FileSystemTools
{
    [McpServerTool, Description("Write file - Use this method to create a new file or completely overwrite the contents of an existing file. For replacing specific text within a file, use EditFile instead.")]
    public static FileOpelationResult WriteFile(
        [Description("The path to the file to edit")] string filePath,
        [Description("The text to replace it with")] string content,
        [Description("The encoding to use (utf-8, shift-jis, etc.). Default is utf-8.")] string encodingName = "utf-8")
    {
        Security.ValidateIsAllowedDirectory(filePath);

        string directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var normalizedContent = NormalizeLineEndings(content);
        var encoding = ResolveEncoding(encodingName);
        File.WriteAllText(filePath, normalizedContent, encoding);

        return FileOpelationResult.Success(filePath);
    }

    [McpServerTool, Description("Edit file by replacing specified text - Use this method to replace specific text within an existing file. For completely overwriting file contents, use WriteFile instead.")]
    public static FileOpelationResult EditFile(
        [Description("The path to the file to edit")] string filePath,
        [Description("The text to replace")] string oldString,
        [Description("The text to replace it with")] string newString,
                [Description("The encoding to use (utf-8, shift-jis, etc.). Default is utf-8.")] string encodingName = "utf-8",
        [Description("The expected number of replacements to perform. Defaults to 1 if not specified.")] int replacementCount = 1)
    {
        Security.ValidateIsAllowedDirectory(filePath);

        string content;
        var encoding = ResolveEncoding(encodingName);

        byte[] fileBytes = File.ReadAllBytes(filePath);
        content = encoding.GetString(fileBytes);

        if (!string.IsNullOrEmpty(oldString))
        {
            string normalizedContent = NormalizeLineEndings(content);
            string normalizedOldString = NormalizeLineEndings(oldString);
            string normalizedNewString = NormalizeLineEndings(newString);

            int actualReplacements = 0;
            if (replacementCount == 1)
            {
                int index = normalizedContent.IndexOf(normalizedOldString);
                if (index >= 0)
                {
                    normalizedContent = normalizedContent.Substring(0, index) + normalizedNewString + normalizedContent.Substring(index + normalizedOldString.Length);
                    actualReplacements = 1;
                    content = normalizedContent;
                }
            }
            else
            {
                int occurrencesBeforeReplace = CountOccurrences(normalizedContent, normalizedOldString);

                string newContent = normalizedContent.Replace(normalizedOldString, normalizedNewString);

                if (normalizedOldString.Length == normalizedNewString.Length)
                {
                    int occurrencesAfterReplace = CountOccurrences(newContent, normalizedNewString);
                    actualReplacements = occurrencesBeforeReplace - (occurrencesAfterReplace - occurrencesBeforeReplace);
                }
                else
                {
                    actualReplacements = occurrencesBeforeReplace;
                }

                if (replacementCount > 0 && actualReplacements != replacementCount)
                {
                    return FileOpelationResult.Failed(
                        filePath,
                        $"予期された置換回数({replacementCount})と実際の置換回数({actualReplacements})が一致しません");
                }

                content = newContent;
            }

            if (actualReplacements == 0)
            {
                return FileOpelationResult.Failed(
                    filePath,
                     $"置換対象の文字列が見つかりませんでした");
            }
        }
        else
        {
            content = newString;
        }

        byte[] outputBytes = encoding.GetBytes(content);
        File.WriteAllBytes(filePath, outputBytes);

        return FileOpelationResult.Success(filePath);
    }

    public static async Task<List<Dictionary<string, string>>> GetMultipleFilesInfoAsync([Description("The full paths to the files to be read.")] string[]? filePaths,
        [Description("The encoding to use (utf-8, shift-jis, etc.). Default is utf-8.")] string encodingName = "utf-8")
    {
        Encoding encoding = ResolveEncoding(encodingName);
        var results = new List<Dictionary<string, string>>();

        foreach (string filePath in filePaths)
        {
            Security.ValidateIsAllowedDirectory(filePath);

            var resultDict = new Dictionary<string, string>
            {
                ["filePath"] = filePath,
                ["fileName"] = Path.GetFileName(filePath)
            };

            string content = await File.ReadAllTextAsync(filePath, encoding);
            resultDict["lineCount"] = content.Split(new[] { "\\r\\n", "\\r", "\\n" }, StringSplitOptions.None).Length.ToString();
            resultDict["content"] = content;

            results.Add(resultDict);
        }

        return results;
    }

    [McpServerTool, Description("Gets basic file information.")]
    public static async Task<Dictionary<string, string>> GetFileInfo([Description("The full paths to the file to be read.")] string filePath,
        [Description("The encoding to use (utf-8, shift-jis, etc.). Default is utf-8.")] string encodingName = "utf-8")
    {
        Encoding encoding = ResolveEncoding(encodingName);

        Security.ValidateIsAllowedDirectory(filePath);

        var resultDict = new Dictionary<string, string>
        {
            ["filePath"] = filePath,
            ["fileName"] = Path.GetFileName(filePath)
        };

        string content = await File.ReadAllTextAsync(filePath, encoding);
        resultDict["lineCount"] = content.Split(new[] { "\\r\\n", "\\r", "\\n" }, StringSplitOptions.None).Length.ToString();
        resultDict["content"] = content;

        return resultDict;
    }

    [McpServerTool, Description("Deletes a file or directory from the file system.")]
    public static string Delete(
        [Description("The full path of the file or directory to delete.")] string fullPath,
        [Description("Whether to delete all contents inside a directory. Ignored for files. Default is false.")] bool recursive = false)
    {
        try
        {
            Security.ValidateIsAllowedDirectory(fullPath);

            bool isFile = File.Exists(fullPath);
            bool isDirectory = Directory.Exists(fullPath);

            if (!isFile && !isDirectory)
            {
                return JsonSerializer.Serialize(new
                {
                    Status = "Error",
                    Message = $"指定されたパスが見つかりません: {fullPath}"
                });
            }

            if (!Security.HasWritePermission(fullPath))
            {
                return JsonSerializer.Serialize(new
                {
                    Status = "Error",
                    Message = $"削除権限がありません: {fullPath}"
                });
            }

            if (isFile)
            {
                File.Delete(fullPath);
                return JsonSerializer.Serialize(new
                {
                    Status = "Success",
                    Message = $"ファイルを削除しました: {fullPath}"
                });
            }
            else
            {
                Directory.Delete(fullPath, recursive);
                return JsonSerializer.Serialize(new
                {
                    Status = "Success",
                    Message = $"ディレクトリを削除しました: {fullPath}",
                    Recursive = recursive
                });
            }
        }
        catch (IOException ex) when (ex.Message.Contains("directory is not empty"))
        {
            return JsonSerializer.Serialize(new
            {
                Status = "Error",
                Message = $"ディレクトリが空ではありません。recursive=true を指定してください: {fullPath}"
            });
        }
        catch (Exception ex)
        {
            return ExceptionHandling.FormatExceptionAsJson(ex, "削除");
        }
    }

    [McpServerTool, Description("Moves a file or directory to a new location.")]
    public static string Move(
        [Description("The path of the file or directory to move.")] string sourcePath,
        [Description("The path to move the file or directory to.")] string destinationPath,
        [Description("Whether to overwrite an existing file. Ignored for directories. Default is false.")] bool overwrite = false)
    {
        try
        {
            Security.ValidateIsAllowedDirectory(sourcePath);
            Security.ValidateIsAllowedDirectory(destinationPath);

            bool isFile = File.Exists(sourcePath);
            bool isDirectory = Directory.Exists(sourcePath);

            if (!isFile && !isDirectory)
            {
                return JsonSerializer.Serialize(new
                {
                    Status = "Error",
                    Message = $"移動元が見つかりません: {sourcePath}"
                });
            }

            if (!Security.HasReadPermission(sourcePath) || !Security.HasWritePermission(sourcePath))
            {
                return JsonSerializer.Serialize(new
                {
                    Status = "Error",
                    Message = $"移動元のアクセス権限がありません: {sourcePath}"
                });
            }

            if (!Security.HasWritePermission(destinationPath))
            {
                return JsonSerializer.Serialize(new
                {
                    Status = "Error",
                    Message = $"移動先への書き込み権限がありません: {destinationPath}"
                });
            }

            if (isFile)
            {
                string destDir = Path.GetDirectoryName(destinationPath);
                if (!string.IsNullOrEmpty(destDir) && !Directory.Exists(destDir))
                {
                    Directory.CreateDirectory(destDir);
                }

                File.Move(sourcePath, destinationPath, overwrite);
                return JsonSerializer.Serialize(new
                {
                    Status = "Success",
                    Message = $"ファイルを移動しました: {sourcePath} -> {destinationPath}"
                });
            }
            else
            {
                Directory.Move(sourcePath, destinationPath);
                return JsonSerializer.Serialize(new
                {
                    Status = "Success",
                    Message = $"ディレクトリを移動しました: {sourcePath} -> {destinationPath}"
                });
            }
        }
        catch (Exception ex)
        {
            return ExceptionHandling.FormatExceptionAsJson(ex, "移動");
        }
    }

    [McpServerTool, Description("Creates a directory.")]
    public static string CreateDirectory(
        [Description("The path of the directory to create.")] string directoryPath)
    {
        try
        {
            Security.ValidateIsAllowedDirectory(directoryPath);

            if (Directory.Exists(directoryPath))
            {
                return JsonSerializer.Serialize(new
                {
                    Status = "Success",
                    DirectoryPath = directoryPath,
                    Message = $"ディレクトリは既に存在します: {directoryPath}"
                });
            }

            if (!Security.HasWritePermission(directoryPath))
            {
                return JsonSerializer.Serialize(new
                {
                    Status = "Error",
                    Message = $"書き込み権限がありません: {directoryPath}"
                });
            }

            Directory.CreateDirectory(directoryPath);

            return JsonSerializer.Serialize(new
            {
                Status = "Success",
                DirectoryPath = directoryPath,
                Message = $"ディレクトリを作成しました: {directoryPath}"
            });
        }
        catch (Exception ex)
        {
            return ExceptionHandling.FormatExceptionAsJson(ex, "ディレクトリ作成");
        }
    }

    #region Private Methods

    private static string NormalizeLineEndings(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        text = text.Replace("\\r\\n", "\\n").Replace("\\r", "\\n");
        
        return text.Replace("\\n", Environment.NewLine);
    }

    private static Encoding ResolveEncoding(string encodingName)
    {
        if (string.IsNullOrWhiteSpace(encodingName))
        {
            return Constants.DefaultEncoding;
        }

        try
        {
            return Encoding.GetEncoding(encodingName);
        }
        catch (ArgumentException)
        {
            return Constants.DefaultEncoding;
        }
    }

    private static int CountOccurrences(string text, string pattern)
    {
        if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(pattern))
            return 0;

        int count = 0;
        int index = 0;
        while ((index = text.IndexOf(pattern, index)) >= 0)
        {
            count++;
            index += pattern.Length;
        }
        return count;
    }

    #endregion
}

public class FileOpelationResult
{
    public string FilePath { get; }
    public string Message { get; }
    public DateTime Timestamp { get; }

    private FileOpelationResult(string filePath, string message)
    {
        FilePath = filePath;
        Message = message;
        Timestamp = DateTime.Now;
    }

    public static FileOpelationResult Failed(string filePath,string message)
    {
        return new FileOpelationResult(filePath, message);
    }
    public static FileOpelationResult Success(string filePath)
    {
        return new FileOpelationResult(filePath, "Success");
    }

}
