using McpInsight.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace McpInsight.ViewModels
{
    /// <summary>
    /// MCPメソッド実行クラス
    /// </summary>
    public class McpMethodExecutor
    {
        private readonly IStatusReporter _statusReporter;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="statusReporter">ステータスレポーター</param>
        /// <param name="serverArguments">サーバー引数</param>
        public McpMethodExecutor(IStatusReporter statusReporter)
        {
            _statusReporter = statusReporter ?? throw new ArgumentNullException(nameof(statusReporter));
        }

        /// <summary>
        /// メソッドを実行
        /// </summary>
        /// <param name="methodInfo">実行するメソッド情報</param>
        /// <param name="jsonInput">JSONパラメータ</param>
        /// <returns>実行成功したかどうか</returns>
        public async Task<bool> ExecuteMethodAsync(IMcpMethodInfo methodInfo, string jsonInput)
        {
            if (methodInfo == null)
            {
                _statusReporter.SetErrorMessage("No method selected");
                return false;
            }

            try
            {
                _statusReporter.SetMethodResult(string.Empty);
                _statusReporter.SetErrorMessage(string.Empty);
                _statusReporter.SetStatusMessage($"Executing {methodInfo.Name}...");

                // MC標準プロトコルを使用して実行
                try
                {
                    string result = await methodInfo.ExecuteAsync(jsonInput);
                    if (!string.IsNullOrEmpty(result))
                    {
                        // 結果の解析とフォーマット
                        FormatAndSetResult(result);
                        _statusReporter.SetStatusMessage("Method executed successfully");
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    _statusReporter.SetErrorMessage($"MCP protocol execution failed: {ex.Message}");
                }

                return false;
            }
            catch (Exception ex)
            {
                _statusReporter.SetErrorMessage(ex.Message);
                _statusReporter.SetStatusMessage("Execution failed");
                Debug.WriteLine($"Execution error: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 結果をフォーマットして設定
        /// </summary>
        /// <param name="outputStr">出力文字列</param>
        private void FormatAndSetResult(string outputStr)
        {
            try
            {
                if (outputStr.StartsWith("{") || outputStr.StartsWith("["))
                {
                    // JSON出力の場合はフォーマットして表示
                    var json = JToken.Parse(outputStr);
                    _statusReporter.SetMethodResult(json.ToString(Formatting.Indented));
                }
                else
                {
                    // 通常のテキスト出力
                    _statusReporter.SetMethodResult(outputStr);
                }
            }
            catch
            {
                // JSON解析エラーの場合はそのまま表示
                _statusReporter.SetMethodResult(outputStr);
            }
        }
    }
}
