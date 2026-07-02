namespace ECommerce.API.Responses
{
    public class ApiResponse<T>
    {
        public bool Success { get; init; }

        public string Message { get; init; } = string.Empty;

        public T? Data { get; init; }

        public string? ErrorCode { get; init; }

        public IReadOnlyList<string> Errors { get; init; } = Array.Empty<string>();
    }
}
