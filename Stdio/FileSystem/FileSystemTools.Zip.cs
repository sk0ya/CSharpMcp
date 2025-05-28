using System.ComponentModel;
using System.IO.Compression;
using System.Text.Json;
using FileSystem.Common;
using ModelContextProtocol.Server;

namespace FileSystem;

public static partial class FileSystemTools
{
    [McpServerTool, Description("圧縮ファイルを作成します")]
    public static async Task<string> ZipAsync(
        [Description("圧縮するディレクトリまたはファイルのパス")] string path,
        [Description("出力するZIPファイルのパス（省略時は自動生成）")] string outputPath = "",
        [Description("圧縮レベル（Fastest, Optimal, NoCompression）")] string compressionLevel = "Optimal")
    {
        try
        {
            Security.ValidateIsAllowedDirectory(path);
            
            if (!File.Exists(path) && !Directory.Exists(path))
            {
                return JsonSerializer.Serialize(new
                {
                    Status = "Error",
                    Message = $"指定されたパスが見つかりません: {path}"
                });
            }
            
            CompressionLevel level = ParseCompressionLevel(compressionLevel);
            
            string zipFilePath = string.IsNullOrWhiteSpace(outputPath)
                ? Path.Combine(Path.GetDirectoryName(path), Path.GetFileName(path) + ".zip")
                : outputPath;
            
            Security.ValidateIsAllowedDirectory(zipFilePath);
            
            if (!Security.HasWritePermission(zipFilePath))
            {
                return JsonSerializer.Serialize(new
                {
                    Status = "Error",
                    Message = $"出力先への書き込み権限がありません: {zipFilePath}"
                });
            }

            if (File.Exists(zipFilePath))
            {
                File.Delete(zipFilePath);
            }

            if (Directory.Exists(path))
            {
                await Task.Run(() => 
                {
                    ZipFile.CreateFromDirectory(path, zipFilePath, level, false);
                });
            }
            else
            {
                using (FileStream zipToCreate = new FileStream(zipFilePath, FileMode.Create))
                {
                    using (ZipArchive archive = new ZipArchive(zipToCreate, ZipArchiveMode.Create))
                    {
                        string fileName = Path.GetFileName(path);
                        ZipArchiveEntry entry = archive.CreateEntry(fileName, level);

                        using (Stream entryStream = entry.Open())
                        using (FileStream fileToCompress = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                        {
                            await fileToCompress.CopyToAsync(entryStream);
                        }
                    }
                }
            }

            return JsonSerializer.Serialize(new
            {
                Status = "Success",
                SourcePath = path,
                ZipFilePath = zipFilePath,
                CompressionLevel = compressionLevel,
                Message = $"圧縮が完了しました: {zipFilePath}"
            });
        }
        catch (Exception ex)
        {
            return ExceptionHandling.FormatExceptionAsJson(ex, "ZIP圧縮");
        }
    }

    [McpServerTool, Description("圧縮ファイルを展開します")]
    public static async Task<string> UnzipAsync(
        [Description("展開するZIPファイルのパス")] string filePath,
        [Description("展開先ディレクトリのパス（省略時は自動生成）")] string outputPath = "",
        [Description("既存ファイルを上書きするかどうか")] bool overwrite = false)
    {
        try
        {
            Security.ValidateIsAllowedDirectory(filePath);
            
            if (!File.Exists(filePath))
            {
                return JsonSerializer.Serialize(new
                {
                    Status = "Error",
                    Message = $"指定されたZIPファイルが見つかりません: {filePath}"
                });
            }

            if (!Path.GetExtension(filePath).Equals(".zip", StringComparison.OrdinalIgnoreCase))
            {
                return JsonSerializer.Serialize(new
                {
                    Status = "Error",
                    Message = "ZIPファイルではありません。"
                });
            }

            string extractDir = string.IsNullOrWhiteSpace(outputPath)
                ? Path.Combine(Path.GetDirectoryName(filePath), Path.GetFileNameWithoutExtension(filePath))
                : outputPath;
            
            Security.ValidateIsAllowedDirectory(extractDir);
            
            if (!Security.HasWritePermission(extractDir))
            {
                return JsonSerializer.Serialize(new
                {
                    Status = "Error",
                    Message = $"展開先への書き込み権限がありません: {extractDir}"
                });
            }

            if (Directory.Exists(extractDir))
            {
                if (overwrite)
                {
                    Directory.Delete(extractDir, true);
                    Directory.CreateDirectory(extractDir);
                }
                else
                {
                    return JsonSerializer.Serialize(new
                    {
                        Status = "Error",
                        Message = $"展開先ディレクトリは既に存在します。上書きする場合は overwrite=true を指定してください: {extractDir}"
                    });
                }
            }
            else
            {
                Directory.CreateDirectory(extractDir);
            }

            await Task.Run(() =>
            {
                using (ZipArchive archive = ZipFile.OpenRead(filePath))
                {
                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        string entryFullPath = Path.Combine(extractDir, entry.FullName);
                        string entryDir = Path.GetDirectoryName(entryFullPath);

                        if (!Directory.Exists(entryDir) && !string.IsNullOrEmpty(entryDir))
                        {
                            Directory.CreateDirectory(entryDir);
                        }

                        if (string.IsNullOrEmpty(entry.Name))
                        {
                            continue;
                        }

                        entry.ExtractToFile(entryFullPath, overwrite);
                    }
                }
            });

            return JsonSerializer.Serialize(new
            {
                Status = "Success",
                ZipFilePath = filePath,
                ExtractDirectory = extractDir,
                Message = $"展開が完了しました: {extractDir}"
            });
        }
        catch (Exception ex)
        {
            return ExceptionHandling.FormatExceptionAsJson(ex, "ZIP展開");
        }
    }

    private static CompressionLevel ParseCompressionLevel(string levelString)
    {
        if (string.IsNullOrWhiteSpace(levelString))
            return CompressionLevel.Optimal;

        switch (levelString.ToLowerInvariant())
        {
            case "fastest":
                return CompressionLevel.Fastest;
            case "nocompression":
                return CompressionLevel.NoCompression;
            case "optimal":
            default:
                return CompressionLevel.Optimal;
        }
    }
}
