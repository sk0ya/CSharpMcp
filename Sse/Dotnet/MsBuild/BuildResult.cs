
namespace MsBuild;

public class BuildResult
{
    public bool Success { get; set; }
    public string Output { get; set; }
    public string ErrorOutput { get; set; }
    public int ExitCode { get; set; }
}