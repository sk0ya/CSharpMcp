using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Text.Json;

namespace McpInsight.Models
{
    public static class JsonTemplateGenerator
    {
        public static string GenerateFromJsonSchema(JsonElement jsonSchema)
        {
            try
            {
                if (jsonSchema.ValueKind == JsonValueKind.Undefined)
                {
                    return "{}";
                }

                var template = new JObject();
                
                if (jsonSchema.TryGetProperty("properties", out var propertiesElement) && 
                    propertiesElement.ValueKind == JsonValueKind.Object)
                {
                    foreach (var property in propertiesElement.EnumerateObject())
                    {
                        string propName = property.Name;
                        
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
