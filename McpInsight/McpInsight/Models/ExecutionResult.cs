namespace McpInsight.Models
{
    public class ExecutionResult
    {
        public string Output { get; }

        public string ErrorMessage { get; }

        public bool IsSuccess => string.IsNullOrEmpty(ErrorMessage);

        private ExecutionResult(string output)
        {
            Output = output ?? string.Empty;
            ErrorMessage = string.Empty;
        }

        private ExecutionResult(string errorMessage, bool isError)
        {
            Output = string.Empty;
            ErrorMessage = errorMessage ?? "Unknown error";
        }

        public static ExecutionResult Success(string output)
        {
            return new ExecutionResult(output);
        }

        public static ExecutionResult Error(string errorMessage)
        {
            return new ExecutionResult(errorMessage, true);
        }
    }
}
