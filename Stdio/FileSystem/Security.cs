using System.Security;
using System.Text.RegularExpressions;

namespace FileSystem;

/// <summary>
/// ファイルシステム操作のセキュリティチェックを提供するクラス
/// </summary>
public static class Security
{
    // 危険とみなすファイル拡張子のパターン
    private static readonly Regex DangerousFileExtensions = new Regex(@"\.(exe|dll|bat|cmd|sh|ps1|vbs|js)$", RegexOptions.IgnoreCase);
    
    // 安全でないパスパターン
    private static readonly Regex UnsafePathPattern = new Regex(@"\.\.[\\/]|[\\/]\.\.[\\/]");
    
    // システムディレクトリパターン
    private static readonly Regex SystemDirectoryPattern = new Regex(@"^[A-Za-z]:\\(Windows|Program Files|Program Files \(x86\)|System|System32)", RegexOptions.IgnoreCase);

    /// <summary>
    /// パスがアクセス許可されたディレクトリ内にあるかを検証します
    /// </summary>
    /// <param name="path">検証するパス</param>
    /// <exception cref="UnauthorizedAccessException">パスがアクセス許可されていない場合</exception>
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

        // コマンドライン引数で指定されたディレクトリに制限
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
                // 無効なパスは無視
                continue;
            }
        }

        if (!isAllowed)
        {
            throw new UnauthorizedAccessException($"パス '{path}' は許可されたディレクトリ内にありません。");
        }
    }

    /// <summary>
    /// 指定されたファイルパスが実行可能ファイルでないことを確認します
    /// </summary>
    /// <param name="filePath">チェックするファイルパス</param>
    /// <returns>実行可能ファイルでない場合はtrue</returns>
    public static bool IsNonExecutableFile(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            return false;

        return !DangerousFileExtensions.IsMatch(filePath);
    }

    /// <summary>
    /// アプリケーションが指定されたパスに対して読み取り権限を持っているか確認します
    /// </summary>
    /// <param name="path">確認するパス</param>
    /// <returns>読み取り権限がある場合はtrue</returns>
    public static bool HasReadPermission(string path)
    {
        try
        {
            // ディレクトリの場合
            if (Directory.Exists(path))
            {
                Directory.GetFiles(path, "*", SearchOption.TopDirectoryOnly);
                return true;
            }
            // ファイルの場合
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

    /// <summary>
    /// アプリケーションが指定されたパスに対して書き込み権限を持っているか確認します
    /// </summary>
    /// <param name="path">確認するパス</param>
    /// <returns>書き込み権限がある場合はtrue</returns>
    public static bool HasWritePermission(string path)
    {
        try
        {
            // ディレクトリの場合
            if (Directory.Exists(path))
            {
                string testFile = Path.Combine(path, $"write_test_{Guid.NewGuid()}.tmp");
                using (File.Create(testFile)) { }
                File.Delete(testFile);
                return true;
            }
            // ファイルの場合
            else if (File.Exists(path))
            {
                using var fs = File.Open(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                return true;
            }
            else
            {
                // ファイルが存在しない場合、親ディレクトリでチェック
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
