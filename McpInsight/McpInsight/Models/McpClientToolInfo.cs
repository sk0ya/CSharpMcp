using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace McpInsight.Models
{
    /// <summary>
    /// ClientTool用のMCPメソッド情報
    /// </summary>
    public class McpClientToolInfo : McpMethodInfoBase
    {
        /// <summary>
        /// ClientTool
        /// </summary>
        public McpClientTool ClientTool { get; }

        /// <summary>
        /// パラメータ
        /// </summary>
        public string Parameters { get; set; } = string.Empty;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="clientTool">ClientTool</param>
        public McpClientToolInfo(McpClientTool clientTool)
        {
            ClientTool = clientTool ?? throw new ArgumentNullException(nameof(clientTool));
            Name = clientTool.Name;
            Description = clientTool.Description ?? string.Empty;
            Parameters = clientTool.AdditionalProperties?.ToString() ?? string.Empty;
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
                var jsonObject = JsonConvert.DeserializeObject<Dictionary<string, object?>>(jsonInput);
                if (jsonObject == null)
                {
                    throw new JsonException("Invalid JSON input");
                }

                // 引数を作成
                var arguments = new AIFunctionArguments();
                foreach (var item in jsonObject)
                {
                    arguments.Add(item.Key, item.Value);
                }

                // 実行
                var result = await ClientTool.InvokeAsync(arguments);
                return result?.ToString() ?? string.Empty;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error executing ClientTool: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// JSONテンプレートを生成
        /// </summary>
        /// <returns>JSONテンプレート</returns>
        public override string GenerateJsonTemplate()
        {
            return JsonTemplateGenerator.GenerateFromJsonSchema(ClientTool.JsonSchema);
        }
    }
}
