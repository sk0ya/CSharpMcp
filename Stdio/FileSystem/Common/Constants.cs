using System.Text;

namespace FileSystem.Common;

/// <summary>
/// アプリケーション全体で使用される定数を定義します
/// </summary>
public static class Constants
{
    /// <summary>
    /// ファイル操作のデフォルトエンコーディング
    /// </summary>
    public static readonly Encoding DefaultEncoding = Encoding.UTF8;
    
    /// <summary>
    /// ファイル読み込みの最大サイズ（10MB）
    /// </summary>
    public const long MaxFileSize = 10 * 1024 * 1024;

    /// <summary>
    /// TCPコネクション表示の最大数
    /// </summary>
    public const int MaxTcpConnectionsToShow = 20;
}
