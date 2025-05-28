using ModelContextProtocol.Client;
using System;

namespace McpInsight.Models
{
    /// <summary>
    /// MCPメソッド情報のファクトリークラス
    /// </summary>
    public static class McpMethodFactory
    {
        /// <summary>
        /// MCPメソッド情報を作成
        /// </summary>
        /// <param name="clientTool">ClientTool</param>
        /// <param name="exePath">実行ファイルのパス</param>
        /// <param name="exeName">実行ファイル名</param>
        /// <returns>MCPメソッド情報</returns>
        public static IMcpMethodInfo CreateFromClientTool(McpClientTool clientTool, string exePath, string exeName)
        {
            if (clientTool == null)
            {
                throw new ArgumentNullException(nameof(clientTool));
            }

            var methodInfo = new McpClientToolInfo(clientTool)
            {
                ExePath = exePath,
                ExeName = exeName
            };

            return methodInfo;
        }

        /// <summary>
        /// MCPメソッド情報を作成
        /// </summary>
        /// <param name="clientPrompt">ClientPrompt</param>
        /// <param name="exePath">実行ファイルのパス</param>
        /// <param name="exeName">実行ファイル名</param>
        /// <returns>MCPメソッド情報</returns>
        public static IMcpMethodInfo CreateFromClientPrompt(McpClientPrompt clientPrompt, string exePath, string exeName)
        {
            if (clientPrompt == null)
            {
                throw new ArgumentNullException(nameof(clientPrompt));
            }

            var methodInfo = new McpClientPromptInfo(clientPrompt)
            {
                ExePath = exePath,
                ExeName = exeName
            };

            return methodInfo;
        }
    }
}
