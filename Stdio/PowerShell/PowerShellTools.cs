using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;

namespace PowerShellTools
{
    [McpServerToolType]
    public static class PowerShellTools
    {
        private static List<string> _allowedCommands;

        private static List<string> GetAllowedCommands()
        {
            if (_allowedCommands == null)
            {
                // 埋め込みリソースからJSONを読み込む
                var assembly = Assembly.GetExecutingAssembly();
                using (var stream = assembly.GetManifestResourceStream("PowerShellTools.Resources.allowed_commands.json"))
                {
                    if (stream == null)
                    {
                        // デフォルトのコマンドリストを使用
                        _allowedCommands = new List<string>
                            {
                                "Get-Process", "Get-Service", "Get-Item", "Get-ChildItem", "Get-Content",
                                "Select-Object", "Where-Object", "ForEach-Object", "Sort-Object",
                                "Format-List", "Format-Table", "Out-String"
                            };
                    }
                    else
                    {
                        // リソースからJSONを読み込む
                        using (var reader = new StreamReader(stream))
                        {
                            string json = reader.ReadToEnd();
                            var config = JsonConvert.DeserializeObject<AllowedCommandsConfig>(json);
                            _allowedCommands = config.AllowedCommands;
                        }
                    }
                }
            }
            return _allowedCommands;
        }

        [McpServerTool, Description("利用可能なPowerShellコマンド一覧を取得します")]
        public static string GetAvailableCommands()
        {
            var allowedCommands = GetAllowedCommands();
            return JsonConvert.SerializeObject(new { AllowedCommands = allowedCommands }, Formatting.Indented);
        }

        [McpServerTool, Description("PowerShellコマンドを安全に実行します")]
        public static string ExecuteCommand(string command, string parameters = "{}")
        {
            var paramDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(parameters);
            return ExecutePowerShellCommand(command, paramDict);
        }

        private static string ExecutePowerShellCommand(string command, Dictionary<string, object> parameters)
        {
            if (string.IsNullOrEmpty(command))
            {
                return "コマンドが指定されていません。";
            }

            if (!IsCommandAllowed(command))
            {
                return $"コマンド '{command}' は許可されていません。";
            }

            // パラメータの準備
            var sb = new StringBuilder();
            sb.Append(command);

            if (parameters != null && parameters.Count > 0)
            {
                foreach (var param in parameters)
                {
                    if (param.Value != null)
                    {
                        sb.Append($" -{param.Key} {FormatParameterValue(param.Value)}");
                    }
                    else
                    {
                        sb.Append($" -{param.Key}");
                    }
                }
            }

            return ExecuteScript(sb.ToString(), null);
        }

        private static string FormatParameterValue(object value)
        {
            if (value is string str)
            {
                return $"'{str.Replace("'", "''")}'";
            }
            else if (value is Array arr)
            {
                var items = new List<string>();
                foreach (var item in arr)
                {
                    items.Add(FormatParameterValue(item));
                }
                return $"@({string.Join(",", items)})";
            }
            return value.ToString();
        }

        private static string ExecuteScript(string script, string[] args)
        {
            try
            {
                using (var ps = PowerShell.Create())
                {
                    // 制約付き言語モードを設定
                    ps.AddScript("$ExecutionContext.SessionState.LanguageMode = 'ConstrainedLanguage'").Invoke();
                    
                    // 許可されたコマンドのみを使用できるようにJEAセッション設定を構成
                    var allowedCmdlets = GetAllowedCommands();
                    var iss = InitialSessionState.CreateDefault();
                    iss.LanguageMode = PSLanguageMode.ConstrainedLanguage;
                    
                    // 許可コマンドのセットアップ
                    var commandVisibility = new Dictionary<string, bool>();
                    foreach (var cmdlet in allowedCmdlets)
                    {
                        commandVisibility[cmdlet] = true;
                    }
                    
                    // JEAセッション構成の適用
                    ps.Runspace.SessionStateProxy.SetVariable("__PSLockdownPolicy", 4);
                    ps.Runspace.SessionStateProxy.SetVariable("__PSLockdownCommands", commandVisibility);
                    
                    // 許可されたコマンドのみ実行可能にする
                    ps.AddScript(@"
                        function Test-CommandAllowed {
                            param([string]$CommandName)
                            $allowedCommands = $__PSLockdownCommands.Keys
                            return $allowedCommands -contains $CommandName
                        }
                        
                        $ExecutionContext.SessionState.InvokeCommand.CommandNotFoundAction = {
                            param($CommandName, $CommandLookupEventArgs)
                            if (-not (Test-CommandAllowed -CommandName $CommandName)) {
                                $CommandLookupEventArgs.CommandScriptBlock = { 
                                    Write-Error ""コマンド '$CommandName' は許可されていません。""
                                }
                            }
                        }
                    ").Invoke();
                    
                    // メインスクリプトの実行
                    ps.Commands.Clear();
                    ps.AddScript(script);
                    
                    if (args != null && args.Length > 0)
                    {
                        ps.AddParameter("urls", args);
                    }
                    
                    // 結果を文字列として取得
                    ps.Commands.AddCommand("Out-String");
                    var results = ps.Invoke();
                    
                    if (results.Count > 0)
                    {
                        StringBuilder stringBuilder = new StringBuilder();
                        foreach (var result in results)
                        {
                            stringBuilder.Append(result);
                        }
                        return stringBuilder.ToString();
                    }
                    else
                    {
                        // エラーがあれば取得して返す
                        if (ps.Streams.Error.Count > 0)
                        {
                            StringBuilder errorBuilder = new StringBuilder();
                            foreach (var error in ps.Streams.Error)
                            {
                                errorBuilder.AppendLine(error.ToString());
                            }
                            return $"エラーが発生しました: {errorBuilder}";
                        }
                        
                        return "コマンドは正常に実行されましたが、出力はありません。";
                    }
                }
            }
            catch (Exception ex)
            {
                return $"エラーが発生しました: {ex.Message}";
            }
        }

        private static bool IsCommandAllowed(string commandInput)
        {
            var allowedCommands = GetAllowedCommands();
            
            // パイプラインの処理
            var pipelineCommands = commandInput.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var pipeCommand in pipelineCommands)
            {
                var trimmedCommand = pipeCommand.Trim();
                
                // コマンドの基本名を抽出（パラメータ等を除く）
                string commandName;
                int spaceIndex = trimmedCommand.IndexOf(' ');
                if (spaceIndex > 0)
                {
                    commandName = trimmedCommand.Substring(0, spaceIndex);
                }
                else
                {
                    commandName = trimmedCommand;
                }

                if (!allowedCommands.Contains(commandName))
                {
                    return false;
                }
            }
            
            return true;
        }
    }

    internal class AllowedCommandsConfig
    {
        public List<string> AllowedCommands { get; set; }
    }
}