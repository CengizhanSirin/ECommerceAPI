namespace ECommerce.Application.Common.Results
{
    public class Result
    {
        public bool IsSuccess { get; set; }

        public string Message { get; set; } = string.Empty;

        public string? ErrorCode { get; protected init; }

        public IReadOnlyList<string> Errors { get; protected init; } = Array.Empty<string>();

        public static Result Success(string message = "")
        {
            return new Result
            {
                IsSuccess = true,
                Message = message
            };
        }

        public static Result Failure(string message,string? errorCode = null,IEnumerable<string>? errors = null)     
        {
            return new Result
            {
                IsSuccess = false,
                Message = message,
                ErrorCode = errorCode,
                Errors = errors?.ToArray() ?? Array.Empty<string>()
            };
        }
    }
}
