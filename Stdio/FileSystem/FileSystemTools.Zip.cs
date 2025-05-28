using FileSystem.Common;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.IO.Compression;
using System.Text.Json;

namespace FileSystem.Tools;

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
            
            // 圧縮レベルの解析
            CompressionLevel level = ParseCompressionLevel(compressionLevel);
            
            // 出力パスの決定
            string zipFilePath = string.IsNullOrWhiteSpace(outputPath)
                ? Path.Combine(Path.GetDirectoryName(path), Path.GetFileName(path) + ".zip")
                : outputPath;
            
            // 出力パスのセキュリティチェック
            Security.ValidateIsAllowedDirectory(zipFilePath);
            
            if (!Security.HasWritePermission(zipFilePath))
            {
                return JsonSerializer.Serialize(new
                {
                    Status = "Error",
                    Message = $"出力先への書き込み権限がありません: {zipFilePath}"
                });
            }

            // 既存のZIPファイルを削除
            if (File.Exists(zipFilePath))
            {
                File.Delete(zipFilePath);
            }

            if (Directory.Exists(path))
            {
                // ディレクトリ圧縮
                await Task.Run(() => 
                {
                    ZipFile.CreateFromDirectory(path, zipFilePath, level, false);
                });
            }
            else
            {
                // 単一ファイル圧縮
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
            // セキュリティチェック
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

            // 展開先ディレクトリの決定
            string extractDir = string.IsNullOrWhiteSpace(outputPath)
                ? Path.Combine(Path.GetDirectoryName(filePath), Path.GetFileNameWithoutExtension(filePath))
                : outputPath;
            
            // 出力パスのセキュリティチェック
            Security.ValidateIsAllowedDirectory(extractDir);
            
            if (!Security.HasWritePermission(extractDir))
            {
                return JsonSerializer.Serialize(new
                {
                    Status = "Error",
                    Message = $"展開先への書き込み権限がありません: {extractDir}"
                });
            }

            // 展開先ディレクトリの処理
            if (Directory.Exists(extractDir))
            {
                if (overwrite)
                {
                    // 既存ディレクトリを削除
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

            // ZIPファイルの展開
            await Task.Run(() =>
            {
                using (ZipArchive archive = ZipFile.OpenRead(filePath))
                {
                    // エントリごとに処理
                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        string entryFullPath = Path.Combine(extractDir, entry.FullName);
                        string entryDir = Path.GetDirectoryName(entryFullPath);

                        // サブディレクトリの作成
                        if (!Directory.Exists(entryDir) && !string.IsNullOrEmpty(entryDir))
                        {
                            Directory.CreateDirectory(entryDir);
                        }

                        // ディレクトリのみの場合はスキップ
                        if (string.IsNullOrEmpty(entry.Name))
                        {
                            continue;
                        }

                        // ファイルの展開
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
