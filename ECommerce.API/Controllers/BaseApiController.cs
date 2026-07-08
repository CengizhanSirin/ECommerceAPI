using ECommerce.Application.Common.Results;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public abstract class BaseApiController : ControllerBase
    {
        protected IActionResult HandleResult(Result result)
        {
            return result.IsSuccess
                ? Ok(result)
                : HandleFailure(result);
        }

        protected IActionResult HandleResult<T>(ResultT<T> result)
        {
            return result.IsSuccess
                ? Ok(result)
                : HandleFailure(result);
        }

        protected IActionResult HandleCreatedResult<T>(ResultT<T> result, string actionName, Func<T, object> routeValuesFactory)
        {
            if (!result.IsSuccess)
                return HandleFailure(result);

                return CreatedAtAction(actionName, routeValuesFactory(result.Data!), result);  
        }

        protected IActionResult HandleNoContent(Result result)
        {
            return result.IsSuccess
                ? NoContent()
                : HandleFailure(result);
        }

        private IActionResult HandleFailure(Result result)
        {
            return result.ErrorCode switch
            {
                ErrorCodes.NotFound
                    => NotFound(result),

                ErrorCodes.BadRequest
                    => BadRequest(result),

                ErrorCodes.ValidationError
                    => BadRequest(result),

                ErrorCodes.Unauthorized
                    => Unauthorized(result),

                ErrorCodes.Forbidden
                    => StatusCode(StatusCodes.Status403Forbidden, result),

                ErrorCodes.Conflict
                    => Conflict(result),

                ErrorCodes.InternalServerError
                    => StatusCode(StatusCodes.Status500InternalServerError, result),

                _
                    => StatusCode(StatusCodes.Status500InternalServerError, result)
            };
        }
    }
}
