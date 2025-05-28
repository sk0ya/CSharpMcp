using ModelContextProtocol.Server;
using System.ComponentModel;

namespace CreateMcpServer
{
    [McpServerPromptType]
    [Description("MCP Serverプロジェクト作成用のプロンプトを提供するクラス")]
    public class CreateMcpServerPrompts
    {
        [McpServerPrompt, Description("新しいMCPサーバープロジェクトを作成するためのプロンプト")]
        public static string CreateMcpServerProjectPrompt(string feature)
        {
            var featureToolsPath = Path.Combine(CreateMcpServerPath.RootFolderPath, feature, $"{feature}Tools.cs");
            return $"""
            McpServerProjectを作成するためのプロンプトです。
            以下の手順を守ってください。
            
            1.  CreateMCPServerproject で '{feature}' についてのプロジェクトを作成します。.

            2. {featureToolsPath} に、{feature}に基づいた機能を実装する。(ファイルを変更する)
            public メソッドにはコメントはつけず、Description属性をつける。
            Descriptionはできるだけ具体的に書くこと。
            private メソッドにはコメントは不要。Description属性も不要。
            """;
        }

        [McpServerPrompt, Description("プロジェクトのREADME.mdファイルを更新するためのプロンプト")]
        public static string UpdateReadMePrompt(string feature)
        {
            var featureToolsPath = Path.Combine(CreateMcpServerPath.RootFolderPath, "Servers",feature, $"{feature}Tools.cs");
            var featureReadMePath = Path.Combine(CreateMcpServerPath.RootFolderPath, "Servers", feature, $"README.md");
            var createMcpServerToolsPath = Path.Combine(CreateMcpServerPath.RootFolderPath,  nameof(CreateMcpServer), $"{nameof(CreateMcpServerTools)}.cs");

            var rootReadMe = Path.Combine(CreateMcpServerPath.RootFolderPath,   $"README.md");

            var createMcpServerReadMePath = Path.Combine(CreateMcpServerPath.RootFolderPath, nameof(CreateMcpServer), $"README.md");

            return $"""
            Update the README.md file in the {feature} project.
            {featureReadMePath} に、{feature}に基づいた機能を実装する。

            ---
            {feature}の ReadMe.mdは以下のファイルを参考にする
            {createMcpServerToolsPath}
            {createMcpServerReadMePath}

            ---
            {feature}のReadMeの更新後、
            {rootReadMe} に{feature} の追加もしくは更新する。
            """;
        }
    }
}
