using System.Text;

namespace FileSystem.Common;

public static class Constants
{
    public static readonly Encoding DefaultEncoding = new UTF8Encoding(false);
    
    public const long MaxFileSize = 10 * 1024 * 1024;

    public const int MaxTcpConnectionsToShow = 20;
}
