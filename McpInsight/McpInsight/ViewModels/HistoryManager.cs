using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;

namespace McpInsight.ViewModels
{
    /// <summary>
    /// 履歴データ保存用クラス
    /// </summary>
    public class HistoryData
    {
        /// <summary>
        /// フォルダパス履歴
        /// </summary>
        public List<string> FolderPaths { get; set; } = new List<string>();

        /// <summary>
        /// サーバー引数履歴
        /// </summary>
        public List<string> ServerArguments { get; set; } = new List<string>();
    }

    /// <summary>
    /// 履歴管理クラス
    /// </summary>
    public class HistoryManager
    {
        private readonly string _historyFilePath;
        private readonly int _maxHistoryItems;

        /// <summary>
        /// フォルダパス履歴
        /// </summary>
        public ObservableCollection<string> FolderPathHistory { get; } = new ObservableCollection<string>();

        /// <summary>
        /// サーバー引数履歴
        /// </summary>
        public ObservableCollection<string> ServerArgumentsHistory { get; } = new ObservableCollection<string>();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="historyFilePath">履歴ファイルパス</param>
        /// <param name="maxHistoryItems">最大履歴アイテム数</param>
        public HistoryManager(string historyFilePath, int maxHistoryItems = 10)
        {
            _historyFilePath = historyFilePath ?? throw new ArgumentNullException(nameof(historyFilePath));
            _maxHistoryItems = maxHistoryItems > 0 ? maxHistoryItems : 10;
            LoadHistory();
        }

        /// <summary>
        /// 履歴を読み込む
        /// </summary>
        public void LoadHistory()
        {
            try
            {
                if (File.Exists(_historyFilePath))
                {
                    var historyJson = File.ReadAllText(_historyFilePath);
                    var history = JsonConvert.DeserializeObject<HistoryData>(historyJson);

                    if (history != null)
                    {
                        // フォルダパス履歴を読み込む
                        if (history.FolderPaths != null)
                        {
                            FolderPathHistory.Clear();
                            foreach (var path in history.FolderPaths)
                            {
                                // パスが存在する場合のみ追加
                                if (Directory.Exists(path))
                                {
                                    FolderPathHistory.Add(path);
                                }
                            }
                        }

                        // 引数履歴を読み込む
                        if (history.ServerArguments != null)
                        {
                            ServerArgumentsHistory.Clear();
                            foreach (var arg in history.ServerArguments)
                            {
                                ServerArgumentsHistory.Add(arg);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading history: {ex.Message}");
                // エラーがあっても処理を継続
            }
        }

        /// <summary>
        /// 履歴を保存
        /// </summary>
        public void SaveHistory()
        {
            try
            {
                var history = new HistoryData
                {
                    FolderPaths = new List<string>(FolderPathHistory),
                    ServerArguments = new List<string>(ServerArgumentsHistory)
                };

                var historyJson = JsonConvert.SerializeObject(history, Formatting.Indented);
                File.WriteAllText(_historyFilePath, historyJson);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error saving history: {ex.Message}");
                // エラーがあっても処理を継続
            }
        }

        /// <summary>
        /// フォルダパス履歴に追加
        /// </summary>
        /// <param name="path">フォルダパス</param>
        public void AddToFolderPathHistory(string path)
        {
            if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
            {
                return;
            }

            // すでに存在する場合は削除して先頭に追加
            if (FolderPathHistory.Contains(path))
            {
                FolderPathHistory.Remove(path);
            }

            // 先頭に追加
            FolderPathHistory.Insert(0, path);

            // 履歴は最大10個まで
            while (FolderPathHistory.Count > _maxHistoryItems)
            {
                FolderPathHistory.RemoveAt(FolderPathHistory.Count - 1);
            }

            // 履歴を保存
            SaveHistory();
        }

        /// <summary>
        /// 引数履歴に追加
        /// </summary>
        /// <param name="args">引数</param>
        public void AddToArgumentsHistory(string args)
        {
            if (string.IsNullOrWhiteSpace(args))
            {
                return;
            }

            // すでに存在する場合は削除して先頭に追加
            if (ServerArgumentsHistory.Contains(args))
            {
                ServerArgumentsHistory.Remove(args);
            }

            // 先頭に追加
            ServerArgumentsHistory.Insert(0, args);

            // 履歴は最大10個まで
            while (ServerArgumentsHistory.Count > _maxHistoryItems)
            {
                ServerArgumentsHistory.RemoveAt(ServerArgumentsHistory.Count - 1);
            }

            // 履歴を保存
            SaveHistory();
        }

        /// <summary>
        /// 最新のフォルダパスを取得
        /// </summary>
        /// <returns>最新のフォルダパス</returns>
        public string GetLatestFolderPath()
        {
            return FolderPathHistory.Count > 0 ? FolderPathHistory[0] : string.Empty;
        }

        /// <summary>
        /// 最新の引数を取得
        /// </summary>
        /// <returns>最新の引数</returns>
        public string GetLatestArguments()
        {
            return ServerArgumentsHistory.Count > 0 ? ServerArgumentsHistory[0] : string.Empty;
        }
    }
}
