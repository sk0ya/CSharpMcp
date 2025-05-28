// このファイルは廃止予定です。
// 新しいIMcpMethodInfoインターフェースとその実装クラスを使用してください。

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;
using Newtonsoft.Json.Linq;

namespace McpInsight.Models
{
    /// <summary>
    /// 互換性のために残しておく旧クラス
    /// </summary>
    [Obsolete("このクラスは廃止予定です。IMcpMethodInfoインターフェースとその実装クラスを使用してください。")]
    public class McpMethodInfo
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ExePath { get; set; } = string.Empty;
        public string ExeName { get; set; } = string.Empty;
        public string Parameters { get; set; } = string.Empty;

        public McpClientTool ClientTool { get; set; } = null;
        public McpClientPrompt ClientPrompt { get; set; } = null;

        // リストボックスに表示する文字列
        public override string ToString()
        {
            return $"{Name} [{ExeName}]";
        }

        /// <summary>
        /// 新しいインターフェース実装に変換
        /// </summary>
        /// <returns>IMcpMethodInfo実装</returns>
        public IMcpMethodInfo ToNewImplementation()
        {
            if (ClientTool != null)
            {
                return McpMethodFactory.CreateFromClientTool(ClientTool, ExePath, ExeName);
            }
            else if (ClientPrompt != null)
            {
                return McpMethodFactory.CreateFromClientPrompt(ClientPrompt, ExePath, ExeName);
            }
            
            throw new InvalidOperationException("McpMethodInfoにはClientToolかClientPromptのいずれかが必要です。");
        }
    }
}
