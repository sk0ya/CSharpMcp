namespace McpInsight.ViewModels
{
    /// <summary>
    /// ステータス報告インターフェース
    /// </summary>
    public interface IStatusReporter
    {
        /// <summary>
        /// ステータスメッセージを設定
        /// </summary>
        /// <param name="message">メッセージ</param>
        void SetStatusMessage(string message);

        /// <summary>
        /// エラーメッセージを設定
        /// </summary>
        /// <param name="errorMessage">エラーメッセージ</param>
        void SetErrorMessage(string errorMessage);

        /// <summary>
        /// 実行結果を設定
        /// </summary>
        /// <param name="result">実行結果</param>
        void SetMethodResult(string result);
    }
}
