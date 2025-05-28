using System.Text.Json;

namespace FileSystem.Common;

/// <summary>
/// 例外処理のためのユーティリティクラス
/// </summary>
public static class ExceptionHandling
{
    /// <summary>
    /// 例外情報をJSON形式で返します
    /// </summary>
    /// <param name="ex">発生した例外</param>
    /// <param name="operation">実行しようとした操作の説明</param>
    /// <returns>例外情報を含むJSON文字列</returns>
    public static string FormatExceptionAsJson(Exception ex, string operation)
    {
        var errorInfo = new
        {
            Status = "Error",
            Operation = operation,
            Message = ex.Message,
            ExceptionType = ex.GetType().Name,
            StackTrace = ex.StackTrace,
            InnerException = ex.InnerException?.Message
        };

        return JsonSerializer.Serialize(errorInfo, new JsonSerializerOptions
        {
            WriteIndented = true
        });
    }

    /// <summary>
    /// 例外を処理して適切なレスポンスを返します
    /// </summary>
    /// <param name="ex">発生した例外</param>
    /// <param name="operation">実行しようとした操作の説明</param>
    /// <param name="logger">ロガーインスタンス（オプション）</param>
    /// <returns>エラーメッセージ</returns>
    public static string HandleException(Exception ex, string operation, ILogger logger = null)
    {
        // ロガーが指定されていれば例外をログに記録
        logger?.LogError(ex, $"Error during {operation}: {ex.Message}");

        if (ex is UnauthorizedAccessException)
        {
            return $"アクセス権限エラー: {ex.Message}";
        }
        else if (ex is FileNotFoundException)
        {
            return $"ファイルが見つかりません: {ex.Message}";
        }
        else if (ex is DirectoryNotFoundException)
        {
            return $"ディレクトリが見つかりません: {ex.Message}";
        }
        else if (ex is IOException)
        {
            return $"I/Oエラー: {ex.Message}";
        }
        else if (ex is ArgumentException)
        {
            return $"引数エラー: {ex.Message}";
        }
        else
        {
            return $"{operation}中にエラーが発生しました: {ex.Message}";
        }
    }
}

/// <summary>
/// ロギングのためのシンプルなインターフェース
/// </summary>
public interface ILogger
{
    void LogInformation(string message);
    void LogWarning(string message);
    void LogError(Exception ex, string message);
    void LogDebug(string message);
}
