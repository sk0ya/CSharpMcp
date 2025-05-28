using System.Security;
using System.Text.RegularExpressions;

namespace FileSystem;

public static class Security
{
    // 危険とみなすファイル拡張子のパターン
    private static readonly Regex DangerousFileExtensions = new Regex(@"\.(exe|dll|bat|cmd|sh|ps1|vbs|js)$", RegexOptions.IgnoreCase);
    
    // 安全でないパスパターン
    private static readonly Regex UnsafePathPattern = new Regex(@"\.\.[\\/]|[\\/]\.\.[\\/]");
    
    // システムディレクトリパターン
    private static readonly Regex SystemDirectoryPattern = new Regex(@"^[A-Za-z]:\\(Windows|Program Files|Program Files \(x86\)|System|System32)", RegexOptions.IgnoreCase);

    public static void ValidateIsAllowedDirectory(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("パスが空または null です。", nameof(path));
        }

        // 相対パスを絶対パスに変換
        var normalizedTargetPath = Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;

        // システムディレクトリへのアクセスを拒否
        if (SystemDirectoryPattern.IsMatch(normalizedTargetPath))
        {
            throw new UnauthorizedAccessException($"システムディレクトリ '{path}' へのアクセスは許可されていません。");
        }

        // パスインジェクション攻撃のチェック
        if (UnsafePathPattern.IsMatch(path))
        {
            throw new UnauthorizedAccessException("ディレクトリトラバーサルパターンは許可されていません。");
        }

        var args = Environment.GetCommandLineArgs();
        bool isAllowed = false;

        foreach (var dir in args)
        {
            if (string.IsNullOrWhiteSpace(dir))
                continue;

            try
            {
                var normalizedDir = Path.GetFullPath(dir).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
                
                if (normalizedTargetPath.StartsWith(normalizedDir, StringComparison.OrdinalIgnoreCase))
                {
                    isAllowed = true;
                    break;
                }
            }
            catch
            {
                continue;
            }
        }

        if (!isAllowed)
        {
            throw new UnauthorizedAccessException($"パス '{path}' は許可されたディレクトリ内にありません。");
        }
    }

    public static bool IsNonExecutableFile(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            return false;

        return !DangerousFileExtensions.IsMatch(filePath);
    }

    public static bool HasReadPermission(string path)
    {
        try
        {
            if (Directory.Exists(path))
            {
                Directory.GetFiles(path, "*", SearchOption.TopDirectoryOnly);
                return true;
            }
            else if (File.Exists(path))
            {
                using var fs = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                return true;
            }
            
            return false;
        }
        catch (UnauthorizedAccessException)
        {
            return false;
        }
        catch (SecurityException)
        {
            return false;
        }
        catch
        {
            return false;
        }
    }

    public static bool HasWritePermission(string path)
    {
        try
        {
            if (Directory.Exists(path))
            {
                string testFile = Path.Combine(path, $"write_test_{Guid.NewGuid()}.tmp");
                using (File.Create(testFile)) { }
                File.Delete(testFile);
                return true;
            }
            else if (File.Exists(path))
            {
                using var fs = File.Open(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                return true;
            }
            else
            {
                string parentDir = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(parentDir) && Directory.Exists(parentDir))
                {
                    return HasWritePermission(parentDir);
                }
            }
            
            return false;
        }
        catch (UnauthorizedAccessException)
        {
            return false;
        }
        catch (SecurityException)
        {
            return false;
        }
        catch
        {
            return false;
        }
    }
}
