using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace McpInsight.Models
{
    public interface IMcpMethodInfo
    {
        string Name { get; }
        
        string Description { get; }
        
        string ExePath { get; }
        
        string ExeName { get; }

        Task<string> ExecuteAsync(string jsonInput);

        string GenerateJsonTemplate();
    }
}
