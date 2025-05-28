using McpInsight.Models;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol.Transport;
using ModelContextProtocol.Protocol.Types;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace McpInsight.ViewModels
{
    /// <summary>
    /// MCPメソッドローダークラス
    /// </summary>
    public class McpMethodLoader
    {
        private readonly IStatusReporter _statusReporter;
        private IMcpClient _client;
        
        /// <summary>
        /// ロードしたMCPメソッド
        /// </summary>
        public List<IMcpMethodInfo> McpMethods { get; } = new List<IMcpMethodInfo>();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="statusReporter">ステータスレポーター</param>
        /// <param name="mcpServerArguments">MCPサーバー引数</param>
        public McpMethodLoader(IStatusReporter statusReporter)
        {
            _statusReporter = statusReporter ?? throw new ArgumentNullException(nameof(statusReporter));
        }

        /// <summary>
        /// MCPメソッドを読み込む
        /// </summary>
        /// <param name="folderPath">フォルダパス</param>
        /// <returns>ロード結果</returns>
        public async Task<bool> LoadMcpMethodsAsync(string folderPath, string arguments)
        {
            McpMethods.Clear();
            _statusReporter.SetErrorMessage(string.Empty);
            _statusReporter.SetStatusMessage("Scanning for MCP methods...");
            _statusReporter.SetMethodResult(string.Empty);
            
            if (string.IsNullOrEmpty(folderPath) || !Directory.Exists(folderPath))
            {
                _statusReporter.SetErrorMessage("Invalid folder path");
                return false;
            }

            try
            {
                // MCPサーバーEXEを検索
                var exePaths = FindMcpServerExecutables(folderPath);
                
                if (!exePaths.Any())
                {
                    _statusReporter.SetErrorMessage("No EXE files found in the specified folder");
                    return false;
                }

                foreach (var exePath in exePaths)
                {
                    try
                    {
                        await ProcessSingleExeWithSdk(exePath, arguments);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error processing EXE {exePath}: {ex.Message}");
                    }
                }

                _statusReporter.SetStatusMessage($"Found {McpMethods.Count} MCP methods");
                return McpMethods.Count > 0;
            }
            catch (Exception ex)
            {
                _statusReporter.SetErrorMessage($"Error loading MCP methods: {ex.Message}");
                return false;
            }
        }

        public async Task DisposeMcpMethodsAsync()
        {
            McpMethods.Clear();
            _statusReporter.SetErrorMessage(string.Empty);
            _statusReporter.SetStatusMessage(string.Empty);
            _statusReporter.SetMethodResult(string.Empty);
            if (_client != null)
            {
                await _client.DisposeAsync();
            }
        }

        /// <summary>
        /// MCPサーバー実行ファイルを検索
        /// </summary>
        /// <param name="folderPath">フォルダパス</param>
        /// <returns>実行ファイルパスのリスト</returns>
        private List<string> FindMcpServerExecutables(string folderPath)
        {
            var exePaths = new List<string>();
            var binDebugFolder = Path.Combine(folderPath, "bin", "Debug");
            if (Directory.Exists(binDebugFolder))
            {
                var frameworkFolders = Directory.GetDirectories(binDebugFolder, "net*")
                                        .OrderByDescending(d => d).ToList();
                if (frameworkFolders.Any())
                {
                    exePaths.AddRange(Directory.GetFiles(frameworkFolders.First(), "*.exe"));
                }
            }
            return exePaths;
        }

        /// <summary>
        /// 単一の実行ファイルを処理
        /// </summary>
        /// <param name="exePath">実行ファイルパス</param>
        /// <returns>処理の結果</returns>
        private async Task ProcessSingleExeWithSdk(string exePath, string arguments)
        {
            try
            {
                if(_client != null)
                {
                    await _client.DisposeAsync();
                }

                _statusReporter.SetStatusMessage($"Connecting to {Path.GetFileName(exePath)} via MCP SDK...");
                
                // McpClientOptions を設定
                var options = new McpClientOptions
                {
                    ClientInfo = new Implementation
                    {
                        Name = "McpInsight",
                        Version = "1.0.0"
                    }
                };

                // トランスポートの設定
                var transportOptions = new StdioClientTransportOptions
                {
                    Name = Path.GetFileNameWithoutExtension(exePath),
                    Command = exePath,
                    // 引数がある場合は追加
                    Arguments = string.IsNullOrEmpty(arguments) 
                        ? new List<string>() 
                        : arguments.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList()
                };

                var transport = new StdioClientTransport(transportOptions);
                
                // クライアントの作成
                _client = await McpClientFactory.CreateAsync(transport, options);
                
                string exeName = Path.GetFileNameWithoutExtension(exePath);
                
                // Toolsを取得
                await LoadClientTools(exePath, exeName);
                
                // Promptsを取得
                await LoadClientPrompts(exePath, exeName);

                _statusReporter.SetStatusMessage($"Successfully loaded tools from {Path.GetFileName(exePath)}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SDK connection failed: {ex.Message}");
                throw; // 上位ハンドラーにエラーを転送して、フォールバック処理を実行
            }
        }

        /// <summary>
        /// ClientToolを読み込む
        /// </summary>
        /// <param name="exePath">実行ファイルパス</param>
        /// <param name="exeName">実行ファイル名</param>
        /// <returns>処理の結果</returns>
        private async Task LoadClientTools(string exePath, string exeName)
        {
            var tools = await _client.ListToolsAsync();
            
            if (tools != null && tools.Any())
            {
                foreach (var tool in tools)
                {
                    var methodInfo = McpMethodFactory.CreateFromClientTool(tool, exePath, exeName);
                    McpMethods.Add(methodInfo);
                }
            }
        }

        /// <summary>
        /// ClientPromptを読み込む
        /// </summary>
        /// <param name="exePath">実行ファイルパス</param>
        /// <param name="exeName">実行ファイル名</param>
        /// <returns>処理の結果</returns>
        private async Task LoadClientPrompts(string exePath, string exeName)
        {
            var prompts = await _client.ListPromptsAsync();
            if (prompts != null && prompts.Any())
            {
                foreach (var prompt in prompts)
                {
                    var methodInfo = McpMethodFactory.CreateFromClientPrompt(prompt, exePath, exeName);
                    McpMethods.Add(methodInfo);
                }
            }
        }
    }
}
