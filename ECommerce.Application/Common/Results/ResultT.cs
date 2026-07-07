namespace ECommerce.Application.Common.Results
{
    public class ResultT<T>() : Result
    {
        public T? Data { get; private init; }

        public static ResultT<T> Success(T data, string message = "")
        {
            return new ResultT<T>
            {
                IsSuccess = true,
                Data = data,
                Message = message
            };
        }

        public new static ResultT<T> Failure(string message, string? errorCode = null, IEnumerable<string>? errors = null)
        {
            return new ResultT<T>
            {
                IsSuccess = false,
                Message = message,
                ErrorCode = errorCode,
                Errors = errors?.ToArray() ?? Array.Empty<string>(),
                Data = default
            };
        }

        public new static ResultT<T> BadRequest(  string message, IEnumerable<string>? errors = null)
        {
            return Failure(message, ErrorCodes.BadRequest, errors);
        }

        public new static ResultT<T> Validation(  string message, IEnumerable<string>? errors = null)
        {
            return Failure(message, ErrorCodes.ValidationError, errors);
        }

        public new static ResultT<T> NotFound(string message)
        {
            return Failure(message, ErrorCodes.NotFound);
        }

        public new static ResultT<T> Conflict(string message)
        {
            return Failure(message, ErrorCodes.Conflict);
        }

        public new static ResultT<T> Unauthorized(string message)
        {
            return Failure(message, ErrorCodes.Unauthorized);
        }

        public new static ResultT<T> Forbidden(string message)
        {
            return Failure(message, ErrorCodes.Forbidden);
        }

        public new static ResultT<T> InternalError(string message)
        {
            return Failure(message, ErrorCodes.InternalServerError);
        }
    }
}
