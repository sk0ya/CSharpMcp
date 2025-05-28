namespace McpInsight.Models
{
    /// <summary>
    /// メソッド実行結果
    /// </summary>
    public class ExecutionResult
    {
        /// <summary>
        /// 実行結果
        /// </summary>
        public string Output { get; }

        /// <summary>
        /// エラーメッセージ
        /// </summary>
        public string ErrorMessage { get; }

        /// <summary>
        /// 成功したかどうか
        /// </summary>
        public bool IsSuccess => string.IsNullOrEmpty(ErrorMessage);

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="output">実行結果</param>
        private ExecutionResult(string output)
        {
            Output = output ?? string.Empty;
            ErrorMessage = string.Empty;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="errorMessage">エラーメッセージ</param>
        private ExecutionResult(string errorMessage, bool isError)
        {
            Output = string.Empty;
            ErrorMessage = errorMessage ?? "Unknown error";
        }

        /// <summary>
        /// 成功結果を作成
        /// </summary>
        /// <param name="output">実行結果</param>
        /// <returns>実行結果</returns>
        public static ExecutionResult Success(string output)
        {
            return new ExecutionResult(output);
        }

        /// <summary>
        /// 失敗結果を作成
        /// </summary>
        /// <param name="errorMessage">エラーメッセージ</param>
        /// <returns>実行結果</returns>
        public static ExecutionResult Error(string errorMessage)
        {
            return new ExecutionResult(errorMessage, true);
        }
    }
}
