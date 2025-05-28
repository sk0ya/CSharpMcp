using System;
using System.Threading.Tasks;

namespace McpInsight.Models
{
    /// <summary>
    /// MCPメソッド情報の基底クラス
    /// </summary>
    public abstract class McpMethodInfoBase : IMcpMethodInfo
    {
        /// <summary>
        /// メソッド名
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// メソッドの説明
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// 実行ファイルのパス
        /// </summary>
        public string ExePath { get; set; } = string.Empty;

        /// <summary>
        /// 実行ファイル名
        /// </summary>
        public string ExeName { get; set; } = string.Empty;

        /// <summary>
        /// リストボックスに表示する文字列
        /// </summary>
        public override string ToString()
        {
            return $"{Name} [{ExeName}]";
        }

        /// <summary>
        /// メソッドを実行
        /// </summary>
        /// <param name="jsonInput">JSONパラメータ</param>
        /// <returns>実行結果</returns>
        public abstract Task<string> ExecuteAsync(string jsonInput);

        /// <summary>
        /// JSONテンプレートを生成
        /// </summary>
        /// <returns>JSONテンプレート</returns>
        public abstract string GenerateJsonTemplate();
    }
}
