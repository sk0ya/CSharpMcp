using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Text.Json;

namespace McpInsight.Models
{
    /// <summary>
    /// JSONテンプレート生成ユーティリティクラス
    /// </summary>
    public static class JsonTemplateGenerator
    {
        /// <summary>
        /// JsonSchemaからテンプレートを生成
        /// </summary>
        /// <param name="jsonSchema">JsonSchema</param>
        /// <returns>JSONテンプレート</returns>
        public static string GenerateFromJsonSchema(JsonElement jsonSchema)
        {
            try
            {
                if (jsonSchema.ValueKind == JsonValueKind.Undefined)
                {
                    return "{}";
                }

                // JsonElementからテンプレートを生成
                var template = new JObject();
                
                // propertiesを取得
                if (jsonSchema.TryGetProperty("properties", out var propertiesElement) && 
                    propertiesElement.ValueKind == JsonValueKind.Object)
                {
                    // propertiesの各プロパティに対して処理
                    foreach (var property in propertiesElement.EnumerateObject())
                    {
                        string propName = property.Name;
                        
                        // 型に基づいてデフォルト値を設定
                        if (property.Value.TryGetProperty("type", out var typeElement))
                        {
                            string type = typeElement.GetString() ?? "string";
                            
                            switch (type.ToLower())
                            {
                                case "string":
                                    template[propName] = "";
                                    break;
                                case "number":
                                case "integer":
                                    template[propName] = 0;
                                    break;
                                case "boolean":
                                    template[propName] = false;
                                    break;
                                case "array":
                                    template[propName] = new JArray();
                                    break;
                                case "object":
                                    template[propName] = new JObject();
                                    break;
                                default:
                                    template[propName] = null;
                                    break;
                            }
                        }
                        else
                        {
                            // 型が指定されていない場合はnull
                            template[propName] = null;
                        }
                    }
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
