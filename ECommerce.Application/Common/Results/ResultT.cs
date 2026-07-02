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
    }
}
