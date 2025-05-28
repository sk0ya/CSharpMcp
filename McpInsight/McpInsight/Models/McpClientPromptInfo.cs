using ModelContextProtocol.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace McpInsight.Models
{
    /// <summary>
    /// ClientPrompt用のMCPメソッド情報
    /// </summary>
    public class McpClientPromptInfo : McpMethodInfoBase
    {
        /// <summary>
        /// ClientPrompt
        /// </summary>
        public McpClientPrompt ClientPrompt { get; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="clientPrompt">ClientPrompt</param>
        public McpClientPromptInfo(McpClientPrompt clientPrompt)
        {
            ClientPrompt = clientPrompt ?? throw new ArgumentNullException(nameof(clientPrompt));
            Name = clientPrompt.Name;
            Description = clientPrompt.Description ?? string.Empty;
        }

        /// <summary>
        /// メソッドを実行
        /// </summary>
        /// <param name="jsonInput">JSONパラメータ</param>
        /// <returns>実行結果</returns>
        public override async Task<string> ExecuteAsync(string jsonInput)
        {
            if (string.IsNullOrEmpty(jsonInput))
            {
                return string.Empty;
            }

            try
            {
                // JSONをパース
                var keyValue = JsonConvert.DeserializeObject<Dictionary<string, object?>>(jsonInput);
                if (keyValue == null)
                {
                    throw new JsonException("Invalid JSON input");
                }

                // 実行
                var task = await ClientPrompt.GetAsync(keyValue);
                
                // 結果を文字列として結合
                string result = string.Join("\n", task.Messages.Select(m => m.Content.Text));
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error executing ClientPrompt: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// JSONテンプレートを生成
        /// </summary>
        /// <returns>JSONテンプレート</returns>
        public override string GenerateJsonTemplate()
        {
            try
            {
                var arguments = ClientPrompt.ProtocolPrompt.Arguments;
                var template = new JObject();
                
                foreach (var argument in arguments)
                {
                    var propName = argument.Name;
                    template[propName] = "";
                }
                
                return template.ToString(Formatting.Indented);
            }
            catch (Exception)
            {
                return "{}";
            }
        }
    }
}
