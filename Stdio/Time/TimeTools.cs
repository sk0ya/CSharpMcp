using System.ComponentModel;
using ModelContextProtocol.Server;

namespace Time;

[McpServerToolType]
public static class TimeTools
{
    [McpServerTool, Description("Gets the current system time and returns it as a formatted string.")]
    public static string GetCurrentTime()
    {
        return DateTime.Now.ToString("G");
    }
}
