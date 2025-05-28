using System.Text.Json;

namespace FileSystem.Common;

public static class ExceptionHandling
{
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

    public static string HandleException(Exception ex, string operation, ILogger logger = null)
    {
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

public interface ILogger
{
    void LogInformation(string message);
    void LogWarning(string message);
    void LogError(Exception ex, string message);
    void LogDebug(string message);
}
