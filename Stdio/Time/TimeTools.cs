using ModelContextProtocol.Server;
using System.ComponentModel;

namespace Time.Tools;

[McpServerToolType]
public static class TimeTools
{
    [McpServerTool, Description("Gets the current system time and returns it as a formatted string.")]
    public static string GetCurrentTime()
    {
        return DateTime.Now.ToString("G");
    }
}
