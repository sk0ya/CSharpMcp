using McpInsight.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace McpInsight.ViewModels
{
    /// <summary>
    /// コマンドライン引数処理クラス
    /// </summary>
    public class CommandLineProcessor
    {
        private readonly IStatusReporter _statusReporter;
        private readonly string[] _args;

        /// <summary>
        /// 自動実行フラグ
        /// </summary>
        public bool AutoExecuted { get; private set; } = false;

        /// <summary>
        /// フォルダパス
        /// </summary>
        public string FolderPath { get; private set; } = string.Empty;

        /// <summary>
        /// メソッド名
        /// </summary>
        public string MethodName { get; private set; } = string.Empty;

        /// <summary>
        /// JSONパラメータ
        /// </summary>
        public string JsonParams { get; private set; } = "{}";

        /// <summary>
        /// サーバー引数
        /// </summary>
        public string ServerArgs { get; private set; } = string.Empty;

        /// <summary>
        /// 実行ファイルパス
        /// </summary>
        public string ExePath { get; private set; } = string.Empty;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="statusReporter">ステータスレポーター</param>
        /// <param name="args">コマンドライン引数</param>
        public CommandLineProcessor(IStatusReporter statusReporter, string[] args)
        {
            _statusReporter = statusReporter ?? throw new ArgumentNullException(nameof(statusReporter));
            _args = args ?? new string[0];
        }

        /// <summary>
        /// コマンドライン引数を処理
        /// </summary>
        /// <returns>処理結果</returns>
        public bool ProcessCommandLineArguments()
        {
            try
            {
                if (_args.Length == 0)
                {
                    return false;
                }

                // 引数解析
                for (int i = 0; i < _args.Length; i++)
                {
                    string arg = _args[i];

                    if (arg == "--exe" || arg == "-e")
                    {
                        if (i + 1 < _args.Length)
                        {
                            ExePath = _args[++i];
                        }
                    }
                    else if (arg == "--method" || arg == "-m")
                    {
                        if (i + 1 < _args.Length)
                        {
                            MethodName = _args[++i];
                        }
                    }
                    else if (arg == "--params" || arg == "-p")
                    {
                        if (i + 1 < _args.Length)
                        {
                            JsonParams = _args[++i];
                        }
                    }
                    else if (arg == "--args" || arg == "-a")
                    {
                        if (i + 1 < _args.Length)
                        {
                            ServerArgs = _args[++i];
                        }
                    }
                    else if (arg == "--folder" || arg == "-f")
                    {
                        if (i + 1 < _args.Length)
                        {
                            FolderPath = _args[++i];
                        }
                    }
                }

                // フォルダパスが設定されているかチェック
                if (!string.IsNullOrEmpty(FolderPath) && Directory.Exists(FolderPath))
                {
                    return true;
                }
                else if (!string.IsNullOrEmpty(ExePath) && File.Exists(ExePath))
                {
                    // 個別のEXEが指定された場合
                    FolderPath = Path.GetDirectoryName(ExePath) ?? string.Empty;
                    return !string.IsNullOrEmpty(FolderPath) && Directory.Exists(FolderPath);
                }

                return false;
            }
            catch (Exception ex)
            {
                _statusReporter.SetErrorMessage($"Error processing command line arguments: {ex.Message}");
                Debug.WriteLine($"Command line argument error: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 指定されたメソッドを自動実行
        /// </summary>
        /// <param name="methodInfos">メソッド情報のリスト</param>
        /// <param name="executor">メソッド実行クラス</param>
        /// <returns>選択されたメソッド情報</returns>
        public async Task<IMcpMethodInfo?> AutoExecuteMethodAsync(
            IEnumerable<IMcpMethodInfo> methodInfos, 
            McpMethodExecutor executor)
        {
            if (string.IsNullOrEmpty(MethodName) || !methodInfos.Any())
            {
                return null;
            }

            // メソッド名が指定されていればそれを選択
            IMcpMethodInfo? method = methodInfos.FirstOrDefault(m => 
                m.Name.Equals(MethodName, StringComparison.OrdinalIgnoreCase));

            if (method != null)
            {
                // 自動実行
                AutoExecuted = true;
                await executor.ExecuteMethodAsync(method, JsonParams);
            }

            return method;
        }

        /// <summary>
        /// 指定されたパスが有効かどうかを確認
        /// </summary>
        /// <param name="path">パス</param>
        /// <returns>有効かどうか</returns>
        public bool IsValidPath(string path)
        {
            return !string.IsNullOrEmpty(path) && (Directory.Exists(path) || File.Exists(path));
        }
    }
}
