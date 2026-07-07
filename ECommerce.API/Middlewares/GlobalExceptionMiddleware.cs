using ECommerce.Application.Common.Constants;
using ECommerce.Application.Common.Results;
using FluentValidation;

namespace ECommerce.API.Middlewares
{
    public sealed class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Unhandled exception occured. TraceId: {TraceId}", context.TraceIdentifier);
                await HandleExceptionAsync(context, exception);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            Result result;

            switch (exception)
            {
                case ValidationException validationException:

                    context.Response.StatusCode = StatusCodes.Status400BadRequest;

                    result = Result.Validation(ErrorMessages.ValidationFailed, errors: validationException.Errors.Select(x => x.ErrorMessage).Distinct());
                    break;

                case KeyNotFoundException ex:

                    context.Response.StatusCode = StatusCodes.Status404NotFound;

                    result = Result.NotFound(ex.Message);
                    break;

                case UnauthorizedAccessException ex:

                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;

                    result = Result.Unauthorized(ex.Message);
                    break;

                default:
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;

                    result = Result.InternalError(ErrorMessages.UnexpectedError);
                    break;
            }
            await context.Response.WriteAsJsonAsync(result);
        }
    }
}
