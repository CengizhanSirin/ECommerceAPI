namespace ECommerce.Application.Common.Results
{
    public class Result
    {
        public bool IsSuccess { get; init; }

        public string Message { get; init; } = string.Empty;

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

        public static Result BadRequest( string message, IEnumerable<string>? errors = null)  
        {
            return Failure(message, ErrorCodes.BadRequest, errors);
        }

        public static Result Validation(string message,IEnumerable<string>? errors = null)
        {
            return Failure(message, ErrorCodes.ValidationError, errors);
        }

        public static Result NotFound(string message)
        {
            return Failure(message, ErrorCodes.NotFound);
        }

        public static Result Conflict(string message)
        {
            return Failure(message, ErrorCodes.Conflict);
        }

        public static Result Unauthorized(string message)
        {
            return Failure(message, ErrorCodes.Unauthorized);
        }

        public static Result Forbidden(string message)
        {
            return Failure(message, ErrorCodes.Forbidden);
        }

        public static Result InternalError(string message)
        {
            return Failure(message, ErrorCodes.InternalServerError);
        }
    }
}
