using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace McpInsight.Models
{
    /// <summary>
    /// MCPメソッド情報の共通インターフェース
    /// </summary>
    public interface IMcpMethodInfo
    {
        /// <summary>
        /// メソッド名
        /// </summary>
        string Name { get; }
        
        /// <summary>
        /// メソッドの説明
        /// </summary>
        string Description { get; }
        
        /// <summary>
        /// 実行ファイルのパス
        /// </summary>
        string ExePath { get; }
        
        /// <summary>
        /// 実行ファイル名
        /// </summary>
        string ExeName { get; }

        /// <summary>
        /// メソッドを実行
        /// </summary>
        /// <param name="jsonInput">JSONパラメータ</param>
        /// <returns>実行結果</returns>
        Task<string> ExecuteAsync(string jsonInput);

        /// <summary>
        /// JSONテンプレートを生成
        /// </summary>
        /// <returns>JSONテンプレート</returns>
        string GenerateJsonTemplate();
    }
}
