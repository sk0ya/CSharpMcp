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

        public ObservableCollection<string> FolderPathHistory => _historyManager.FolderPathHistory;

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

        public MainViewModel()
            : this(new string[0])
        {
        }

        public MainViewModel(string[] args)
        {
            string historyFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mcpinsight_history.json");
            _historyManager = new HistoryManager(historyFilePath);

            _commandLineProcessor = new CommandLineProcessor(this, args);
            _methodLoader = new McpMethodLoader(this);
            _methodExecutor = new McpMethodExecutor(this);

            InitializeFromHistory();

            if (args.Length > 0)
            {
                ProcessCommandLineArguments();
            }
        }

        #endregion

        #region Private Methods

        private void InitializeFromHistory()
        {
            if (_historyManager.FolderPathHistory.Count > 0)
            {
                FolderPath = _historyManager.FolderPathHistory[0];
            }

            if (_historyManager.ServerArgumentsHistory.Count > 0)
            {
                McpServerArguments = _historyManager.ServerArgumentsHistory[0];
            }
        }

        private void ProcessCommandLineArguments()
        {
            if (_commandLineProcessor.ProcessCommandLineArguments())
            {
                if (!string.IsNullOrEmpty(_commandLineProcessor.FolderPath))
                {
                    FolderPath = _commandLineProcessor.FolderPath;
                    _historyManager.AddToFolderPathHistory(FolderPath);
                }

                if (!string.IsNullOrEmpty(_commandLineProcessor.ServerArgs))
                {
                    McpServerArguments = _commandLineProcessor.ServerArgs;
                    _historyManager.AddToArgumentsHistory(McpServerArguments);
                }

                Task.Run(async () =>
                {
                    await LoadMcpMethodsAsync();
                    
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

        public void SetStatusMessage(string message)
        {
            StatusMessage = message;
        }

        public void SetErrorMessage(string errorMessage)
        {
            ErrorMessage = errorMessage;
        }

        public void SetMethodResult(string result)
        {
            MethodResult = result;
        }

        #endregion

        #region Commands

        [RelayCommand]
        private async Task BrowseFolder()
        {
            if (string.IsNullOrEmpty(FolderPath))
            {
                FolderPath = @"C:\Projects\MCPServer\CSharpMcpServer\CreateMcpServer";
            }
            else
            {
                if (!Directory.Exists(FolderPath))
                {
                    ErrorMessage = "Invalid folder path";
                    return;
                }
            }
            
            _historyManager.AddToFolderPathHistory(FolderPath);
            
            await LoadMcpMethodsAsync();
        }

        [RelayCommand]
        private async Task Dispose()
        {
            McpMethods.Clear();
            await _methodLoader.DisposeMcpMethodsAsync();
        }

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
                _historyManager.AddToArgumentsHistory(McpServerArguments);

                IsExecuting = true;
                MethodResult = "";
                ErrorMessage = "";
                StatusMessage = $"Executing {SelectedMethod.Name}...";

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
