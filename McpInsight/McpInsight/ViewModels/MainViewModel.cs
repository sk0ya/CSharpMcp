using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using McpInsight.Models;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace McpInsight.ViewModels
{
    /// <summary>
    /// メインビューモデル
    /// </summary>
    public partial class MainViewModel : ViewModelBase, IStatusReporter
    {
        private readonly HistoryManager _historyManager;
        private readonly McpMethodLoader _methodLoader;
        private readonly McpMethodExecutor _methodExecutor;
        private readonly CommandLineProcessor _commandLineProcessor;

        #region Observable Properties

        [ObservableProperty]
        private string _folderPath = "";

        [ObservableProperty]
        private ObservableCollection<IMcpMethodInfo> _mcpMethods = new();

        [ObservableProperty]
        private string _methodResult = "";

        [ObservableProperty]
        private bool _isExecuting;

        [ObservableProperty]
        private string _statusMessage = "";

        [ObservableProperty]
        private string _errorMessage = "";

        [ObservableProperty]
        private string _mcpServerArguments = "";

        /// <summary>
        /// フォルダパス履歴
        /// </summary>
        public ObservableCollection<string> FolderPathHistory => _historyManager.FolderPathHistory;

        /// <summary>
        /// サーバー引数履歴
        /// </summary>
        public ObservableCollection<string> McpServerArgumentsHistory => _historyManager.ServerArgumentsHistory;

        #endregion

        #region Properties with Custom Implementation

        private IMcpMethodInfo? _selectedMethod;
        public IMcpMethodInfo? SelectedMethod
        {
            get => _selectedMethod;
            set
            {
                if (SetProperty(ref _selectedMethod, value))
                {
                    // SelectedMethodが変更されたら自動的にテンプレートを生成
                    GenerateJsonTemplateForSelectedMethod();
                }
            }
        }

        private string _jsonInput = "{}";
        public string JsonInput
        {
            get => _jsonInput;
            set => SetProperty(ref _jsonInput, value);
        }

        #endregion

        #region Constructors

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MainViewModel()
            : this(new string[0])
        {
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="args">コマンドライン引数</param>
        public MainViewModel(string[] args)
        {
            // 履歴マネージャーの初期化
            string historyFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mcpinsight_history.json");
            _historyManager = new HistoryManager(historyFilePath);

            // ビューモデルの初期化
            _commandLineProcessor = new CommandLineProcessor(this, args);
            _methodLoader = new McpMethodLoader(this);
            _methodExecutor = new McpMethodExecutor(this);

            // 履歴からデータを復元
            InitializeFromHistory();

            // コマンドライン引数の処理
            if (args.Length > 0)
            {
                ProcessCommandLineArguments();
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// 履歴からデータを初期化
        /// </summary>
        private void InitializeFromHistory()
        {
            // フォルダパスの設定
            if (_historyManager.FolderPathHistory.Count > 0)
            {
                FolderPath = _historyManager.FolderPathHistory[0];
            }

            // サーバー引数の設定
            if (_historyManager.ServerArgumentsHistory.Count > 0)
            {
                McpServerArguments = _historyManager.ServerArgumentsHistory[0];
            }
        }

        /// <summary>
        /// コマンドライン引数を処理
        /// </summary>
        private void ProcessCommandLineArguments()
        {
            if (_commandLineProcessor.ProcessCommandLineArguments())
            {
                // フォルダパスを設定
                if (!string.IsNullOrEmpty(_commandLineProcessor.FolderPath))
                {
                    FolderPath = _commandLineProcessor.FolderPath;
                    _historyManager.AddToFolderPathHistory(FolderPath);
                }

                // サーバー引数を設定
                if (!string.IsNullOrEmpty(_commandLineProcessor.ServerArgs))
                {
                    McpServerArguments = _commandLineProcessor.ServerArgs;
                    _historyManager.AddToArgumentsHistory(McpServerArguments);
                }

                // メソッドをロード
                Task.Run(async () =>
                {
                    await LoadMcpMethodsAsync();
                    
                    // メソッド名が指定され、ロードが完了したら自動実行
                    if (!string.IsNullOrEmpty(_commandLineProcessor.MethodName))
                    {
                        var method = await _commandLineProcessor.AutoExecuteMethodAsync(McpMethods, _methodExecutor);
                        if (method != null)
                        {
                            SelectedMethod = method;
                            JsonInput = _commandLineProcessor.JsonParams;
                        }
                    }
                });
            }
        }

        /// <summary>
        /// 選択されたメソッドのJSONテンプレートを生成
        /// </summary>
        private void GenerateJsonTemplateForSelectedMethod()
        {
            if (SelectedMethod == null)
            {
                JsonInput = "{}";
                return;
            }

            try
            {
                JsonInput = SelectedMethod.GenerateJsonTemplate();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error generating template: {ex.Message}");
                JsonInput = "{}";
            }
        }

        #endregion

        #region IStatusReporter Implementation

        /// <summary>
        /// ステータスメッセージを設定
        /// </summary>
        /// <param name="message">メッセージ</param>
        public void SetStatusMessage(string message)
        {
            StatusMessage = message;
        }

        /// <summary>
        /// エラーメッセージを設定
        /// </summary>
        /// <param name="errorMessage">エラーメッセージ</param>
        public void SetErrorMessage(string errorMessage)
        {
            ErrorMessage = errorMessage;
        }

        /// <summary>
        /// 実行結果を設定
        /// </summary>
        /// <param name="result">実行結果</param>
        public void SetMethodResult(string result)
        {
            MethodResult = result;
        }

        #endregion

        #region Commands

        /// <summary>
        /// フォルダ選択コマンド
        /// </summary>
        [RelayCommand]
        private async Task BrowseFolder()
        {
            // In a real application, this would use a folder picker dialog
            if (string.IsNullOrEmpty(FolderPath))
            {
                FolderPath = @"C:\Projects\MCPServer\CSharpMcpServer\CreateMcpServer";
            }
            else
            {
                // Make sure the folder path is valid
                if (!Directory.Exists(FolderPath))
                {
                    ErrorMessage = "Invalid folder path";
                    return;
                }
            }
            
            // 履歴に追加
            _historyManager.AddToFolderPathHistory(FolderPath);
            
            await LoadMcpMethodsAsync();
        }

        [RelayCommand]
        private async Task Dispose()
        {
            McpMethods.Clear();
            await _methodLoader.DisposeMcpMethodsAsync();
        }

        /// <summary>
        /// フォルダをエクスプローラーで開くコマンド
        /// </summary>
        [RelayCommand]
        private void OpenFolderInExplorer()
        {
            if (string.IsNullOrEmpty(FolderPath) || !Directory.Exists(FolderPath))
            {
                ErrorMessage = "Invalid folder path";
                return;
            }

            try
            {
                Process.Start("explorer.exe", FolderPath);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error opening folder: {ex.Message}";
                Debug.WriteLine($"Explorer error: {ex.Message}");
            }
        }

        /// <summary>
        /// メソッド実行コマンド
        /// </summary>
        [RelayCommand]
        private async Task ExecuteMethod()
        {
            if (SelectedMethod == null)
            {
                ErrorMessage = "No method selected";
                return;
            }

            try
            {
                // 引数を履歴に追加
                _historyManager.AddToArgumentsHistory(McpServerArguments);

                IsExecuting = true;
                MethodResult = "";
                ErrorMessage = "";
                StatusMessage = $"Executing {SelectedMethod.Name}...";

                // メソッド実行
                await _methodExecutor.ExecuteMethodAsync(SelectedMethod, JsonInput);
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                StatusMessage = "Execution failed";
                Debug.WriteLine($"Execution error: {ex.Message}");
            }
            finally
            {
                IsExecuting = false;
            }
        }

        /// <summary>
        /// MCPメソッドを読み込む
        /// </summary>
        /// <returns>ロード結果</returns>
        private async Task<bool> LoadMcpMethodsAsync()
        {
            McpMethods.Clear();
            bool result = await _methodLoader.LoadMcpMethodsAsync(FolderPath, McpServerArguments);
            if (result)
            {
                foreach (var method in _methodLoader.McpMethods)
                {
                    McpMethods.Add(method);
                }
            }
            return result;
        }

        #endregion
    }
}
