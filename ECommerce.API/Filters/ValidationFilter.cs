using ECommerce.Application.Common.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ECommerce.API.Filters
{
    public class ValidationFilter : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(
            ActionExecutingContext context,
            ActionExecutionDelegate next)
        {
            if (!context.ModelState.IsValid)
            {
                var errors = context.ModelState.Values
                             .SelectMany(x => x.Errors)
                             .Select(x => x.ErrorMessage)
                             .Distinct()
                             .ToArray();

                context.Result = new BadRequestObjectResult(
                    Result.Failure(
                        "Doğrulama hatası.",
                        errors: errors));

                return;
            }

            await next();
        }
    }
}
